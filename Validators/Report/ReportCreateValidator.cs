using ESGFiap.ViewModels.Report;
using FluentValidation;

namespace ESGFiap.Validators.Report;

public class ReportCreateValidator : AbstractValidator<ReportCreateViewModel>
{
    public ReportCreateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Region)
            .NotEmpty().WithMessage("Região é obrigatória.")
            .MaximumLength(100).WithMessage("Região deve ter no máximo 100 caracteres.");

        RuleFor(x => x.PeriodStart)
            .NotEmpty().WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty().WithMessage("Data de fim é obrigatória.")
            .GreaterThan(x => x.PeriodStart).WithMessage("Data de fim deve ser posterior à data de início.");

        RuleFor(x => x.RegionalAverageLiters)
            .GreaterThanOrEqualTo(0).WithMessage("Média regional não pode ser negativa.");

        RuleFor(x => x.ImpactAnalysis)
            .NotEmpty().WithMessage("Análise de impacto é obrigatória.")
            .MaximumLength(2000).WithMessage("Análise deve ter no máximo 2000 caracteres.");

        RuleFor(x => x.MitigationPlan)
            .NotEmpty().WithMessage("Plano de mitigação é obrigatório.")
            .MaximumLength(2000).WithMessage("Plano deve ter no máximo 2000 caracteres.");
    }
}
