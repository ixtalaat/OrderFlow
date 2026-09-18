using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Coupons;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Coupons;

public sealed class CouponRepository(ApplicationDbContext db) : ICouponRepository
{
    public Task<Coupon?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Coupons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => db.Coupons.FirstOrDefaultAsync(x => x.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default)
        => db.Coupons.AddAsync(coupon, cancellationToken).AsTask();

    public void Remove(Coupon coupon) => db.Coupons.Remove(coupon);
}
