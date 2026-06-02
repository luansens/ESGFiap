using ESGFiap.ViewModels.Alert;
using FluentValidation;

namespace ESGFiap.Validators.Alert;

public class AlertUpdateValidator : AbstractValidator<AlertUpdateViewModel>
{
    public AlertUpdateValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido.");
    }
}
