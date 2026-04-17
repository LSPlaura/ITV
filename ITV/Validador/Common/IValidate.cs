using CSharpFunctionalExtensions;
using ITV.Error.Common;

namespace ITV.Validador;

/// <summary>
/// Interfaz genérica para inversión de dependencias
/// </summary>
public interface IValidate<T>
{
    /// <summary>
    /// Implementa las validaciones necesarias y en caso de no cumplir con ellas
    /// lanza las excepciones de dominio correspondientes 
    /// </summary>
    /// <param name="item">Instancia a validar</param>
    Result<bool, DomainError> Validar(T item);
}