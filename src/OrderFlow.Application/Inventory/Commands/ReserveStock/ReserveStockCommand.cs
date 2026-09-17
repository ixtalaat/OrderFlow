using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.ReserveStock;

public sealed record ReserveStockCommand(int ProductId, int Quantity) : IRequest<Result<InventoryResponse>>;
