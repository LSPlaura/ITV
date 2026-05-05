using System.Collections.Generic;
using CSharpFunctionalExtensions;
using ITV.Error.Common;

namespace ITV.Service;

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
    /// Obtiene una lista con todas las instancias almacenadas en el repositorio
    /// </summary>
    /// <returns>Lista de instancias</returns>
    IEnumerable<TValue> GetAll();
    
    /// <summary>
    /// Importa los datos almacenados en el tipo de archivo correspondiente mediante la ruta configurada y los almacena en el repositorio
    /// borrando todos los datos anteriores para no causar conflictos de integridad
    /// </summary>
    /// <returns>El número de instancias guardadas en el repositorio</returns>
    Result<int, DomainError> Importar();
    
    /// <summary>
    /// Exporta los datos actuales del repositorio en un archivo del tipo correspondiente al storage configurado
    /// </summary>
    /// <returns>El número de instancias exportadas al archivo</returns>
    Result<int, DomainError> Exportar();
    
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
}