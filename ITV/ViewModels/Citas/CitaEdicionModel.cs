using CommunityToolkit.Mvvm.ComponentModel;
using ITV.Models;
using ITV.Service.Citas;

namespace ITV.ViewModels.Citas;

public partial class CitaEdicionModel : ObservableObject
{
    [ObservableProperty] private Cita _cita = null!;
    private IService<int, Cita> _citaService = null!;

    public CitaEdicionModel(IService<int, Cita> citaService, Cita cita)
    {
        _cita = cita;
        _citaService = citaService;
    }
}