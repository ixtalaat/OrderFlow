using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Queries.GetCatalogProducts;

public sealed record GetCatalogProductsQuery(int CustomerId, CatalogQueryParams QueryParams) : IRequest<Result<PagedList<CatalogProductResponse>>>;
