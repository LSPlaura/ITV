using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace ITV.Views.AcercaDe;

public partial class AcercaDe : Window
{
    public AcercaDe()
    {
        InitializeComponent();
    }
    
    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
       Close();
    }
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch (Exception ex)
        {
           Debug.WriteLine($"Error al abrir URL: {ex.Message}");
        }
    }
}