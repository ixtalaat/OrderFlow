using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Coupons;

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default);
    void Remove(Coupon coupon);
}
