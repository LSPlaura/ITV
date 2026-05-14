using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ITV.Models;
using ITV.Service.Citas;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ITV.ViewModels;

public partial class CitaDataGrip : ObservableObject
{
    private readonly IService<int, Cita> _citasService;
    [ObservableProperty] private ObservableCollection<Cita> lista = new();
    private List<Cita> _citas = new List<Cita>();
    public CitaDataGrip(IService<int, Cita> citasService)
    {
        _citasService = citasService;
        Load();
    }

    private void Load()
    {
        _citas = _citasService.GetAll().ToList();
        Lista = new ObservableCollection<Cita>(_citas);
    }
}