using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV.Models;
using ITV.Service.Citas;
using ITV.Storage.CSV;
using ITV.Storage.XML;
using Serilog;

namespace ITV.ViewModels.ExportImport;

public partial class ExportImportModel : ObservableObject
{
    private readonly IService<int, Cita> _service;
    private readonly StorageCitaCsv _csvStorage;
    private readonly StorageCitaJson _jsonStorage;
    private readonly StorageCitaXml _xmlStorage;
    private readonly ILogger _logger = Log.ForContext<ExportImportModel>();

    public IRelayCommand ImportarCsvCommand { get; }
    public IRelayCommand ImportarJsonCommand { get; }
    public IRelayCommand ImportarXmlCommand { get; }
    public IRelayCommand ExportarCsvCommand { get; }
    public IRelayCommand ExportarJsonCommand { get; }
    public IRelayCommand ExportarXmlCommand { get; }

    public ExportImportModel(IService<int, Cita> service, StorageCitaCsv csvStorage, StorageCitaJson jsonStorage,
        StorageCitaXml xmlStorage)
    {
        _service = service;
        _csvStorage = csvStorage;
        _jsonStorage = jsonStorage;
        _xmlStorage = xmlStorage;

        ImportarCsvCommand = new RelayCommand(ImportarCsv);
        ImportarJsonCommand = new RelayCommand(ImportarJson);
        ImportarXmlCommand = new RelayCommand(ImportarXml);
        ExportarCsvCommand = new RelayCommand(ExportarCsv);
        ExportarJsonCommand = new RelayCommand(ExportarJson);
        ExportarXmlCommand = new RelayCommand(ExportarXml);
    }

    private void ImportarCsv()
    {
        var result = _service.Importar(_csvStorage);
        
        if (result.IsSuccess)
        {
            _logger.Information("Citas importadas exitosamente desde CSV. Total: {Count}", result.Value);
            MessageBox.Show($"✅ Importación exitosa\n\nSe importaron {result.Value} citas desde CSV", 
                "Importar CSV", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            _logger.Warning("Error al importar CSV: {Error}", result.Error);
            MessageBox.Show($"❌ Error en la importación\n\n{result.Error}", 
                "Importar CSV", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportarCsv()
    {
        var result = _service.Exportar(_csvStorage);
        
        if (result.IsSuccess)
        {
            _logger.Information("Citas exportadas exitosamente a CSV");
            MessageBox.Show("✅ Exportación exitosa\n\nLas citas se han exportado a CSV", 
                "Exportar CSV", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            _logger.Warning("Error al exportar CSV: {Error}", result.Error);
            MessageBox.Show($"❌ Error en la exportación\n\n{result.Error}", 
                "Exportar CSV", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportarJson()
    {
        var result = _service.Importar(_jsonStorage);
        
        if (result.IsSuccess)
        {
            _logger.Information("Citas importadas exitosamente desde JSON. Total: {Count}", result.Value);
            MessageBox.Show($"✅ Importación exitosa\n\nSe importaron {result.Value} citas desde JSON", 
                "Importar JSON", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            _logger.Warning("Error al importar JSON: {Error}", result.Error);
            MessageBox.Show($"❌ Error en la importación\n\n{result.Error}", 
                "Importar JSON", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportarJson()
    {
        var result = _service.Exportar(_jsonStorage);
        
        if (result.IsSuccess)
        {
            _logger.Information("Citas exportadas exitosamente a JSON");
            MessageBox.Show("✅ Exportación exitosa\n\nLas citas se han exportado a JSON", 
                "Exportar JSON", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            _logger.Warning("Error al exportar JSON: {Error}", result.Error);
            MessageBox.Show($"❌ Error en la exportación\n\n{result.Error}", 
                "Exportar JSON", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportarXml()
    {
        var result = _service.Importar(_xmlStorage);
        
        if (result.IsSuccess)
        {
            _logger.Information("Citas importadas exitosamente desde XML. Total: {Count}", result.Value);
            MessageBox.Show($"✅ Importación exitosa\n\nSe importaron {result.Value} citas desde XML", 
                "Importar XML", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            _logger.Warning("Error al importar XML: {Error}", result.Error);
            MessageBox.Show($"❌ Error en la importación\n\n{result.Error}", 
                "Importar XML", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportarXml()
    {
        var result = _service.Exportar(_xmlStorage);
        
        if (result.IsSuccess)
        {
            _logger.Information("Citas exportadas exitosamente a XML");
            MessageBox.Show("✅ Exportación exitosa\n\nLas citas se han exportado a XML", 
                "Exportar XML", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            _logger.Warning("Error al exportar XML: {Error}", result.Error);
            MessageBox.Show($"❌ Error en la exportación\n\n{result.Error}", 
                "Exportar XML", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}