using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ITV.Views.AcercaDe;
using ITV.Views.BackUp;
using ITV.Views.Citas;
using ITV.Views.ExportImport;

namespace ITV;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Citas_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Citas());
    }
    
    private void Importar_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new BackUps());
    }
    
    private void Exportar_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new ExportImport());
    }
    
    private void Acercade_Click(object sender, RoutedEventArgs e)
    {
        var info = new AcercaDe();
        info.ShowDialog();
    }
}