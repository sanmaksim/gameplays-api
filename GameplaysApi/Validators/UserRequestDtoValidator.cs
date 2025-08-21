using FluentValidation;
using GameplaysApi.DTOs;
using MySql.Data.MySqlClient;

namespace GameplaysApi.Validators
{
    public class UserRequestDtoValidator : AbstractValidator<UserRequestDto>
    {
        public UserRequestDtoValidator()
        {
            RuleFor(userRequestDto => userRequestDto.Username)
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
                    .WithMessage("Username cannot end with a dash or underscore.")
                .When(userRequestDto => !string.IsNullOrEmpty(userRequestDto.Username));

            RuleFor(userRequestDto => userRequestDto.Email)
                .EmailAddress()
                .When(userRequestDto => !string.IsNullOrEmpty(userRequestDto.Email));

            RuleFor(userRequestDto => userRequestDto.Password)
                .MinimumLength(8)
                    .WithMessage("Your password length must be at least 8 characters.")
                .MaximumLength(32)
                    .WithMessage("Your password length must not exceed 32 characters.")
                .Matches("[A-Z]+")
                    .WithMessage("Your password must contain at least one uppercase letter.")
                .Matches("[a-z]+")
                    .WithMessage("Your password must contain at least one lowercase letter.")
                .Matches("[0-9]+")
                    .WithMessage("Your password must contain at least one number.")
                .Matches(@"[!@#$%^&*_\-+=.,:? ]+")
                    .WithMessage("Your password must contain at least one special character: !@#$%^&*_-+=.,:?")
                .Matches(@"^[a-zA-Z0-9!@#$%^&*_\-+=.,:? ]+$")
                    .WithMessage("Your password contains invalid characters; only letters, digits, and !@#$%^&*_-+=.,:? are allowed.")
                .When(userRequestDto => !string.IsNullOrEmpty(userRequestDto.Password));
        }
    }
}
