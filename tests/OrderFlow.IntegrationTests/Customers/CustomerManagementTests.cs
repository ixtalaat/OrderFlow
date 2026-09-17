using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Customers;

public class CustomerManagementTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SalesEmployee_Can_Create_Customer()
    {
        // Arrange
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.SalesEmployee);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateCustomerRequest(
            "Alice Smith",
            $"alice-{Guid.NewGuid()}@test.com",
            "Password@123",
            "+1234567890",
            "123 Main St, Springfield");

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        createdCustomer.Should().NotBeNull();
        createdCustomer!.FullName.Should().Be("Alice Smith");
        createdCustomer.Email.Should().Be(request.Email);
        createdCustomer.PhoneNumber.Should().Be("+1234567890");
        createdCustomer.Address.Should().Be("123 Main St, Springfield");
        createdCustomer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SalesEmployee_Can_Get_Customer_By_Id()
    {
        // Arrange
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.SalesEmployee);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateCustomerRequest(
            "Bob Jones",
            $"bob-{Guid.NewGuid()}@test.com",
            "Password@123",
            "+9876543210",
            "456 Elm St");

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var getResponse = await _client.GetAsync($"/api/customers/{created!.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(created.Id);
        customer.FullName.Should().Be("Bob Jones");
    }

    [Fact]
    public async Task SalesEmployee_Can_List_And_Filter_Customers()
    {
        // Arrange
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.SalesEmployee);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var uniquePrefix = Guid.NewGuid().ToString("N")[..8];
        var createRequest1 = new CreateCustomerRequest($"SearchTarget-{uniquePrefix}", $"search1-{uniquePrefix}@test.com", "Password@123", "+1001", "Address 1");
        var createRequest2 = new CreateCustomerRequest($"OtherUser-{uniquePrefix}", $"search2-{uniquePrefix}@test.com", "Password@123", "+1002", "Address 2");

        (await _client.PostAsJsonAsync("/api/customers", createRequest1)).EnsureSuccessStatusCode();
        (await _client.PostAsJsonAsync("/api/customers", createRequest2)).EnsureSuccessStatusCode();

        // Act - Search
        var searchResponse = await _client.GetAsync($"/api/customers?searchTerm={uniquePrefix}&pageNumber=1&pageSize=10");

        // Assert
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedList = await searchResponse.Content.ReadFromJsonAsync<PagedList<CustomerResponse>>();
        pagedList.Should().NotBeNull();
        pagedList!.Items.Should().HaveCount(2);
        pagedList.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SalesEmployee_Can_Update_Customer()
    {
        // Arrange
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.SalesEmployee);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateCustomerRequest(
            "Original Name",
            $"orig-{Guid.NewGuid()}@test.com",
            "Password@123",
            "+1111111111",
            "Original Address");

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var updateRequest = new UpdateCustomerRequest(
            "Updated Name",
            "+9999999999",
            "Updated Address");

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/customers/{created!.Id}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        updated.Should().NotBeNull();
        updated!.FullName.Should().Be("Updated Name");
        updated.PhoneNumber.Should().Be("+9999999999");
        updated.Address.Should().Be("Updated Address");
        updated.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task SalesEmployee_Can_Deactivate_And_Activate_Customer()
    {
        // Arrange
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.SalesEmployee);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateCustomerRequest(
            "Status User",
            $"status-{Guid.NewGuid()}@test.com",
            "Password@123",
            "+1234567890",
            "Status Address");

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act 1 - Deactivate
        var deactivateResponse = await _client.PatchAsync($"/api/customers/{created!.Id}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getInactive = await _client.GetFromJsonAsync<CustomerResponse>($"/api/customers/{created.Id}");
        getInactive!.IsActive.Should().BeFalse();

        // Act 2 - Activate
        var activateResponse = await _client.PatchAsync($"/api/customers/{created.Id}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getActive = await _client.GetFromJsonAsync<CustomerResponse>($"/api/customers/{created.Id}");
        getActive!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_Customer_Should_Return_BadRequest_When_Email_Already_Exists()
    {
        // Arrange
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.SalesEmployee);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var email = $"dup-{Guid.NewGuid()}@test.com";
        var request = new CreateCustomerRequest("User 1", email, "Password@123", "+12345", "Address");

        var firstResponse = await _client.PostAsJsonAsync("/api/customers", request);
        firstResponse.EnsureSuccessStatusCode();

        // Act - duplicate
        var secondResponse = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
