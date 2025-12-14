using FluentValidation;
using MovieAPI.Dto;

namespace MovieAPI.Validation
{
    public class ActorInputDtoValidator : AbstractValidator<ActorInputDto>
    {
        public ActorInputDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ім'я обов'язкове");

            RuleFor(x => x.Gender)
                .InclusiveBetween(0, 1).WithMessage("Стать повинна бути лише 0 (жінка) або 1 (чоловік)");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Країна обов'язкова");
        }
    }
}