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

    public CitaVistaModel(IService<int, Cita> citaService, Cita cita)
    {
        Cita = cita;
        _citaService = citaService;
        EditarCommand = new RelayCommand(Editar);
        BorrarCommand = new RelayCommand(Borrar);
        ExportarCommand = new RelayCommand(Exportar);
    }

    private void Editar()
    {
        var edicionVista = new Edicion(Cita);
        edicionVista.ShowDialog();
    }

    private void Borrar()
    {
        _logger.Information("Intentando borrar cita con el ID {Id}", Cita.Id);
        var result = _citaService.Borrar(Cita.Id, Configuracion.DeleteType);

        if (result.IsFailure) MessageBox.Show(result.Error.Message);
        else MessageBox.Show($"Se ha borrado la cita con la matrícula {result.Value.Matricula} del día {result.Value.FechaInspeccion}");
        
        // ✅ CIERRA LA VENTANA DESPUÉS
        Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)
            ?.Close();
    }
    
    private void Exportar()
    {
        var exportarVista = new ExportCitas(Cita);
        exportarVista.ShowDialog();
    }
}