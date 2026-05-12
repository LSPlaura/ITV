using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Models;

namespace ITV.Service.Export;

public interface IExport<T>
{
    /// <summary>
    /// Exporta los datos de un objeto a un archivo html
    /// </summary>
    /// <param name="item">El objeto a exportar</param>
    /// <returns>La ruta al archivo si todo va bien, un error de dominio si algo falla</returns>
    Result<string, DomainError> ExportHtml(T item);
    /// <summary>
    /// Exporta los datos de un objeto a un archivo pdf
    /// </summary>
    /// <param name="item">El objeto a exportar</param>
    /// <returns>La ruta al archivo si todo va bien, un error de dominio si algo falla</returns>
    Result<string, DomainError> ExportPdf(T item);
}