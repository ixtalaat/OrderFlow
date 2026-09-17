namespace OrderFlow.Application.Common.Exceptions;

public sealed class ConcurrencyConflictException(string message) : ConflictException(message)
{
}
