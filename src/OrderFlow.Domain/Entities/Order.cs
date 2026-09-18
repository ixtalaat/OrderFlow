namespace OrderFlow.Domain.Entities;

public sealed class Order
{
    private Order() { }
    private Order(int customerId) { if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId)); CustomerId = customerId; Status = OrderStatus.Draft; CreatedAtUtc = DateTime.UtcNow; }

    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public string? CouponCode { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount => Items.Sum(x => x.LineTotal) - DiscountAmount;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public AccountingSyncStatus AccountingSyncStatus { get; private set; } = AccountingSyncStatus.Pending;
    public string? ExternalInvoiceId { get; private set; }
    public int AccountingSyncAttempts { get; private set; }
    public DateTime? AccountingLastAttemptAtUtc { get; private set; }
    public string? AccountingLastError { get; private set; }
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    public static Order Create(int customerId) => new(customerId);
    public void AddItem(OrderItem item) { ArgumentNullException.ThrowIfNull(item); Items.Add(item); }
    public void ApplyCoupon(string code, decimal discountAmount)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Coupon code is required.", nameof(code));
        if (discountAmount < 0) throw new ArgumentOutOfRangeException(nameof(discountAmount));
        CouponCode = code.Trim().ToUpperInvariant();
        DiscountAmount = discountAmount;
        Touch();
    }
    public void Submit() { EnsureStatus(OrderStatus.Draft); Status = OrderStatus.Submitted; Touch(); }
    public void Confirm() { EnsureStatus(OrderStatus.Submitted); Status = OrderStatus.Confirmed; AccountingSyncStatus = AccountingSyncStatus.Pending; Touch(); }
    public void BeginAccountingAttempt() { AccountingSyncStatus = AccountingSyncStatus.Pending; AccountingSyncAttempts++; AccountingLastAttemptAtUtc = DateTime.UtcNow; }
    public void MarkAccountingSucceeded(string invoiceId) { ExternalInvoiceId = invoiceId; AccountingSyncStatus = AccountingSyncStatus.Succeeded; AccountingLastError = null; AccountingLastAttemptAtUtc = DateTime.UtcNow; Touch(); }
    public void MarkAccountingFailed(string error) { AccountingSyncStatus = AccountingSyncStatus.Failed; AccountingLastError = error; AccountingLastAttemptAtUtc = DateTime.UtcNow; Touch(); }
    public void Reject() { EnsureStatus(OrderStatus.Submitted); Status = OrderStatus.Rejected; Touch(); }
    public void Cancel() { EnsureStatus(OrderStatus.Submitted); Status = OrderStatus.Cancelled; Touch(); }
    public void Process() { EnsureStatus(OrderStatus.Confirmed); Status = OrderStatus.Processing; Touch(); }
    public void Complete() { EnsureStatus(OrderStatus.Processing); Status = OrderStatus.Completed; Touch(); }
    private void EnsureStatus(OrderStatus expected) { if (Status != expected) throw new InvalidOperationException($"Order must be {expected}."); }
    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
