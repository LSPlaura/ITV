using ITV.Error.Common;

namespace ITV.Error.DataBase;
/// <summary>
/// Record para capturar errores al operar con bases de datos>
/// </summary>
public record DataBaseError(string Message) : DomainError (Message);