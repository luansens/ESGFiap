using ESGFiap.ViewModels.Sensor;
using FluentValidation;

namespace ESGFiap.Validators.Sensor;

public class SensorCreateValidator : AbstractValidator<SensorCreateViewModel>
{
    public SensorCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do sensor é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Localização é obrigatória.")
            .MaximumLength(200).WithMessage("Localização deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de sensor inválido.");

        RuleFor(x => x.AlertThresholdLitersPerHour)
            .GreaterThan(0).WithMessage("O limite de alerta deve ser maior que zero.");
    }
}
