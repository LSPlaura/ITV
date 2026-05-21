using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV.Factories.Provider;
using ITV.Models;
using ITV.Service.Citas;
using ITV.Service.DataService;
using ITV.Storage.Common;
using ITV.Storage.CSV;
using ITV.Storage.XML;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ITV.ViewModels.ExportImport;

public partial class ExportImportModel : ObservableObject
{
    private readonly IDataService<Cita> _service;
    private readonly ILogger _logger = Log.ForContext<ExportImportModel>();

    public ExportImportModel(IDataService<Cita> service, StorageCitaCsv csvStorage, StorageCitaJson jsonStorage, StorageCitaXml xmlStorage)
    {
        _service = service;
    }

    [RelayCommand] private void ImportarCsv()  => EjecutarImportacion("CSV");
    [RelayCommand] private void ImportarJson() => EjecutarImportacion("JSON");
    [RelayCommand] private void ImportarXml()  => EjecutarImportacion("XML");
    [RelayCommand] private void ExportarCsv()  => EjecutarExportacion("CSV");
    [RelayCommand] private void ExportarJson() => EjecutarExportacion("JSON");
    [RelayCommand] private void ExportarXml()  => EjecutarExportacion("XML");
    

    private void EjecutarImportacion(string formato)
    {
        var result = _service.Importar(StorageFactory.ObtenerStorage(formato));
        
        if (result.IsSuccess)
        {
            _logger.Information("Citas importadas desde {Formato}. Total: {Count}", formato, result.Value);
            
            MessageBox.Show(
                $"Se han importado {result.Value} citas correctamente.", 
                "Importación finalizada", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information
            );
        }
        else
        {
            _logger.Warning("Error al importar {Formato}: {Error}", formato, result.Error);
            
            MessageBox.Show(
                $"No se pudieron importar los datos:\n{result.Error}", 
                "Error de importación", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error
            );
        }
    }

    private void EjecutarExportacion(string formato)
    {
        var result = _service.Exportar(StorageFactory.ObtenerStorage(formato));
        
        if (result.IsSuccess)
        {
            _logger.Information("Citas exportadas a {Formato}", formato);
            
            MessageBox.Show(
                "Los datos se han exportado correctamente.", 
                "Exportación finalizada", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information
            );
        }
        else
        {
            _logger.Warning("Error al exportar {Formato}: {Error}", formato, result.Error);
            
            MessageBox.Show(
                $"No se pudo completar la exportación:\n{result.Error}", 
                "Error de exportación", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error
            );
        }
    }
}