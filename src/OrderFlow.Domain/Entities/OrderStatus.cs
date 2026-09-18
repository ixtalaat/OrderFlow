namespace OrderFlow.Domain.Entities;

public enum OrderStatus
{
    Draft,
    Submitted,
    Confirmed,
    Processing,
    Completed,
    Rejected,
    Cancelled
}
