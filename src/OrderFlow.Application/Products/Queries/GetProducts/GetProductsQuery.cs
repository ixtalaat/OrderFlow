using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(ProductQueryParams QueryParams, bool ActiveOnly = true) : IRequest<Result<PagedList<ProductResponse>>>;
