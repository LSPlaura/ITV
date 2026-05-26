using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using ITV.Config;
using ITV.Infrastructure;
using Serilog;

namespace ITV;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    private readonly ILogger _logger = Log.ForContext<App>();

    protected override void OnStartup(StartupEventArgs e) {
        _logger.Information("Iniciando la aplicaión");
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuracion.Config) 
            .CreateLogger();
        ServiceProvider = DependenciesProvider.BuildServiceProvider();
        
        
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e) {
        _logger.Information("Aplicación cerrándose");
        base.OnExit(e);
    }
}