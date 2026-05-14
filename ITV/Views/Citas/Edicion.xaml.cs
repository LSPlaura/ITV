using System.Windows;
using System.Windows.Controls;
using ITV.Models;
using ITV.Service.Citas;
using ITV.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ITV.Views.Citas;

public partial class Edicion : Window
{
    public Edicion(Cita cita)
    {
        InitializeComponent();
        this.DataContext = new CitaEdicionModel(App.ServiceProvider.GetRequiredService<IService<int, Cita>>(), cita);
    }
}