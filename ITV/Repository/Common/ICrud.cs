using System.Collections.Generic;
using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Models;

namespace ITV.Repository.Common;

public interface ICrud<TKey, TValue>
{
    /// <summary>
    /// Devuelve el listado completo de valores
    /// </summary>
    IEnumerable<TValue> GetAll();

    /// <summary>
    /// Añade la instancia al repositorio
    /// </summary>
    /// <param name="value">La instancia añadir</param>
    /// <returns>La instancia</returns>
    Result<TValue, DomainError> Agregar(TValue value);

    /// <summary>
    /// Borra la instancia del repositorio usando para ello una clave única que la identifica
    /// </summary>
    /// <param name="key">El valor para buscar la instancia deseada</param>
    /// <returns>La instancia o null si no se ha podido borrar</returns>
    Result<TValue, DomainError> Borrar(TKey key, bool isLogical = true);

    /// <summary>
    /// Busca la instancia por su clave única autonumérica
    /// </summary>
    /// <param name="key">El valor que identifica a la instancia</param>
    /// <returns></returns>
    Result<TValue, DomainError> BuscarId(TKey key);

    /// <summary>
    /// Actualiza una instancia mediante el uso de una instancia intermedia con los nuevos datos deseados
    /// y el uso de un valor que identifique la instancia a actualizar para poder obtenerla y añadirle lo nuevos valores
    /// </summary>
    /// <param name="key">El valor para buscar la instancia deseada</param>
    /// <param name="value">Una nueva instancia con unos nuevos datos</param>
    /// <returns>La instancia actualizada</returns>
    Result<TValue, DomainError> Actualizar(TKey key, TValue value);
    
    /// <summary>
    /// Valida que la clave única autonumérica exista
    /// </summary>
    /// <param name="key">El valor a validar</param>
    /// <returns>Si existe true, sino false</returns>
    bool ExistId(TKey key);
    
    /// <summary>
    /// Borra todos los datos del repositorio
    /// </summary>
    void DeleteAll();
}