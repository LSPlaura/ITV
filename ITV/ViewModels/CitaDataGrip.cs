using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV.Models;
using ITV.Service.Citas;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ITV.ViewModels;

public partial class CitaDataGrip : ObservableObject
{
    private readonly IService<int, Cita> _citasService;
    
    [ObservableProperty] 
    private ObservableCollection<Cita> _lista = new();

    private List<Cita> _citas => _citasService.GetAll(_contador, TamanoPagina).ToList();

    public string NumeroPagina => $"Página {_contador + 1}";

    private int _contador = 0;

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
    
    public CitaDataGrip(IService<int, Cita> citasService)
    {
        _citasService = citasService;
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
            Lista = new ObservableCollection<Cita>(_citas);
        }
        catch (Exception)
        {
            Lista = new ObservableCollection<Cita>();
        }
    }
    
    
}