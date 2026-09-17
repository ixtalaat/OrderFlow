using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Products;

public class ProductManagementTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SalesEmployee_Can_Create_Update_And_Deactivate_Product()
    {
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var sku = $"SKU-{Guid.NewGuid():N}"[..20].ToUpperInvariant();

        var create = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget", "Description", sku, 10, "Hardware"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();
        product.Should().NotBeNull();
        product!.CategoryName.Should().Be("Hardware");

        var update = await _client.PutAsJsonAsync($"/api/products/{product.Id}", new UpdateProductRequest("Updated Widget", "Updated description", sku, 12.5m, "Hardware"));
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<ProductResponse>())!.Name.Should().Be("Updated Widget");

        var deactivate = await _client.PatchAsync($"/api/products/{product.Id}/deactivate", null);
        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var inactive = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        inactive!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Duplicate_Sku_Should_Return_Conflict()
    {
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var sku = $"DUP-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        var request = new CreateProductRequest("One", "Description", sku, 1, "Category");
        (await _client.PostAsJsonAsync("/api/products", request)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync("/api/products", request)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Customer_Cannot_Modify_Products_And_Only_Sees_Active_Products()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var sku = $"HIDE-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        var create = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Hidden", "Description", sku, 1, "Hidden Category"));
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PatchAsync($"/api/products/{product!.Id}/deactivate", null);

        var (_, customerToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Customer");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var list = await _client.GetFromJsonAsync<PagedList<ProductResponse>>("/api/products");
        list!.Items.Should().NotContain(x => x.Id == product.Id);
        (await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Nope", "Description", "NOPE-001", 1, "Nope"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
