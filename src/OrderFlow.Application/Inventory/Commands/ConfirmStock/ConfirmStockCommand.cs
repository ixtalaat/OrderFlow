using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.ConfirmStock;

public sealed record ConfirmStockCommand(int ProductId, int Quantity) : IRequest<Result<InventoryResponse>>;
