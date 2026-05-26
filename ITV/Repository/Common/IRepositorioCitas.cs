using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Models;

namespace ITV.Repository.Common;

public interface IRepositorioCitas : ICrud<int, Cita>
{
    /// <summary>
    /// Valida si la matricula de un vehiculo existe
    /// </summary>
    /// <param name="key">La matricula</param>
    /// <returns>True si la matricula existe, sino false</returns>
    bool ExistMatricula(string key);
    /// <summary>
    /// Obtiene un vehiculo mediante su matricula
    /// </summary>
    /// <param name="key">La matricula</param>
    /// <returns>El vehiculo, null si no ha sido encontrado</returns>
    Result<Cita, DomainError> BuscarMatricula(string key);
}