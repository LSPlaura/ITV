using System.Collections.Generic;
using CSharpFunctionalExtensions;
using ITV.Error.Common;

namespace ITV.Service;

public interface IBuckUpService <T>
{
    /// <summary>
    /// Crea un directorio que almacena directorios comprimidos en .zip con archivos buckup de los datos que en el momento tenga el repositorio
    /// </summary>
    /// <param name="lista">Lista de vehiculos a salvar</param>
    /// <returns>la ruta al directorio -zip creado</returns>
    Result<string, DomainError> Guardar(IEnumerable<T> lista);
    /// <summary>
    /// Descomprime el .zip y carga el archivo bukup
    /// </summary>
    /// <param name="path">Ruta de la que extraer los datos</param>
    /// <returns>Lista de vehiculos restaurados</returns>
    Result<IEnumerable<T>, DomainError> Resturar(string path);
    /// <summary>
    /// Obtiene todos los directorios comprimidos con sus respectivos archivos buckup
    /// </summary>
    /// <returns>Lista de los nombres de los directorios comprimidos</returns>
    IEnumerable<string> Listar();
}