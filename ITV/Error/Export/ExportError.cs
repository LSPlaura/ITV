namespace ITV.Error.Export;

using ITV.Error.Common;

/// <summary>
/// Record para capturar errores al operar con <see cref="IExportService"/>>
/// </summary>
public abstract record ExportError (string Message) : DomainError(Message)
{
    public record ExportHtmlError(string Error) : ExportError($"Error al intentar exportar los datos actuales a html. Error: {Error} ");
    
    public record ExportPdfError(string Error) : ExportError($"Error al intentar exportar los datos actuales a pdf. Error: {Error} ");
}