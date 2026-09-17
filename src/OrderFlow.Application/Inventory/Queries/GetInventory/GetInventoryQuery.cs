using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Queries.GetInventory;

public sealed record GetInventoryQuery(int ProductId) : IRequest<Result<InventoryResponse>>;
