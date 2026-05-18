using System.Windows;
using ITV.Models;
using ITV.Service;
using ITV.Service.Export;
using ITV.ViewModels;
using ITV.ViewModels.Citas;
using Microsoft.Extensions.DependencyInjection;

namespace ITV.Views.ExportCitas;

public partial class ExportCitas : Window
{
    public ExportCitas(Cita cita)
    {
        InitializeComponent();
        DataContext = new ExportCitasModel(App.ServiceProvider.GetRequiredService<IExport<Cita>>(), cita, this.Close);
    }
}