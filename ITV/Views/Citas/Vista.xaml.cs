using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ITV.Models;
using ITV.Service.Citas;
using ITV.ViewModels;
using ITV.ViewModels.Citas;
using Microsoft.Extensions.DependencyInjection;
using CitaVistaModel = ITV.ViewModels.Citas.CitaVistaModel;

namespace ITV.Views.Citas;

public partial class Vista : Window
{
    public Vista(Cita cita)
    {
        InitializeComponent();
        this.DataContext = new CitaVistaModel(App.ServiceProvider.GetRequiredService<IService<int, Cita>>(), cita);
    }
}