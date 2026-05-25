using ITV.Error.Common;
using ITV.Service.Export;

namespace ITV.Error.Export;

/// <summary>
/// Record para capturar errores al operar con <see cref="ReportGenerator"/>>
/// </summary>
public abstract record ReportError (string Message) : DomainError(Message)
{
    public record ReportHtmlError(string Error) : ReportError($"Error al intentar exportar los datos actuales a html. Error: {Error} ");
    
    public record ReportPdfError(string Error) : ReportError($"Error al intentar exportar los datos actuales a pdf. Error: {Error} ");
}