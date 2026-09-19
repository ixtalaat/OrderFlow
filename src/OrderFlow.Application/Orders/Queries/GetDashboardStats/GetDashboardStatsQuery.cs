using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries.GetDashboardStats;

public sealed record GetDashboardStatsQuery : IRequest<Result<DashboardStatsResponse>>;
