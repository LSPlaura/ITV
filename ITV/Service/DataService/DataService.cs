using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using ITV.Config;
using ITV.Error.Common;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Service.Citas;
using ITV.Storage.Common;
using Serilog;

namespace ITV.Service.DataService;

public class DataService(IBackUpService<Cita> backUp, ICrud<int, Cita> repositorio) : IDataService<Cita>
{
    private readonly ILogger _logger = Log.ForContext<ServiceVehiculos>();
    
    public Result<int, DomainError> Importar(IStorage<Cita> storage)
    {
        _logger.Information("Iniciando proceso de importación desde {Ruta}", Configuracion.StorageFilePath);
        return storage.Cargar().Tap(_ => repositorio.DeleteAll()).Bind(AgregarColeccion)
            .Tap(l => _logger.Information("Importación finalizada con éxito. Total: {Count} registros", l));
    }

    public  Result<int, DomainError> Exportar(IStorage<Cita> storage)
    {
        _logger.Information("Iniciando exportación de datos a {Ruta}", Configuracion.StorageFilePath);
        var lista = repositorio.GetAll().ToList();
        return storage.Salvar(lista).Map(_ => lista.Count)
            .Tap(l =>_logger.Information("Exportación completada correctamente. Total: {Count}", l));
    }

    //Funciones buckup service
    public Result<string, DomainError> GuardarBuckUp()
    {
        _logger.Information("Generando copia de seguridad (BackUps)");
        return backUp.Guardar(repositorio.GetAll())
            .Tap(l =>  _logger.Information("Copia de seguridad guardada en: {Path}", l));
    }

    public Result<int, DomainError> RestaurarBuckUp(string path)
    {
        _logger.Information("Restaurando sistema desde BackUps: {Path}", path);
        return backUp.Restuarar(path).Tap(_ => repositorio.DeleteAll()).Bind(AgregarColeccion)
            .Tap(l => _logger.Information("Restauración completada satisfactoriamente. Total: {Count}", l));
    }

    public List<string> ListadoBackUps()
    {
        _logger.Information("Listando las rutas de todos los BackUps");
        return backUp.Listar().ToList();
    }
    
    private Result<int, DomainError> AgregarColeccion(IEnumerable<Cita> coleccion)
    {
        int contador = 0;
        foreach (var cita in coleccion)
        {
            var agregado = repositorio.Agregar(cita);
            if (agregado.IsFailure) return agregado.Map(_ => contador);
            contador++;
        }
        return Result.Success<int, DomainError>(contador);
    }
}