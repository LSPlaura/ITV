using CommunityToolkit.Mvvm.ComponentModel;
using ITV.Models;

namespace ITV.ViewModels;

public partial class CitaVista : ObservableObject
{
    [ObservableProperty]
    private Cita _cita = null!;

    public CitaVista(Cita cita)
    {
        Cita = cita;
    }
}