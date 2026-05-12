using System.Windows;

namespace ITV.Views.AcercaDe;

public partial class AcercaDe : Window
{
    public AcercaDe()
    {
        InitializeComponent();
    }
    
    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
       this.Close();
    }
}