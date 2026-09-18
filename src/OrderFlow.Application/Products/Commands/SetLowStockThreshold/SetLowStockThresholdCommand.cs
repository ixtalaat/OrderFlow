using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Products.Commands.SetLowStockThreshold;

public sealed record SetLowStockThresholdCommand(int ProductId, int? Threshold) : IRequest<Result>;
