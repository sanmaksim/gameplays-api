using FluentValidation;
using GameplaysApi.DTOs;

namespace GameplaysApi.Validators
{
    public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestDtoValidator()
        {
            RuleFor(registerRequestDto => registerRequestDto.Username)
                .NotEmpty()
                    .WithMessage("Your username cannot be empty")
                .MinimumLength(6)
                    .WithMessage("Your username length must be at least 6 characters.")
                .MaximumLength(20)
                    .WithMessage("Your username length must not exceed 20 characters.")
                .Matches("^[a-zA-Z0-9-_]+$")
                    .WithMessage("Username can contain only letters, numbers, dashes, and underscores.")
                .Matches("^(?!.*(--|__)).*$")
                    .WithMessage("Username cannot contain consecutive dashes or underscores.")
                .Must(username => !string.IsNullOrEmpty(username) && username[0] != '-' && username[0] != '_')
                    .WithMessage("Username cannot start with a dash or underscore.")
                .Must(username => !string.IsNullOrEmpty(username) && username[^1] != '-' && username[^1] != '_')
                    .WithMessage("Username cannot end with a dash or underscore.");

            RuleFor(registerRequestDto => registerRequestDto.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Must be a valid email address.");

            RuleFor(registerRequestDto => registerRequestDto.Password)
                .NotEmpty().WithMessage("Your password cannot be empty")
                .MinimumLength(8).WithMessage("Your password length must be at least 8 characters.")
                .MaximumLength(32).WithMessage("Your password length must not exceed 32 characters.")
                .Matches("[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                .Matches("[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                .Matches("[0-9]+").WithMessage("Your password must contain at least one number.")
                .Matches(@"[!@#$%^&*_\-+=.,:? ]+").WithMessage("Your password must contain at least one special character: !@#$%^&*_-+=.,:?")
                .Matches(@"^[a-zA-Z0-9!@#$%^&*_\-+=.,:? ]+$")
                .WithMessage("Your password contains invalid characters; only letters, digits, and !@#$%^&*_-+=.,:? are allowed.");
        }
    }
}
