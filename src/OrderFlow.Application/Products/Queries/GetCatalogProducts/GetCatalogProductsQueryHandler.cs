using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Pricing;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Queries.GetCatalogProducts;

public sealed class GetCatalogProductsQueryHandler(IProductRepository products, ICustomerRepository customers, IPricingService pricing) : IRequestHandler<GetCatalogProductsQuery, Result<PagedList<CatalogProductResponse>>>
{
    public async Task<Result<PagedList<CatalogProductResponse>>> Handle(GetCatalogProductsQuery query, CancellationToken ct)
    {
        var customer = query.CustomerId > 0 ? await customers.GetByIdAsync(query.CustomerId, ct) : null;
        if (customer is not null && !customer.IsActive) return Result.Failure<PagedList<CatalogProductResponse>>(ProductErrors.NotFound);
        var tier = customer?.Tier ?? Domain.Entities.CustomerTier.Regular;
        var page = await products.GetCatalogPagedListAsync(query.QueryParams, ct);
        var items = new List<CatalogProductResponse>(page.Items.Count);
        foreach (var item in page.Items) items.Add(item with { CurrentCustomerPrice = await pricing.GetPriceAsync(item.Id, item.CurrentCustomerPrice, tier, ct) });
        return Result.Success(new PagedList<CatalogProductResponse>(items, page.TotalCount, page.PageNumber, page.PageSize));
    }
}
