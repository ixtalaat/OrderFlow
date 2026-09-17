using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Customers;

public class CustomerAuthorizationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Customer_Endpoints_Should_Return_Unauthorized_When_No_Token()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var getResponse = await _client.GetAsync("/api/customers");
        getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var meResponse = await _client.GetAsync("/api/customers/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var postResponse = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("A", "a@a.com", "Password@123", "+123", "Addr"));
        postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Customer_Role_Cannot_Create_Or_List_Customers()
    {
        // Arrange
        var (_, customerToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.Customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

        // Act & Assert 1 - List
        var listResponse = await _client.GetAsync("/api/customers");
        listResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Act & Assert 2 - Create
        var createResponse = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("A", "a@a.com", "Password@123", "+123", "Addr"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Customer_Role_Cannot_Access_Other_Customer_Private_Data()
    {
        // Arrange
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.SalesEmployee);
        var (_, customer1Token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.Customer);
        var (_, customer2Token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.Customer);

        // Sales employee creates customer 1 profile
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var createRequest = new CreateCustomerRequest("Customer One", $"c1-{Guid.NewGuid()}@test.com", "Password@123", "+111", "Addr 1");
        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var customer1Profile = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Customer 2 attempts to view Customer 1's profile by ID
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer2Token);
        var forbiddenResponse = await _client.GetAsync($"/api/customers/{customer1Profile!.Id}");

        // Assert
        forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Customer_Can_Access_Own_Profile_Via_Me_Endpoint()
    {
        // Arrange
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, Roles.SalesEmployee);

        var customerEmail = $"myprofile-{Guid.NewGuid()}@test.com";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var createRequest = new CreateCustomerRequest("My Profile", customerEmail, "Password@123", "+999", "My Home");
        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Log in / generate token as this newly created customer
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new OrderFlow.Application.Auth.DTOs.LoginRequest(customerEmail, "Password@123"));
        loginResponse.EnsureSuccessStatusCode();
        var authResult = await loginResponse.Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();

        // Act - Call /api/customers/me
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult!.AccessToken);
        var meResponse = await _client.GetAsync("/api/customers/me");

        // Assert
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await meResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        profile.Should().NotBeNull();
        profile!.Id.Should().Be(created!.Id);
        profile.Email.Should().Be(customerEmail);
        profile.FullName.Should().Be("My Profile");

        // Act 2 - Call /api/customers/{ownId}
        var ownIdResponse = await _client.GetAsync($"/api/customers/{created.Id}");
        ownIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
