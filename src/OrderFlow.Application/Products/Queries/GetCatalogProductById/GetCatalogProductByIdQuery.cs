using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Queries.GetCatalogProductById;

public sealed record GetCatalogProductByIdQuery(int Id) : IRequest<Result<CatalogProductResponse>>;
