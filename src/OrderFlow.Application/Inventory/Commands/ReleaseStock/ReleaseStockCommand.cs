using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.ReleaseStock;

public sealed record ReleaseStockCommand(int ProductId, int Quantity) : IRequest<Result<InventoryResponse>>;
