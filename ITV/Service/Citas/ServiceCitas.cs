using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Error.Common;
using ITV.Error.Vehiculos;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Service.Export;
using ITV.Storage.Common;
using ITV.Utils;
using ITV.Validador;
using Serilog;
using SQLitePCL;

namespace ITV.Service.Citas;

public class ServiceCitas (
    IRepositorioCita repositorio,
    IValidate<Cita> validador
    ) : IService<int, Cita>

{
    private readonly ILogger _logger = Log.ForContext<ServiceCitas>();
    private readonly int _limiteVehciulos = 3;
    
    public ServiceCitas(
            IRepositorioCita repositorio,
            IValidate<Cita> validador,
            bool seed
        ) : this(repositorio, validador) { if (seed)  Seed(); }
    
    //Funciones Crud
    public Result<Cita, DomainError> Agregar(Cita item)
    {
        return Result.Success<Cita, DomainError>(item).
            Tap(_ => _logger.Information("Agregando cita con la matricula: {Matricula}", item.Matricula))
            .Bind(v => validador.Validar(v).Map(_ => v))
            .Map(Estandarizar)
            .Ensure(v => VerificarFechaVehiculo(v.Matricula, v.FechaInspeccion, v.Id), 
                v => new CitaError.FechaYaEstablecida(v.Matricula, v.FechaInspeccion))
            .TapError(err => _logger.Warning("Error: {Error}", err.Message))
            .Ensure(v => ContarVehiculos(v.DniDueño, v.FechaInspeccion, v.Id), 
                v => new CitaError.OwnerWithThreeOrMoreCitas(v.DniDueño, v.FechaInspeccion))
            .TapError(err => _logger.Warning("Error: {Error}", err.Message))
            .Bind(repositorio.Agregar);
    }

    private Cita Estandarizar(Cita item)
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

    public Result<Cita, DomainError> Borrar(int key, bool isLogical = true)
    {
        _logger.Information("Borrando vehiculo con la matrícula: {Matricula}", key);
        return repositorio.BuscarId(key)
            .Bind(v => repositorio.Borrar(v.Id, isLogical));
    }
    
    public  Result<Cita, DomainError> GetById(int key)
    {
        _logger.Information("Buscando vehiculo con matricula: {Matricula}", key);

        return repositorio.BuscarId(key);
    }

    public Result<Cita, DomainError> Actualizar(int key, Cita item)
    {
        return Result.Success<Cita, DomainError>(item)
            .Tap(_ => _logger.Information("Actualizando datos del vehiculo: {Matricula}", key))
            .Bind(v => validador.Validar(v).Map(_ => v))
            .Map(Estandarizar)
            .Ensure(v => repositorio.ExistId(key), new CitaError.CitaNotFoundId(key))
            .Ensure(v => VerificarFechaVehiculo(v.Matricula, v.FechaInspeccion, key),
                v => new CitaError.FechaYaEstablecida(v.Matricula, v.FechaInspeccion))
            .Ensure(v => ContarVehiculos(v.DniDueño, v.FechaInspeccion, key),
                v => new CitaError.OwnerWithThreeOrMoreCitas(v.DniDueño, v.FechaInspeccion))
            .Bind(v => repositorio.Actualizar(key, v));
    }

    public IEnumerable<Cita> GetAll(int pagina = 0, int cantidad = 20)
    {
        _logger.Debug("Obteniendo listado completo de vehículos");
        return repositorio.GetAll()
            .Skip(pagina * cantidad) 
            .Take(cantidad);
    }
    
    /// <summary>
    /// Siembra el repositorio si no hay datos ya en este.
    /// <remarks>
    /// Este método verifica la existencia previa de datos para evitar duplicidad. 
    /// Utiliza un enfoque de <b>integridad atómica</b>: si falla la inserción de cualquier registro 
    /// de prueba, se revierte el estado del repositorio y se aborta la ejecución para prevenir 
    /// un estado inconsistente en el entorno de demostración.
    /// </remarks>
    /// </summary>
    private void Seed()
    {
        repositorio.DeleteAll();
        if (repositorio.GetAll().Any()) return;
        var lista = Factories.FactoryCitas.Seed();
        foreach (var cita in lista)
        {
            var agregado = Agregar(cita);
            if (agregado.IsFailure)
            {
                _logger.Error("Error en el sembrado de datos");
                return;
            }
        }
    }
    
    /// <summary>
    /// Restricción, busca las citas asocidos a un dni en especifico y verifica si no ha alcanzado el límite de citas para un mismo dni en la misma fecha
    /// </summary>
    /// <param name="dni">El dni</param>
    /// <param name="fecha">La fecha</param>
    /// <param name="id">El id de la cita que se está verificando para evitar incogruencias y fallas en las restricciones al actualizar</param>
    /// <returns>True si se puede insertar (no ha llegado al límite)</returns>
    private bool ContarVehiculos(string dni, DateTime fecha, int id)
    {
        var citas = repositorio.GetAll();
        if (citas.Count(v => v.DniDueño.Equals(dni, StringComparison.OrdinalIgnoreCase) && v.FechaInspeccion == fecha && v.Id != id) >= _limiteVehciulos) 
        {
            _logger.Warning("Límite alcanzado: El cliente con DNI {Dni} ya tiene el máximo de vehículos permitidos para la fecha {Fecha}", dni, fecha);
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// Restricción, un mismo vehiculo no puede tener una cita programada para la misma fecha
    /// </summary>
    /// <param name="matricula">La clave para identificar un vehículo</param>
    /// <param name="fecha">La fecha a corroborar</param>
    /// <param name="id">El id de la cita que se está verificando para evitar incogruencias y fallas en las restricciones al actualizar</param>
    /// <returns>True si se puede intertar (el vehículo no tiene una cita programada para esa fecha) (</returns>
    private bool VerificarFechaVehiculo(string matricula, DateTime? fecha, int id)
    {
        var citas = repositorio.GetAll();
        if (citas.Any(c => c.Matricula.Equals(matricula, StringComparison.OrdinalIgnoreCase) && c.FechaInspeccion == fecha && c.Id != id)) 
        {
            _logger.Warning("Límite alcanzado: Ya existe una cita programada para el vehiculo con la matricula {Matricula} con la misma fecha {Fecha}", matricula, fecha);
            return false;
        }
        return true;
    }
}