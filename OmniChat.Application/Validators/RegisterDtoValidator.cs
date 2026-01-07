using FluentValidation;

namespace OmniChat.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterOrganizationDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Nome da empresa é obrigatório.")
            .Length(2, 100);

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter pelo menos um número.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^\+[1-9]\d{1,14}$").WithMessage("Telefone deve estar no formato E.164 (ex: +5511999999999).");
    }
}