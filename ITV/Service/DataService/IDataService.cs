using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Storage.Common;

namespace ITV.Service.DataService;

public interface IDataService<TValue>
{
    /// <summary>
    /// Importa los datos   almacenados en el tipo de archivo correspondiente mediante la ruta configurada y los almacena en el repositorio
    /// borrando todos los datos anteriores para no causar conflictos de integridad
    /// <param name="storage">El Storage a utilizar implementado mediante su interfaz para que sea intercambiable</param>
    /// </summary>
    /// <returns>El número de instancias guardadas en el repositorio</returns>
    Result<int, DomainError> Importar(IStorage<TValue> storage);
    
    /// <summary>
    /// Exporta los datos actuales del repositorio en un archivo del tipo correspondiente al storage configurado
    /// <param name="storage">El Storage a utilizar implementado mediante su interfaz para que sea intercambiable</param>
    /// </summary>
    /// <returns>El número de instancias exportadas al archivo</returns>
    Result<int, DomainError> Exportar(IStorage<TValue> storage);
    
    /// <summary>
    /// Realiza un buckup del estado actual del repositorio en la carpeta de buckups configurada y guardadondo el archivo dentro de una carpeta .zip
    /// </summary>
    /// <returns>La ruta al buckup</returns>
    Result<string, DomainError> GuardarBuckUp();
    
    /// <summary>
    /// Restaura el repositorio mediante un buckup ya realizado medinate la ruta a este, descomprimieéndolo, obteniendo los datos del archivo, borrando el reopsitorio para que
    /// no haya conflicto de integradad entre los datos y guardando los datos anteriormente extraidos.
    /// </summary>
    /// <param name="path">La ruta a la carpeta .zip que guarda el buckup</param>
    /// <returns>El número de instancias almacendas</returns>
    Result<int, DomainError> RestaurarBuckUp(string path);

    /// <summary>
    /// Listado de los BackUps realizados
    /// </summary>
    /// <returns></returns>
    List<string> ListadoBackUps();
}