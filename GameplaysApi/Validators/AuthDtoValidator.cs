using FluentValidation;
using GameplaysApi.DTOs;

namespace GameplaysApi.Validators
{
    public class AuthDtoValidator : AbstractValidator<AuthDto>
    {
        public AuthDtoValidator()
        {
            RuleFor(authDto => authDto)
                .Must(authDto => string.IsNullOrEmpty(authDto.Username) ^ string.IsNullOrEmpty(authDto.Email))
                .WithMessage("Either Username or Email is required, but not both.");

            RuleFor(authDto => authDto.Email)
                .EmailAddress()
                .When(authDto => !string.IsNullOrEmpty(authDto.Email));

            RuleFor(authDto => authDto.Password).NotEmpty();
        }
    }
}
