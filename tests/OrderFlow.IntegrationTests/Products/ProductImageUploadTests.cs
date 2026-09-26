using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Products;

public class ProductImageUploadTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly byte[] PngBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SalesEmployee_Can_Upload_Png_And_ImageUrl_Is_Returned()
    {
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var sku = $"IMG-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        var create = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Camera", "Description", sku, 10, "Hardware"));
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();
        product!.ImageUrl.Should().BeNull();

        var upload = await PostImageAsync(product.Id, PngBytes, "image/png", "photo.png");
        upload.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await upload.Content.ReadFromJsonAsync<ProductResponse>();
        updated!.ImageUrl.Should().StartWith("/product-images/").And.EndWith(".png");

        var fetched = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        fetched!.ImageUrl.Should().Be(updated.ImageUrl);
    }

    [Fact]
    public async Task Upload_Should_Reject_Non_Image_Content()
    {
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var sku = $"TXT-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        var create = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget", "Description", sku, 10, "Hardware"));
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();

        var upload = await PostImageAsync(product!.Id, "not an image"u8.ToArray(), "image/png", "evil.png");
        upload.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_Should_Reject_Missing_Product_And_Forbidden_Roles()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        (await PostImageAsync(int.MaxValue, PngBytes, "image/png", "photo.png")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var (_, customerToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Customer");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        (await PostImageAsync(1, PngBytes, "image/png", "photo.png")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<HttpResponseMessage> PostImageAsync(int productId, byte[] bytes, string contentType, string fileName)
    {
        using var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(content, "file", fileName);
        return await _client.PostAsync($"/api/products/{productId}/image", form);
    }
}
