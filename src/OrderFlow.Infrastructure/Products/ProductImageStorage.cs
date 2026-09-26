using Microsoft.Extensions.Hosting;
using OrderFlow.Application.Products;

namespace OrderFlow.Infrastructure.Products;

public sealed class ProductImageStorage(IHostEnvironment environment) : IProductImageStorage
{
    public const long MaxBytes = 5 * 1024 * 1024;

    public async Task<StoredImage> SaveAsync(Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = DetectExtension(content, contentType);
        if (extension is null) throw new InvalidOperationException("Unsupported image content.");

        var folder = Path.Combine(environment.ContentRootPath, "wwwroot", "product-images");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var file = File.Create(Path.Combine(folder, fileName));
        content.Position = 0;
        await content.CopyToAsync(file, cancellationToken);

        return new StoredImage($"/product-images/{fileName}");
    }

    public void Delete(string relativeUrl)
    {
        var fileName = Path.GetFileName(relativeUrl.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(fileName)) return;
        var path = Path.Combine(environment.ContentRootPath, "wwwroot", "product-images", fileName);
        if (File.Exists(path)) File.Delete(path);
    }

    private static string? DetectExtension(Stream content, string contentType)
    {
        if (content.Length > MaxBytes) return null;

        Span<byte> header = stackalloc byte[8];
        content.Position = 0;
        var read = content.Read(header);
        if (read < 4) return null;

        var isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
        if (!isJpeg && !isPng) return null;

        var declared = contentType.Trim().ToLowerInvariant();
        if (isJpeg && declared != "image/jpeg" && declared != "image/jpg") return null;
        if (isPng && declared != "image/png") return null;

        return isJpeg ? ".jpg" : ".png";
    }
}
