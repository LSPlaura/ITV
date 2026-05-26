using ITV.Error.Common;
using ITV.Service;

namespace ITV.Error.BackUp;

/// <summary>
/// Record para capturar errores al operar con <see cref="IBackUpService{T}"/>>
/// </summary>
public record BackUpError (string Message) : DomainError(Message)
{
    public sealed record DirectoryNotFound(string Path) : BackUpError($"El directorio .zip de la ruta {Path} no pudo ser encontrado");
}