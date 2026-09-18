namespace OrderFlow.Application.Common.Identity;

public interface ICurrentUser
{
    string? UserId { get; }
}
