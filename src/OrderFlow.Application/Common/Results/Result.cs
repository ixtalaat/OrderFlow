namespace OrderFlow.Application.Common.Results;

public sealed class Result<TValue>(TValue? value, bool isSuccess, Error error) : Result(isSuccess, error)
{
    private readonly TValue? _value = value;
    public TValue? Value => IsSuccess ? _value : throw new InvalidOperationException("Value is not available for failed results.");
}
