using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Commands.SetProductImage;

public sealed record SetProductImageCommand(int ProductId, Stream Content, string ContentType, long Length) : IRequest<Result<ProductResponse>>;
