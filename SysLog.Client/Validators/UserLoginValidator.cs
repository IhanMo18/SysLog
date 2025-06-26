using FluentValidation;
using SysLog.Client.Models;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Validators;

public class UserLoginValidator : AbstractValidator<UserLoginModel>
{
    public UserLoginValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email address");

        RuleFor(user => user.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(10)
            .WithMessage("Password must be at least 10 characters");

    }
}