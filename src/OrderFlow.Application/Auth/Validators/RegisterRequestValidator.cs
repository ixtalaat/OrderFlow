using FluentValidation;
using OrderFlow.Application.Auth.DTOs;

namespace OrderFlow.Application.Auth.Validators;

public sealed class RegisterRequestValidator
    : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);
    }
}