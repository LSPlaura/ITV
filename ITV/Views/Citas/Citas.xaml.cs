using System.Windows;
using System.Windows.Controls;

namespace ITV.Views.Citas;

public partial class Citas : Page
{
    public Citas()
    {
        InitializeComponent();
    }
    
    private void Formulario_Click(object sender, RoutedEventArgs e)
    {
        var formulario = new Formulario();
        formulario.ShowDialog();
    }
}