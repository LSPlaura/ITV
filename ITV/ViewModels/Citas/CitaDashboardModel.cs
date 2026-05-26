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
    
    [ObservableProperty] private string _matricula = string.Empty;
    [ObservableProperty] private string _dni = string.Empty;
    [ObservableProperty] private string _marca = string.Empty;
    [ObservableProperty] private string _modelo = string.Empty;
    [ObservableProperty] private DateTime? _fechaMatriculacionDesde;
    [ObservableProperty] private DateTime? _fechaMatriculacionHasta;
    [ObservableProperty] private DateTime? _fechaInspeccionDesde;
    [ObservableProperty] private DateTime? _fechaInspeccionHasta;
    [ObservableProperty] private string _tipoMotor = "Todos los motores";
    
    [ObservableProperty] private ObservableCollection<Cita> _lista = new();
    [ObservableProperty] private Cita? _citaSeleccionada;
    [ObservableProperty] private int _selectedIndex = 0;

    private int _contador = 0;
    private bool _puedePasarSiguiente = false;

    public string NumeroPagina => $"Página {_contador + 1}";
    public int TamanoPagina => SelectedIndex switch
    {
        0 => 5,
        1 => 10,
        2 => 15,
        _ => 5
    };
    
    public IRelayCommand NuevaCitaCommand { get; }

    public CitaDashboardModel(IService<int, Cita> citasService)
    {
        _citasService = citasService;
        NuevaCitaCommand = new RelayCommand(NuevaCita);
        _logger.Debug("Inicializando CitaDashboardModel");
        LoadCitas();
    }

    private void NuevaCita()
    {
        _logger.Information("Abriendo formulario para nueva cita");
        var view = new Formulario(new Cita(), true);
        view.ShowDialog();
        _logger.Information("Formulario de nueva cita cerrado, recargando datos");
        LoadCitas();
    }

    partial void OnMatriculaChanged(string value) => ResetearPaginaYFiltrar("Matrícula", value);
    partial void OnDniChanged(string value) => ResetearPaginaYFiltrar("DNI", value);
    partial void OnMarcaChanged(string value) => ResetearPaginaYFiltrar("Marca", value);
    partial void OnModeloChanged(string value) => ResetearPaginaYFiltrar("Modelo", value);
    partial void OnFechaMatriculacionDesdeChanged(DateTime? value) => ResetearPaginaYFiltrar("FechaMatriculacionDesde", value?.ToString("dd/MM/yyyy"));
    partial void OnFechaMatriculacionHastaChanged(DateTime? value) => ResetearPaginaYFiltrar("FechaMatriculacionHasta", value?.ToString("dd/MM/yyyy"));
    partial void OnFechaInspeccionDesdeChanged(DateTime? value) => ResetearPaginaYFiltrar("FechaInspeccionDesde", value?.ToString("dd/MM/yyyy"));
    partial void OnFechaInspeccionHastaChanged(DateTime? value) => ResetearPaginaYFiltrar("FechaInspeccionHasta", value?.ToString("dd/MM/yyyy"));
    partial void OnTipoMotorChanged(string value) => ResetearPaginaYFiltrar("TipoMotor", value);

    private void ResetearPaginaYFiltrar(string propiedad, string? valor)
    {
        _logger.Debug("Cambio detectado en filtro: {Propiedad} = {Valor}. Reiniciando índice de página", propiedad, valor ?? "null");
        _contador = 0;
        OnPropertyChanged(nameof(NumeroPagina));
        Filtrar();
    }

    partial void OnCitaSeleccionadaChanged(Cita? value)
    {
        if (value != null) 
        {
            _logger.Information("Fila seleccionada detectada para Cita ID: {Id}", value.Id);
            Visualizar(value);
        }
    }

    partial void OnSelectedIndexChanged(int value)
    {
        _logger.Debug("Cambio de tamaño de página seleccionado. Index: {Index}", value);
        OnPropertyChanged(nameof(TamanoPagina));
        _contador = 0;
        OnPropertyChanged(nameof(NumeroPagina));
        Filtrar();
    }

    private void LoadCitas()
    {
        Filtrar();
    }
    
    [RelayCommand]
    private void Filtrar()
    {
        try
        {
            _logger.Debug("Ejecutando proceso de filtrado y paginación masiva");
            var todasLasCitas = _citasService.GetAll(0, int.MaxValue).Where(c => !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(Matricula))
                todasLasCitas = todasLasCitas.Where(c => c.Matricula.Contains(Matricula, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Dni))
                todasLasCitas = todasLasCitas.Where(c => c.DniDueño.Contains(Dni, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Marca))
                todasLasCitas = todasLasCitas.Where(c => c.Marca.Contains(Marca, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Modelo))
                todasLasCitas = todasLasCitas.Where(c => c.Modelo.Contains(Modelo, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(TipoMotor) && !TipoMotor.Equals("Todos los motores", StringComparison.OrdinalIgnoreCase))
                todasLasCitas = todasLasCitas.Where(c => c.Motor.ToString().Equals(TipoMotor, StringComparison.OrdinalIgnoreCase));

            if (FechaInspeccionDesde != null) 
                todasLasCitas = todasLasCitas.Where(c => c.FechaInspeccion >= FechaInspeccionDesde.Value.Date);

            if (FechaInspeccionHasta != null) 
                todasLasCitas = todasLasCitas.Where(c => c.FechaInspeccion <= FechaInspeccionHasta.Value.Date.AddDays(1).AddTicks(-1));

            if (FechaMatriculacionDesde != null) 
                todasLasCitas = todasLasCitas.Where(c => c.FechaMatriculacion >= FechaMatriculacionDesde.Value.Date);

            if (FechaMatriculacionHasta != null) 
                todasLasCitas = todasLasCitas.Where(c => c.FechaMatriculacion <= FechaMatriculacionHasta.Value.Date.AddDays(1).AddTicks(-1));

            var listaFiltradaCompleta = todasLasCitas.ToList();
            _logger.Debug("Filtro aplicado. Registros coincidentes: {Count}", listaFiltradaCompleta.Count);

            var datosPaginados = listaFiltradaCompleta
                .Skip(_contador * TamanoPagina)
                .Take(TamanoPagina)
                .ToList();

            Lista = new ObservableCollection<Cita>(datosPaginados);
            
            _puedePasarSiguiente = ((_contador + 1) * TamanoPagina) < listaFiltradaCompleta.Count;

            PaginaSiguienteCommand.NotifyCanExecuteChanged();
            PaginaAnteriorCommand.NotifyCanExecuteChanged();
            
            _logger.Debug("Paginación asignada. Mostrando {Count} registros en {Pagina}", Lista.Count, NumeroPagina);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error crítico durante la operación de filtrado y paginación de citas");
            Lista = new ObservableCollection<Cita>();
        }
    }

    [RelayCommand]
    private void LimpiarFiltros()
    {
        _logger.Information("Comando LimpiarFiltros ejecutado");
        Matricula = string.Empty;
        Dni = string.Empty;
        Marca = string.Empty;
        Modelo = string.Empty;
        FechaMatriculacionDesde = null;
        FechaMatriculacionHasta = null;
        FechaInspeccionDesde = null;
        FechaInspeccionHasta = null;
        TipoMotor = "Todos los motores";
    }
    
    [RelayCommand(CanExecute = nameof(CanPaginaSiguiente))]
    private void PaginaSiguiente()
    {
        _contador++;
        _logger.Debug("Avanzando a la página index: {Index}", _contador);
        OnPropertyChanged(nameof(NumeroPagina));
        LoadCitas();
    }

    [RelayCommand(CanExecute = nameof(CanPaginaAnterior))]
    private void PaginaAnterior()
    {
        _contador--;
        _logger.Debug("Retrocediendo a la página index: {Index}", _contador);
        OnPropertyChanged(nameof(NumeroPagina));
        LoadCitas();
    }

    private bool CanPaginaSiguiente() => _puedePasarSiguiente; 
    private bool CanPaginaAnterior() => _contador > 0;

    private void Visualizar(Cita cita)
    {
        _logger.Information("Abriendo ventana de detalle para Cita ID: {Id}", cita.Id);
        var vista = new Vista(cita);
        vista.ShowDialog();
        Filtrar();
    }
}