using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV.Models;
using ITV.Service.Export;
using Serilog;

namespace ITV.ViewModels.Citas;

public partial class ExportCitasModel : ObservableObject
{
    private Cita _cita;
    private IExport<Cita> _exportService;
    private readonly ILogger _logger = Log.ForContext<ExportCitasModel>();

    public IRelayCommand ExportHtmlCommand { get; }
    public IRelayCommand ExportPdfCommand { get; }
    private readonly Action _closeAction;
    public ExportCitasModel(IExport<Cita> citaService, Cita cita, Action closeAction)
    {
        _cita = cita;
        _exportService = citaService;
        ExportHtmlCommand = new RelayCommand(ExportHtml);
        ExportPdfCommand = new RelayCommand(ExportPdf);
        _closeAction = closeAction;
    }

    private void ExportHtml()
    {
        var result = _exportService.ExportHtml(_cita);
    
        if (result.IsFailure)
        {
            _logger.Error("Error exportando HTML: {Error}", result.Error.Message);
            MessageBox.Show($"Error: {result.Error.Message}", "Error de exportación");
        }
        else
        {
            _logger.Information("HTML exportado exitosamente");
            MessageBox.Show($"Archivo exportado correctamente:\n{result.Value}", "Éxito");
        
            _closeAction();
        }
    }
    
    private void ExportPdf()
    {
        _logger.Information("Exportando los datos de la cita con la matricula {Matricula} del dia {Dia} a pdf", _cita.Matricula, _cita.FechaInspeccion);
        var result = _exportService.ExportPdf(_cita);
        if (result.IsFailure)
        {
            _logger.Error("Error al exportar a pdf: {Error}", result.Error.Message);
            MessageBox.Show($"Error: {result.Error.Message}", "Error de exportación");
        }
        else
        {
            _logger.Information("pdf exportado exitosamente");
            MessageBox.Show($"Archivo exportado correctamente:\n{result.Value}", "Éxito");
        
            _closeAction();
        }
    }
}