namespace OrderFlow.Application.Products;

public interface IProductImageStorage
{
    Task<StoredImage> SaveAsync(Stream content, string contentType, CancellationToken cancellationToken = default);
    void Delete(string relativeUrl);
}
