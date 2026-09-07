namespace OrderFlow.Application.Common.Results;

public record Error(string Code, string Description, int? StatusCode)
{
    public static Error None => new(string.Empty, string.Empty, null);
}
