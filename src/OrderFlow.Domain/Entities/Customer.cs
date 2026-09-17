namespace OrderFlow.Domain.Entities;

public class Customer
{
    private Customer() { } // Required by EF Core

    public Customer(string userId, string phoneNumber, string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        ArgumentException.ThrowIfNullOrWhiteSpace(address, nameof(address));

        UserId = userId;
        PhoneNumber = phoneNumber.Trim();
        Address = address.Trim();
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Customer Create(string userId, string phoneNumber, string address)
    {
        return new Customer(userId, phoneNumber, address);
    }

    public void UpdateDetails(string phoneNumber, string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        ArgumentException.ThrowIfNullOrWhiteSpace(address, nameof(address));

        PhoneNumber = phoneNumber.Trim();
        Address = address.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
