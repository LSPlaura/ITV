using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Error.Common;
using ITV.Storage.Common;

namespace ITV.Service.Citas;

public interface IService<TKey, TValue>
{
    /// <summary>
    /// Agrega la instancia
    /// </summary>
    /// <param name="item">La instancia a agregar</param>
    /// <returns>La instancia agregada o el error de dominio correspondiente</returns>
    Result<TValue, DomainError> Agregar(TValue item);
    
    /// <summary>
    /// Borra una instancia del repositorio usando para ello una clave única de identificación
    /// </summary>
    /// <param name="key">La clave única para identificar la instancia a borrar</param>
    /// <param name="isLogical">Especifica el tipo de borrado a realizar por defecto lógico, 's' para un borrado físico</param>
    /// <returns>La instancia borrada o el error de dominio correspondiente</returns>
    Result<TValue, DomainError> Borrar(TKey key, bool isLogical = true);
    
    /// <summary>
    /// Busca una instancia mediante una clave única de identificación
    /// </summary>
    /// <param name="key">La clave única para identificar la instancia que se quiere encontrar</param>
    /// <returns>La instancia obtenida o el error de dominio correspondiente</returns>
    Result<TValue, DomainError> GetById(TKey key); 
    
    /// <summary>
    /// Actualiza los datos de una instancia usando para ello una clave única de identificación
    /// </summary>
    /// <param name="key">La clave única para identificar la instancia a actualizar</param>
    /// <param name="item">La instancia con los datos actualizados</param>
    /// <returns>La instancia actualizada o el error de dominio correspondiente</returns>
    Result<TValue, DomainError> Actualizar(TKey key, TValue item);
    
    /// <summary>
    /// Obtiene una lista con todas las instancias almacenadas en el repositorio con paginación
    /// <param name="pagina">índice de la pagina</param>
    /// <param name="cantidad">Numero de elementos a tomar</param>
    /// </summary>
    /// <returns>Lista de instancias</returns>
    IEnumerable<TValue> GetAll(int pagina = 0, int cantidad = 10);
}