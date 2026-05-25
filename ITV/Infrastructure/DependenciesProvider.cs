using System.Configuration;
using ITV.Cache;
using ITV.Config;
using ITV.Entity;
using ITV.Models;
using ITV.Repository.Ado;
using ITV.Repository.Common;
using ITV.Repository.Dapper;
using ITV.Repository.EFCore;
using ITV.Service;
using ITV.Service.BackUp;
using ITV.Service.Citas;
using ITV.Service.DataService;
using ITV.Service.Export;
using ITV.Storage.Common;
using ITV.Storage.CSV;
using ITV.Storage.XML;
using ITV.Validador;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace ITV.Infrastructure;

/// <summary>
/// Inyector de dependencioas
/// </summary>
public static class DependenciesProvider
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        RegisterRepository(services);
        RegisterStorage(services);
        RegisterValidador(services);
        RegisterCache(services);
        RegisterServices(services);
        return services.BuildServiceProvider();
    }

    private static void RegisterRepository(IServiceCollection services)
    {
        services.AddSingleton<IRepositorioCita>(sp =>
        {
            var repository = Configuracion.RepositoryType.ToLower();
            return repository switch
            {
                "dapper" => new DapperRepository(new SqliteConnection(Configuracion.DataBaseString)),
                "efcore" => new EfCoreRepository(new AppDbContext(Configuracion.DataBaseString)),
                "ado" => new AdoRepository(new SqliteConnection(Configuracion.DataBaseString)),
                _ => new EfCoreRepository(new AppDbContext(Configuracion.DataBaseString)),
            };
        });
    }
    
    private static void RegisterStorage(IServiceCollection services)
    {
        services.AddTransient<IStorage<Cita>>(sp =>
        {
            var repository = Configuracion.RepositoryType.ToLower();
            return repository switch
            {
                "csv" => new StorageCitaCsv(Configuracion.StorageFile, Configuracion.StorageFolder),
                "json" => new StorageCitaJson(Configuracion.StorageFile, Configuracion.StorageFolder),
                "xml" => new StorageCitaXml(Configuracion.StorageFile, Configuracion.StorageFolder),
                _ => new StorageCitaJson(Configuracion.StorageFile, Configuracion.StorageFolder)
            };
        });
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        
        services.AddTransient<IBackUpService<Cita>, BackupService>(sp => new BackupService(sp.GetRequiredService<IStorage<Cita>>(),
            Configuracion.BackUpFile, Configuracion.BackUpFolder));

        services.AddTransient<IReportGenerator<Cita>, ReportGenerator>(sp => new ReportGenerator());
        
        services.AddTransient<IDataService<Cita>, DataService>(sp => 
            new DataService(
                sp.GetRequiredService<IBackUpService<Cita>>(),
                sp.GetRequiredService<IRepositorioCita>()
            )
        );

        services.AddTransient<IService<int, Cita>, ServiceVehiculos>(sp => new ServiceVehiculos(
            sp.GetRequiredService<IRepositorioCita>(),
            sp.GetRequiredService<IValidate<Cita>>(),
            Configuracion.ToSeed
        ));
    }
    
    private static void RegisterCache(IServiceCollection services)
    {
        services.AddTransient<ICache<int, Cita>, LruCache>(sp => new LruCache(Configuracion.Cache));
    }

    private static void RegisterValidador(IServiceCollection services)
    {
        services.AddTransient<IValidate<Cita>, ValidadorCita>(sp => new ValidadorCita());
    }
}