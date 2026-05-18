using System.Windows;
using System.Windows.Controls;
using ITV.Models;
using ITV.Service.Citas;
using ITV.ViewModels;
using ITV.ViewModels.Citas;
using Microsoft.Extensions.DependencyInjection;

namespace ITV.Views.Citas;

public partial class Formulario : Window
{
    public Formulario(Cita cita, bool isNew)
    {
        InitializeComponent();
        this.DataContext = new CitaFormularioModel(App.ServiceProvider.GetRequiredService<IService<int, Cita>>(), cita, this.Close, isNew);
    }
}