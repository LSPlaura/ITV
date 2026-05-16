using System.Windows;
using System.Windows.Controls;
using ITV.Config;
using ITV.Models;
using ITV.Service.Citas;
using ITV.Storage.CSV;
using ITV.Storage.XML;
using ITV.ViewModels.Citas;
using ITV.ViewModels.ExportImport;
using Microsoft.Extensions.DependencyInjection;

namespace ITV.Views.ExportImport;

public partial class ExportImport : Page
{
    public ExportImport()
    {
        InitializeComponent();
        DataContext = new ExportImportModel(App.ServiceProvider.GetRequiredService<IService<int, Cita>>(),
            new StorageCitaCsv(Configuracion.StorageFile, Configuracion.StorageFolder),
            new StorageCitaJson(Configuracion.StorageFile, Configuracion.StorageFolder),
            new StorageCitaXml(Configuracion.StorageFile, Configuracion.StorageFolder)
            );
    }
}