using System.Windows;
using System.Windows.Controls;
using ITV.Models;
using ITV.Service.Citas;
using ITV.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ITV.Views.Citas;

public partial class Citas : Page
{
    public Citas()
    {
        InitializeComponent();
        var vm = new CitaDataGrip(App.ServiceProvider.GetRequiredService<IService<int, Cita>>());
        DataContext = vm;
    }
}