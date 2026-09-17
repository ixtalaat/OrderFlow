using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Products;

public class ProductCatalogTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Customer_Can_Search_And_Page_Active_Catalog_Products()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var sku = $"CAT-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var create = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Catalog Target", "Catalog description", sku, 25.50m, "Catalog"));
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();

        var (_, customerToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Customer");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var response = await _client.GetFromJsonAsync<PagedList<CatalogProductResponse>>($"/api/catalog/products?searchTerm={sku}&pageNumber=1&pageSize=1");

        response!.Items.Should().ContainSingle(x => x.Id == product!.Id);
        response.Items[0].CurrentCustomerPrice.Should().Be(25.50m);
        response.Items[0].AvailableQuantity.Should().Be(0);
        response.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task Customer_Cannot_Browse_Inactive_Product()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var sku = $"OFF-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var create = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Inactive Catalog Product", "Description", sku, 5, "Catalog"));
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PatchAsync($"/api/products/{product!.Id}/deactivate", null);

        var (_, customerToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Customer");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var list = await _client.GetFromJsonAsync<PagedList<CatalogProductResponse>>("/api/catalog/products");
        list!.Items.Should().NotContain(x => x.Id == product.Id);
        (await _client.GetAsync($"/api/catalog/products/{product.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Catalog_Should_Require_Customer_Authorization()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        (await _client.GetAsync("/api/catalog/products")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
