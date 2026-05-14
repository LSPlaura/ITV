using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ITV.Models;
using ITV.Service.Citas;
using ITV.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ITV.Views.Citas;

public partial class Vista : Window
{
    public Vista(Cita cita)
    {
        InitializeComponent();
        this.DataContext = new CitaVistaModel(App.ServiceProvider.GetRequiredService<IService<int, Cita>>(), cita);
    }
}