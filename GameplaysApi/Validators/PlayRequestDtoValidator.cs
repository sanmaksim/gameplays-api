using FluentValidation;
using GameplaysApi.DTOs;

namespace GameplaysApi.Validators
{
    public class PlayRequestDtoValidator : AbstractValidator<PlayRequestDto>
    {
        public PlayRequestDtoValidator()
        {
            RuleFor(playDto => playDto)
                .Must(
                    playDto => playDto.UserId != null 
                    || playDto.ApiGameId != null
                    || playDto.PlayId != null
                    || playDto.StatusId != null)
                .WithMessage($"At least one property of {nameof(PlayRequestDto)} must not be null.");
        }
    }
}
