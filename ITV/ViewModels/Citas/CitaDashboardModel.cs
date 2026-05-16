using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV.Models;
using ITV.Service.Citas;
using ITV.Views.Citas;
using Serilog;

namespace ITV.ViewModels.Citas;

public partial class CitaDashboardModel : ObservableObject
{
    private readonly ILogger _logger = Log.ForContext<CitaDashboardModel>();
    private readonly IService<int, Cita> _citasService;
    
    [ObservableProperty] 
    private ObservableCollection<Cita> _lista = new();
    
    [ObservableProperty]
    private Cita? _citaSeleccionada;


    private List<Cita> _citas => _citasService.GetAll(_contador, TamanoPagina).ToList();

    public string NumeroPagina => $"Página {_contador + 1}";

    private int _contador = 0;
    
    partial void OnCitaSeleccionadaChanged(Cita? value)
    {
        if (value != null) Visualizar(value);
    }

    [RelayCommand(CanExecute = nameof(CanPaginaSiguiente))]
    private void PaginaSiguiente()
    {
        _contador++;
        OnPropertyChanged(nameof(NumeroPagina));
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
        LoadCitas();
    }

    [RelayCommand(CanExecute = nameof(CanPaginaAnterior))]
    private void PaginaAnterior()
    {
        _contador--;
        OnPropertyChanged(nameof(NumeroPagina));
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
        LoadCitas();
    }

    private bool CanPaginaSiguiente() => _citas.Count != 0; 

    private bool CanPaginaAnterior() => _contador > 0;
    
    [ObservableProperty]
    private int selectedIndex = 0;
    
    public CitaDashboardModel(IService<int, Cita> citasService)
    {
        _citasService = citasService;
        LoadCitas();
    }

    private void Visualizar(Cita cita)
    {
        _logger.Information("Abriendo vista para cita ID: {Id}", cita.Id);
        var vista = new Vista(cita);
        vista.ShowDialog();
        _logger.Information("Vista cerrada, recargando citas");
        LoadCitas();
    }

    public int TamanoPagina => SelectedIndex switch
    {
        0 => 5,
        1 => 10,
        2 => 15,
        _ => 5
    };


    partial void OnSelectedIndexChanged(int value)
    {
        OnPropertyChanged(nameof(TamanoPagina));
        LoadCitas();
    }

    private void LoadCitas()
    {
        try
        {
            var citas = _citas.Where(c => !c.IsDeleted).ToList();
        
            Lista = new ObservableCollection<Cita>(citas);
            _logger.Information("Se cargaron {Count} citas", citas.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error cargando citas");
            Lista = new ObservableCollection<Cita>();
        }
    }
}