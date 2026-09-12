using FluentValidation.Results;
namespace OficinaMecanica.AuthLambda.Application.Common;

public static class ValidationResultExtensions
{
    public static IReadOnlyCollection<string> ObterMensagensErro(this ValidationResult result)
    {
        return result.Errors
            .Select(error => error.ErrorMessage)
            .Where(mensagem => !string.IsNullOrWhiteSpace(mensagem))
            .Distinct()
            .ToArray();
    }
}
