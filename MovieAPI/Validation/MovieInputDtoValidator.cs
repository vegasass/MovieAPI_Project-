using FluentValidation;
using MovieAPI.Dto;

namespace MovieAPI.Validation
{
    public class MovieInputDtoValidator : AbstractValidator<MovieInputDto>
    {
        public MovieInputDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Назва фільму обов'язкова");

            RuleFor(x => x.Year)
                .InclusiveBetween(1900, 2050)
                .WithMessage("Рік має бути в адекватному діапазоні");

            RuleFor(x => x.Genre)
                .NotEmpty().WithMessage("Жанр обов'язковий");

            RuleFor(x => x.ActorIds)
                .NotNull().WithMessage("Список акторів обов'язковий")
                .Must(list => list.Count > 0)
                    .When(x => x.ActorIds != null)
                    .WithMessage("Потрібен хоча б один актор");
        }
    }
}
