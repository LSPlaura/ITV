using ITV.Error.Common;
using ITV.Storage.Common;

namespace ITV.Error.Storage;

/// <summary>
/// Record para capturar errores al operar con ficheros en el <see cref="IStorage{T}"/>>
/// </summary>
public record StorageError(string Message) : DomainError ($"Error al intentar realizar una operación del storage: {Message}");