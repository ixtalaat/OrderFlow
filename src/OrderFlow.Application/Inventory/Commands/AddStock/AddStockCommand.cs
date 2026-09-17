using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.AddStock;

public sealed record AddStockCommand(int ProductId, int Quantity) : IRequest<Result<InventoryResponse>>;
