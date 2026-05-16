using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV.Models;
using ITV.Service.Citas;
using Serilog;

namespace ITV.ViewModels.Backup;

public partial class BackUpModel : ObservableObject
{
    private readonly IService<int, Cita> _service;
    private readonly ILogger _logger = Log.ForContext<BackUpModel>();

    [ObservableProperty]
    private List<string> _rutas = new();
    
    [ObservableProperty]
    private string? rutaSeleccionada;

    public BackUpModel(IService<int, Cita> service)
    {
        _service = service;
        Listar();
    }
    
    private void Listar()
    {
        Rutas = _service.ListadoBackUps();
        _logger.Information("Se han cargado {Count} rutas", Rutas.Count);
    }

    [RelayCommand]
    private void Crear()
    {
        _logger.Information("Iniciando creación de nuevo Backup");
        var result = _service.GuardarBuckUp();
        
        if (result.IsSuccess)
        {
            _logger.Information("Backup creado exitosamente en: {Path}", result.Value);
            MessageBox.Show("Backup creado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            Listar();
            return;
        }

        _logger.Warning("Error al crear Backup: {Error}", result.Error.Message);
        MessageBox.Show($"Error al crear Backup: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    [RelayCommand]
    private void Restaurar()
    {
        if (RutaSeleccionada == null)  return;

        _logger.Information("Iniciando restauración desde: {Path}", RutaSeleccionada);
        var result = _service.RestaurarBuckUp(RutaSeleccionada);
        
        if (result.IsSuccess)
        {
            _logger.Information("Backup restaurado exitosamente");
            MessageBox.Show("Backup restaurado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            RutaSeleccionada = null;
            return;
        }

        _logger.Warning("Error al restaurar Backup: {Error}", result.Error.Message);
        MessageBox.Show($"Error al restaurar: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    [RelayCommand]
    private void Borrar()
    {
        if (RutaSeleccionada == null)  return;

        var confirmacion = MessageBox.Show(
            "¿Está seguro de que desea borrar este Backup?",
            "Confirmar borrado",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmacion != MessageBoxResult.Yes)
        {
            _logger.Information("Borrado de Backup cancelado por el usuario");
            return;
        }

        _logger.Information("Borrando Backup: {Path}", RutaSeleccionada);
        File.Delete(RutaSeleccionada);
        
        _logger.Information("Backup borrado exitosamente");
        MessageBox.Show("Backup borrado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        RutaSeleccionada = null;
        Listar();
    }
}