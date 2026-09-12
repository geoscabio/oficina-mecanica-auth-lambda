using FluentValidation.Results;
namespace OficinaMecanica.AuthLambda.Application.Common;
public static class ValidationResultExtensions { public static IReadOnlyCollection<string> ObterMensagensErro(this ValidationResult result) => result.Errors.Select(x => x.ErrorMessage).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray(); }
