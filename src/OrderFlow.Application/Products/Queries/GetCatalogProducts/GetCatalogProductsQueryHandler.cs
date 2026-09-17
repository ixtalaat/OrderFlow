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
        var prices = await pricing.GetPricesAsync(page.Items.ToDictionary(x => x.Id, x => x.CurrentCustomerPrice), tier, ct);
        var items = page.Items.Select(item => item with { CurrentCustomerPrice = prices[item.Id] }).ToList();
        return Result.Success(new PagedList<CatalogProductResponse>(items, page.TotalCount, page.PageNumber, page.PageSize));
    }
}
