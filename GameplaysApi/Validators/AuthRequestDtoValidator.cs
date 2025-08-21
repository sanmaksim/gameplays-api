using FluentValidation;
using GameplaysApi.DTOs;

namespace GameplaysApi.Validators
{
    public class AuthRequestDtoValidator : AbstractValidator<AuthRequestDto>
    {
        public AuthRequestDtoValidator()
        {
            RuleFor(authDto => authDto)
                .Must(authDto => string.IsNullOrEmpty(authDto.Username) ^ string.IsNullOrEmpty(authDto.Email))
                .WithMessage("Either Username or Email is required, but not both.");

            RuleFor(x => x.Username)
                .Matches(@"^[a-zA-Z0-9_\-]+$")
                .WithMessage("Username must be alphanumeric.")
                .When(authDto => !string.IsNullOrEmpty(authDto.Username));

            RuleFor(authDto => authDto.Email)
                .EmailAddress()
                .WithMessage("Must be a valid email address.")
                .When(authDto => !string.IsNullOrEmpty(authDto.Email));

            RuleFor(authDto => authDto.Password)
                .NotEmpty()
                .Matches(@"^[a-zA-Z0-9!@#$%^&*_\-+=.,:? ]+$");
        }
    }
}
