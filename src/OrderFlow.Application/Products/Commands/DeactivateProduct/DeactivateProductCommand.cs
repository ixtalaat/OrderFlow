using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Products.Commands.DeactivateProduct;

public sealed record DeactivateProductCommand(int Id) : IRequest<Result>;
