

using CSharpFunctionalExtensions;
using ITV.Error.Common;
namespace ITV.Storage.Common;

/// <summary>
/// Permite exportar e importar los datos usando un almacenamiento persistente
/// </summary>
public interface IStorage<T>
{
    /// <summary>
    /// Importa los datos a un fichero
    /// </summary>
    /// <param name="items">Coleccion de datos a guardar</param>
    /// <param name="path">True si se ha podido salvar los datos o el error correspondiente</param>
    Result<bool, DomainError> Salvar(IEnumerable<T> items, string path);
    /// <summary>
    /// Exporta los datos de un fichero
    /// </summary>
    /// <param name="path">La ruta al fichero</param>
    /// <returns>Coleccion de los datos mapeados o el error correspondiente</returns>
   Result<IEnumerable<T>, DomainError> Cargar(string path);
}