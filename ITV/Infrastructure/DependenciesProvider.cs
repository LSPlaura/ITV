using ITV.Cache;
using ITV.Config;
using ITV.Entity;
using ITV.Models;
using ITV.Repository.Ado;
using ITV.Repository.Common;
using ITV.Repository.Dapper;
using ITV.Repository.EFCore;
using ITV.Repository.JSON;
using ITV.Repository.Memory;
using ITV.Service;
using ITV.Storage.Common;
using ITV.Storage.CSV;
using ITV.Storage.XML;
using ITV.Validador;
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
        services.AddSingleton<IRepositorioVehiculos>(sp =>
        {
            var repository = Configuracion.RepositoryType.ToLower();
            return repository switch
            {
                "memory" => new RepositorioEnMemoria(),
                "json" => new RepositorioJson(),
                "dapper" => new DapperRepository(),
                "efcore" => new EfCoreRepository(new AppDbContext(Configuracion.DataBaseString)),
                "ado" => new AdoRepository(),
                _ => new RepositorioEnMemoria()
            };
        });
    }
    
    private static void RegisterStorage(IServiceCollection services)
    {
        services.AddTransient<IStorage<Vehiculo>>(sp =>
        {
            var repository = Configuracion.RepositoryType.ToLower();
            return repository switch
            {
                "csv" => new StorageVehiculoCsv(Configuracion.StorageFile, Configuracion.StorageFolder),
                "json" => new StorageVehiculoJson(Configuracion.StorageFile, Configuracion.StorageFolder),
                "xml" => new StorageVehiculoXml(Configuracion.StorageFile, Configuracion.StorageFolder),
                _ => new StorageVehiculoJson(Configuracion.StorageFile, Configuracion.StorageFolder)
            };
        });
    }
    
    private static void RegisterValidador(IServiceCollection services)
    {
        services.AddTransient<IBuckUpServiceVehiculos, BackupService>(sp => 
            new BackupService(sp.GetRequiredService<IStorage<Vehiculo>>()));

        services.AddTransient<IService<string, Vehiculo>, ServiceVehiculos>(sp => new ServiceVehiculos(
            sp.GetRequiredService<IRepositorioVehiculos>(),
            sp.GetRequiredService<IBuckUpServiceVehiculos>(),
            sp.GetRequiredService<IStorage<Vehiculo>>(),
            sp.GetRequiredService<ICache<string, Vehiculo>>(),
            sp.GetRequiredService<IValidate<Vehiculo>>()
        ));
    }
    
    private static void RegisterCache(IServiceCollection services)
    {
        services.AddTransient<ICache<string, Vehiculo>, LruCache>(sp => new LruCache(Configuracion.Cache));
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IValidate<Vehiculo>, ValidadorVehiculo>(sp => new ValidadorVehiculo());
    }
}