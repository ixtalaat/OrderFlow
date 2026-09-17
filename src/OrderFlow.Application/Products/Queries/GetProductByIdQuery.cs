using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products;

public sealed record GetProductByIdQuery(int Id, bool ActiveOnly = true) : IRequest<Result<ProductResponse>>;
