using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ITV.Models;
using ITV.ViewModels;

namespace ITV.Views.Citas;

public partial class Vista : Window
{
    public Vista(Cita cita)
    {
        InitializeComponent();
        this.DataContext = new CitaVista(cita);
    }
    
    private void Edicion_Click(object sender, RoutedEventArgs e)
    {
        var formulario = new Edicion();
        formulario.ShowDialog();
    }
}