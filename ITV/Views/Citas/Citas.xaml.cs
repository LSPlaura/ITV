using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ITV.Models;
using ITV.Service.Citas;
using ITV.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using CitaDashboardModel = ITV.ViewModels.Citas.CitaDashboardModel;

namespace ITV.Views.Citas;

public partial class Citas : Page
{
    public Citas()
    {
        InitializeComponent();
        var vm = new CitaDashboardModel(App.ServiceProvider.GetRequiredService<IService<int, Cita>>());
        DataContext = vm;
    }
    
    // private void DgCitas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    // {
    //     if (DgCitas.SelectedItem is Cita citaSeleccionada)
    //     {
    //         var vista = new Vista(citaSeleccionada);
    //         vista.ShowDialog();
    //     }
    // }
}