using FluentValidation;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Messages;

namespace OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;

public sealed class AutenticarClientePorDocumentoValidator
    : AbstractValidator<AutenticarClientePorDocumentoRequest>
{
    public AutenticarClientePorDocumentoValidator()
    {
        RuleFor(request => request)
            .NotNull()
            .WithMessage("Requisição inválida.");

        When(request => request is not null, () =>
        {
            RuleFor(request => request.Documento)
                .NotEmpty()
                .WithMessage(ClienteErrorMessages.DocumentoObrigatorio);
        });
    }
}
