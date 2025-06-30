using FluentValidation;
using SysLog.Client.Models;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Validators;

public class UserRegisterValidator : AbstractValidator<UserRegisterModel>
{
    public UserRegisterValidator()
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
        
        RuleFor(user=> user.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(20)
            .WithMessage("Username must be between 3 and 20 characters");

    }
}