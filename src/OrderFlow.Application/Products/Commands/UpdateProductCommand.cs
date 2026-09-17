using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products;

public sealed record UpdateProductCommand(int Id, string Name, string Description, string Sku, decimal Price, string CategoryName) : IRequest<Result<ProductResponse>>;
