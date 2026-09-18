using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders;

public sealed class IdempotencyService(IIdempotencyRepository keys, IUnitOfWork unitOfWork) : IIdempotencyService
{
    public static readonly TimeSpan TimeToLive = TimeSpan.FromHours(24);

    public async Task<IdempotencyReplay?> LookupAsync(string userId, string key, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var stored = await keys.GetAsync(userId, key, cancellationToken);
        if (stored is null)
            return null;

        if (stored.IsExpired(DateTime.UtcNow))
        {
            await keys.RemoveAsync(stored, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return null;
        }

        var hash = ComputeHash(request);
        if (!string.Equals(hash, stored.RequestHash, StringComparison.Ordinal))
            return new IdempotencyReplay(false, null);

        var order = JsonSerializer.Deserialize<OrderResponse>(stored.ResponseBody);
        return new IdempotencyReplay(true, order);
    }

    public async Task RecordAsync(string userId, string key, CreateOrderRequest request, OrderResponse response, CancellationToken cancellationToken = default)
    {
        var existing = await keys.GetAsync(userId, key, cancellationToken);
        if (existing is not null)
            return;

        await keys.AddAsync(IdempotencyKey.Create(
            userId,
            key,
            ComputeHash(request),
            StatusCodes.Status201Created,
            JsonSerializer.Serialize(response),
            DateTime.UtcNow.Add(TimeToLive)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeHash(CreateOrderRequest request)
    {
        var canonical = string.Join(";", request.Items
            .OrderBy(x => x.ProductId)
            .Select(x => $"{x.ProductId}:{x.Quantity}"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
