using ESGFiap.ViewModels.Reading;
using FluentValidation;

namespace ESGFiap.Validators.Reading;

public class ReadingCreateValidator : AbstractValidator<ReadingCreateViewModel>
{
    public ReadingCreateValidator()
    {
        RuleFor(x => x.LitersPerHour)
            .GreaterThanOrEqualTo(0).WithMessage("Consumo em litros não pode ser negativo.");

        RuleFor(x => x.PressureBar)
            .InclusiveBetween(0, 20).WithMessage("Pressão deve estar entre 0 e 20 bar.");

        RuleFor(x => x.ReadingAt)
            .Must(d => !d.HasValue || d.Value <= DateTime.UtcNow)
            .WithMessage("Data da leitura não pode ser futura.");
    }
}
