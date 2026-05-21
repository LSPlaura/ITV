using System.Windows.Controls;
using ITV.Models;
using ITV.Service;
using ITV.Service.Citas;
using ITV.Service.DataService;
using ITV.ViewModels.Backup;
using ITV.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ITV.Views.BackUp;

public partial class BackUps : Page
{
    public BackUps()
    {
        InitializeComponent();
        DataContext = new BackUpModel(App.ServiceProvider.GetRequiredService<IDataService<Cita>>());
    }
}