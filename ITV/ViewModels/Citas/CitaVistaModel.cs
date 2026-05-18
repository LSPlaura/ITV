using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV.Config;
using ITV.Models;
using ITV.Service.Citas;
using ITV.Views.Citas;
using ITV.Views.ExportCitas;
using Serilog;

namespace ITV.ViewModels.Citas;

public partial class CitaVistaModel : ObservableObject
{
    private readonly ILogger _logger = Log.ForContext<CitaVistaModel>();
    private IService<int, Cita> _citaService = null!;
    
    [ObservableProperty]
    private Cita _cita = null!;

    public IRelayCommand EditarCommand { get; }
    public IRelayCommand BorrarCommand { get; }
    public IRelayCommand ExportarCommand { get; }
    
    private readonly Action _closeAction;

    public CitaVistaModel(IService<int, Cita> citaService, Cita cita, Action closeAction)
    {
        Cita = cita;
        _citaService = citaService;
        EditarCommand = new RelayCommand(Editar);
        BorrarCommand = new RelayCommand(Borrar);
        ExportarCommand = new RelayCommand(Exportar);
        _closeAction = closeAction;
        
        _logger.Debug("Inicializando CitaVistaModel para Cita ID: {Id}", cita.Id);
    }

    private void Editar()
    {
        _logger.Information("Abriendo formulario de edición para Cita ID: {Id}", Cita.Id);
        var edicionVista = new Formulario(Cita, false);
        edicionVista.ShowDialog();
        
        _logger.Information("Formulario de edición cerrado. Cerrando vista de detalle de Cita ID: {Id}", Cita.Id);
        _closeAction();
    }

    private void Borrar()
    {
        _logger.Information("Iniciando proceso de borrado para Cita ID: {Id}. Tipo de borrado: {DeleteType}", Cita.Id, Configuracion.DeleteType);
        var result = _citaService.Borrar(Cita.Id, Configuracion.DeleteType);

        if (result.IsFailure)
        {
            _logger.Error("Error al borrar la Cita ID: {Id}. Motivo: {ErrorMessage}", Cita.Id, result.Error.Message);
            MessageBox.Show(result.Error.Message);
        }
        else
        {
            _logger.Information("Cita ID: {Id} borrada correctamente. Matrícula: {Matricula}", Cita.Id, result.Value.Matricula);
            MessageBox.Show($"Se ha borrado la cita con la matrícula {result.Value.Matricula} del día {result.Value.FechaInspeccion}");
        }

        _logger.Debug("Ejecutando acción de cierre tras intento de borrado");
        _closeAction();
    }
    
    private void Exportar()
    {
        _logger.Information("Abriendo ventana de exportación para Cita ID: {Id}", Cita.Id);
        var exportarVista = new ExportCitas(Cita);
        exportarVista.ShowDialog();
        _logger.Debug("Ventana de exportación cerrada para Cita ID: {Id}", Cita.Id);
    }
}