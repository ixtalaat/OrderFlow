using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Products.Commands.ActivateProduct;

public sealed record ActivateProductCommand(int Id) : IRequest<Result>;
