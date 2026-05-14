using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using ITV.Infrastructure;
using Serilog;

namespace ITV;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    private readonly ILogger _logger = Log.ForContext<App>();

    protected override void OnStartup(StartupEventArgs e) {
        _logger.Information("Iniciando la aplicaión");
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        
        ServiceProvider = DependenciesProvider.BuildServiceProvider();
        
        //si me da tiempo
        // var splash = new SplashWindow();
        // splash.ShowDialog();
        
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e) {
        _logger.Information("👋 Aplicación cerrándose");
        base.OnExit(e);
    }
}