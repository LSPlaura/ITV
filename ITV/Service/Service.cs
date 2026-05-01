using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Error.Common;
using ITV.Error.Vehiculos;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Storage.Common;
using ITV.Utils;
using ITV.Validador;
using Serilog;

namespace ITV.Service;

public class ServiceVehiculos (
    IRepositorioVehiculos repositorio,
    IBackUpService<Vehiculo> backUpService,
    IStorage<Vehiculo> storage,
    ICache<string, Vehiculo> cache,
    IValidate<Vehiculo> validador) : IService<string, Vehiculo>
{
    private readonly ILogger _logger = Log.ForContext<ServiceVehiculos>();
    
    //Funciones Crud
    public Result<Vehiculo, DomainError> Agregar(Vehiculo item)
    {
        return Result.Success<Vehiculo, DomainError>(item).
            Tap(_ => _logger.Information("Agregando vehiculo con la matricula: {Matricula}", item.Matricula))
            .Bind(v => validador.Validar(v).Map(_ => v))
            .Map(Estandarizar)
            .Bind(repositorio.Agregar);
    }

    private Vehiculo Estandarizar(Vehiculo item)
    {
        var vehiculo = item with
        {
            Matricula = item.Matricula.ToUpper(),
            Marca = item.Marca.ToCapitalize(),
            Modelo = item.Modelo.ToCapitalize(),
            DniDueño = item.DniDueño.ToUpper()
        };
        return vehiculo;
    }

    public Result<Vehiculo, DomainError> Borrar(string key, bool isLogical = true)
    {
        _logger.Information("Borrando vehiculo con la matrícula: {Matricula}", key);
        return repositorio.BuscarMatricula(key)
            .Tap(v => cache.Borrar(v.Matricula))
            .Bind(v => repositorio.Borrar(v.Id, isLogical));
    }
    
    public  Result<Vehiculo, DomainError> GetById(string key)
    {
        _logger.Information("Buscando vehiculo con matricula: {Matricula}", key);

        var cacheado = cache.Obtener(key);
        if (cacheado != null) Result.Success<Vehiculo, DomainError>(cacheado)
            .Tap(v => _logger.Information("El vehiculo con la matricula {Matricula} encontrado", v.Matricula));

        return repositorio.BuscarMatricula(key).Tap(v => cache.Agregar(key, v));
    }
    
    public Result<Vehiculo, DomainError> Actualizar(string key, Vehiculo item)
    {
        return Result.Success<Vehiculo, DomainError>(item).
            Tap(_ =>   _logger.Information("Actualizando datos del vehiculo: {Matricula}", key))
            .Ensure(v => v.Matricula != key, new VehiculoError.InconsistentUpdate(key, item.Matricula))
            .Bind(v => validador.Validar(v).Map(_ => v))
            .Map(Estandarizar)
            .Bind(v => repositorio.Actualizar(repositorio.BuscarMatricula(key).Value.Id, v))
            .Tap(_ => cache.Borrar(key));
    }

    public IEnumerable<Vehiculo> GetAll()
    {
        _logger.Debug("Obteniendo listado completo de vehículos");
        return repositorio.GetAll();
    }

    //Funciones Storage
    public Result<int, DomainError> Importar()
    {
        _logger.Information("Iniciando proceso de importación desde {Ruta}", Configuracion.StorageFilePath);
        return storage.Cargar().Tap(_ => repositorio.DeleteAll()).Bind(AgregarColeccion)
            .Tap(l => _logger.Information("Importación finalizada con éxito. Total: {Count} registros", l));
    }

    public  Result<int, DomainError> Exportar()
    {
        _logger.Information("Iniciando exportación de datos a {Ruta}", Configuracion.StorageFilePath);
        var lista = repositorio.GetAll().ToList();
        return storage.Salvar(lista).Map(_ => lista.Count)
            .Tap(l =>_logger.Information("Exportación completada correctamente. Total: {Count}", l));
    }

    //Funciones buckup service
    public Result<string, DomainError> GuardarBuckUp()
    {
        _logger.Information("Generando copia de seguridad (BackUp)");
        return backUpService.Guardar(repositorio.GetAll())
            .Tap(l =>  _logger.Information("Copia de seguridad guardada en: {Path}", l));
    }

    public Result<int, DomainError> RestaurarBuckUp(string path)
    {
        _logger.Information("Restaurando sistema desde BackUp: {Path}", path);
        return backUpService.Restuarar(path).Tap(_ => repositorio.DeleteAll()).Bind(AgregarColeccion)
            .Tap(l => _logger.Information("Restauración completada satisfactoriamente. Total: {Count}", l));
    }

    private Result<int, DomainError> AgregarColeccion(IEnumerable<Vehiculo> coleccion)
    {
        int contador = 0;
        foreach (var vehiculo in coleccion)
        {
            var agregado = repositorio.Agregar(vehiculo);
            if (agregado.IsFailure) return agregado.Map(_ => contador);
            contador++;
        }
        return Result.Success<int, DomainError>(contador);
    }
}