using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Notifications;
using OrderFlow.Application.Products;
using OrderFlow.Infrastructure.Identity;

namespace OrderFlow.Infrastructure.Notifications;

public sealed class StaffNotificationService(
    UserManager<ApplicationUser> users,
    IProductRepository products,
    IEmailSender emailSender,
    ILogger<StaffNotificationService> logger) : IStaffNotificationService
{
    public async Task NotifyLowStockAsync(int productId, int availableQuantity, CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(productId, cancellationToken);
        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var role in new[] { Roles.Admin, Roles.SalesEmployee })
            foreach (var user in await users.GetUsersInRoleAsync(role))
                if (!string.IsNullOrWhiteSpace(user.Email))
                    recipients.Add(user.Email);

        if (recipients.Count == 0)
        {
            logger.LogWarning("Low stock for product {ProductId} ({Available} available) but no staff recipients exist.", productId, availableQuantity);
            return;
        }

        foreach (var recipient in recipients)
        {
            try
            {
                await emailSender.SendAsync(
                    recipient,
                    $"Low stock: {product?.Name ?? $"product {productId}"}",
                    $"Product {product?.Name ?? productId.ToString()} has {availableQuantity} units available.",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Low stock notification to {Recipient} failed.", recipient);
            }
        }
    }
}
