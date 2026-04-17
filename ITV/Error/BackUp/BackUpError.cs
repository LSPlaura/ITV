using ITV.Error.Common;

namespace ITV.Error.BuckUp;

/// <summary>
/// Record para capturar errores al operar con <see cref="IBackUpService"/>>
/// </summary>
public record BackUpError (string Message) : DomainError(Message)
{
    public sealed record DirectoryNotFound(string Path) : BackUpError($"El directorio .zip de la ruta {Path} no pudo ser encontrado");
}