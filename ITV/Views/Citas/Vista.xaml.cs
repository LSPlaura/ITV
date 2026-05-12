using System.Windows;
using System.Windows.Controls;

namespace ITV.Views.Citas;

public partial class Vista : Window
{
    public Vista()
    {
        InitializeComponent();
    }
    
    private void Edicion_Click(object sender, RoutedEventArgs e)
    {
        var formulario = new Edicion();
        formulario.ShowDialog();
    }
}