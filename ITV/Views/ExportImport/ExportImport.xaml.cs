using System.Windows;
using System.Windows.Controls;

namespace ITV.Views.ExportImport;

public partial class ExportImport : Page
{
    public ExportImport()
    {
        InitializeComponent();
    }

    private void ExportarXml_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Exportando a XML...");
        // aquí tu lógica real
    }

    private void ExportarJson_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Exportando a JSON...");
    }

    private void ExportarCsv_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Exportando a CSV...");
    }
    
    private void ImportarXml_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Exportando a XML...");
        // aquí tu lógica real
    }

    private void ImportarJson_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Exportando a JSON...");
    }

    private void ImportarCsv_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Exportando a CSV...");
    }
}