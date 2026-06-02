using ESGFiap.ViewModels.Auth;
using FluentValidation;

namespace ESGFiap.Validators.Auth;

public class RegisterValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.")
            .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(100).WithMessage("Nome da empresa deve ter no máximo 100 caracteres.");

        RuleFor(x => x.UserType)
            .IsInEnum().WithMessage("Tipo de usuário inválido.");
    }
}
