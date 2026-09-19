using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.Queries.GetDashboardStats;

public sealed class GetDashboardStatsQueryHandler(
    IOrderRepository orders,
    IProductRepository products,
    ICustomerRepository customers) : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsResponse>>
{
    public async Task<Result<DashboardStatsResponse>> Handle(GetDashboardStatsQuery query, CancellationToken ct)
    {
        var summary = await orders.GetSalesSummaryAsync(ct);
        var productPage = await products.GetPagedListAsync(new ProductQueryParams(null, null, 1, 1), false, ct);
        var customerPage = await customers.GetPagedListAsync(new CustomerQueryParams(null, null, 1, 1), ct);
        var lowStock = await products.GetLowStockCountAsync(ct);

        return Result.Success(new DashboardStatsResponse(
            summary.Sum(x => x.Count),
            summary.Where(x => x.Status == OrderStatus.Submitted).Sum(x => x.Count),
            summary.Where(x => x.Status == OrderStatus.Completed).Sum(x => x.Count),
            summary.Sum(x => x.Revenue),
            productPage.TotalCount,
            customerPage.TotalCount,
            lowStock));
    }
}
