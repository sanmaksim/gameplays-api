using FluentValidation;
using GameplaysApi.DTOs;

namespace GameplaysApi.Validators
{
    public class PlayDtoValidator : AbstractValidator<PlayDto>
    {
        public PlayDtoValidator()
        {
            RuleFor(playDto => playDto)
                .Must(
                    playDto => playDto.UserId != null 
                    || playDto.ApiGameId != null
                    || playDto.StatusId != null)
                .WithMessage($"At least one property of {nameof(PlayDto)} must not be null.");
        }
    }
}
