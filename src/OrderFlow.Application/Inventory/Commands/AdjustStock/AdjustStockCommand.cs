using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.AdjustStock;

public sealed record AdjustStockCommand(int ProductId, int Quantity, string Reason) : IRequest<Result<InventoryResponse>>;
