using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers;

public static class CustomerErrors
{
    public static readonly Error NotFound =
        new("Customer.NotFound", "Customer not found.", StatusCodes.Status404NotFound);

    public static readonly Error UserNotFound =
        new("Customer.UserNotFound", "User associated with customer not found.", StatusCodes.Status404NotFound);

    public static readonly Error UserAlreadyHasProfile =
        new("Customer.UserAlreadyHasProfile", "The specified user already has a customer profile.", StatusCodes.Status400BadRequest);

    public static readonly Error EmailAlreadyExists =
        new("Customer.EmailAlreadyExists", "Email is already registered.", StatusCodes.Status400BadRequest);

    public static readonly Error UserCreationFailed =
        new("Customer.UserCreationFailed", "Failed to create user account for customer.", StatusCodes.Status400BadRequest);

    public static readonly Error Forbidden =
        new("Customer.Forbidden", "You do not have permission to access or modify this customer profile.", StatusCodes.Status403Forbidden);
}
