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
}