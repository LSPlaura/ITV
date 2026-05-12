using System.Windows;
using System.Windows.Controls;

namespace ITV.Views.Citas;

public partial class Formulario : Window
    
{
    public Formulario()
    {
        InitializeComponent();
        var vista = new Vista();
        vista.ShowDialog();
    }
}