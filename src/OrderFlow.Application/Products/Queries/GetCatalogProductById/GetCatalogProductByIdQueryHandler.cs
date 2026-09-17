using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Pricing;
using OrderFlow.Application.Products;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Queries.GetCatalogProductById;

public sealed class GetCatalogProductByIdQueryHandler(IProductRepository products, ICustomerRepository customers, IPricingService pricing) : IRequestHandler<GetCatalogProductByIdQuery, Result<CatalogProductResponse>>
{
    public async Task<Result<CatalogProductResponse>> Handle(GetCatalogProductByIdQuery query, CancellationToken ct)
    {
        var customer = query.CustomerId > 0 ? await customers.GetByIdAsync(query.CustomerId, ct) : null;
        if (customer is not null && !customer.IsActive) return Result.Failure<CatalogProductResponse>(ProductErrors.NotFound);
        var product = await products.GetCatalogResponseByIdAsync(query.Id, ct);
        var tier = customer?.Tier ?? Domain.Entities.CustomerTier.Regular;
        return product is null ? Result.Failure<CatalogProductResponse>(ProductErrors.NotFound) : Result.Success(product with { CurrentCustomerPrice = await pricing.GetPriceAsync(product.Id, product.CurrentCustomerPrice, tier, ct) });
    }
}
