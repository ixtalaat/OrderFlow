using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Products;

public sealed record ActivateProductCommand(int Id) : IRequest<Result>;
