namespace OficinaMecanica.AuthLambda.Application.Common;

public sealed class DependenciaIndisponivelException : Exception
{
    public DependenciaIndisponivelException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
