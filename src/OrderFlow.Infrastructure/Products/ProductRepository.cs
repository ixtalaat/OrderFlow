using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Products;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Products;

public sealed class ProductRepository(ApplicationDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsBySkuAsync(string sku, int? excludingId = null, CancellationToken cancellationToken = default)
        => db.Products.AnyAsync(x => x.Sku == sku && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);

    public Task<Category?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default)
        => db.Categories.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

    public Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default) => db.Categories.AddAsync(category, cancellationToken).AsTask();
    public Task AddAsync(Product product, CancellationToken cancellationToken = default) => db.Products.AddAsync(product, cancellationToken).AsTask();

    public async Task<ProductResponse?> GetResponseByIdAsync(int id, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var product = await Query(activeOnly).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return product is null ? null : ToResponse(product);
    }

    public async Task<PagedList<ProductResponse>> GetPagedListAsync(ProductQueryParams parameters, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = Query(activeOnly);
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var pattern = $"%{parameters.SearchTerm.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Name, pattern) || EF.Functions.Like(x.Sku, pattern) || EF.Functions.Like(x.Category.Name, pattern));
        }
        if (parameters.IsActive.HasValue) query = query.Where(x => x.IsActive == parameters.IsActive.Value);
        var total = await query.CountAsync(cancellationToken);
        var page = parameters.PageNumber; var size = parameters.PageSize;
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id).Skip((page - 1) * size).Take(size).Select(x => new ProductResponse(x.Id, x.Name, x.Description, x.Sku, x.Price, x.CategoryId, x.Category.Name, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc)).ToListAsync(cancellationToken);
        return new PagedList<ProductResponse>(items, total, page, size);
    }

    public async Task<CatalogProductResponse?> GetCatalogResponseByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Products.AsNoTracking().Where(x => x.Id == id && x.IsActive)
            .Select(x => new CatalogProductResponse(x.Id, x.Name, x.Description, x.Sku, x.Category.Name, x.Price, x.ProductInventory.AvailableQuantity))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedList<CatalogProductResponse>> GetCatalogPagedListAsync(CatalogQueryParams parameters, CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var pattern = $"%{parameters.SearchTerm.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Name, pattern) || EF.Functions.Like(x.Sku, pattern));
        }
        var total = await query.CountAsync(cancellationToken);
        var page = parameters.PageNumber; var size = parameters.PageSize;
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * size).Take(size)
            .Select(x => new CatalogProductResponse(x.Id, x.Name, x.Description, x.Sku, x.Category.Name, x.Price, x.ProductInventory.AvailableQuantity))
            .ToListAsync(cancellationToken);
        return new PagedList<CatalogProductResponse>(items, total, page, size);
    }

    private IQueryable<Product> Query(bool activeOnly) => db.Products.AsNoTracking().Include(x => x.Category).Where(x => !activeOnly || x.IsActive);

    private static ProductResponse ToResponse(Product x) => new(x.Id, x.Name, x.Description, x.Sku, x.Price, x.CategoryId, x.Category.Name, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc);
}
