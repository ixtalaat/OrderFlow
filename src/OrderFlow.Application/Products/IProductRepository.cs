using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetResponseByIdAsync(int id, bool activeOnly, CancellationToken cancellationToken = default);
    Task<PagedList<ProductResponse>> GetPagedListAsync(ProductQueryParams queryParams, bool activeOnly, CancellationToken cancellationToken = default);
    Task<CatalogProductResponse?> GetCatalogResponseByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedList<CatalogProductResponse>> GetCatalogPagedListAsync(CatalogQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(string sku, int? excludingId = null, CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}
