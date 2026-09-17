using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Orders;

public static class OrderErrors
{
    public static readonly Error CustomerNotFound = new("Order.CustomerNotFound", "Customer not found or inactive.", StatusCodes.Status400BadRequest);
    public static readonly Error ProductUnavailable = new("Order.ProductUnavailable", "Product is not available.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidQuantity = new("Order.InvalidQuantity", "Quantity must be greater than zero.", StatusCodes.Status400BadRequest);
    public static readonly Error InsufficientStock = new("Order.InsufficientStock", "Insufficient stock.", StatusCodes.Status409Conflict);
    public static readonly Error ConcurrencyConflict = new("Order.ConcurrencyConflict", "Inventory was changed by another request. Please retry.", StatusCodes.Status409Conflict);
    public static readonly Error NotFound = new("Order.NotFound", "Order not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidTransition = new("Order.InvalidTransition", "Invalid order status transition.", StatusCodes.Status409Conflict);
}
