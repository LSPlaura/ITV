using System.IO;
using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Entity;
using ITV.Error.Common;
using ITV.Error.DataBase;
using ITV.Error.Vehiculos;
using ITV.Mappers;
using ITV.Models;
using ITV.Repository.Common;
using Serilog;

namespace ITV.Repository.EFCore;

public class EfCoreRepository : IRepositorioVehiculos
{
    private readonly ILogger _logger = Log.ForContext<EfCoreRepository>();
    private readonly AppDbContext _context;
    private readonly int _limiteVehciulos = 3;

    public EfCoreRepository(AppDbContext context)
    {
        _context = context;
        EnsureDirectory();
        if (_context.Database.CanConnect())
        {
            _context.Database.EnsureCreated();
        }
    }

    private void EnsureDirectory()
    {
        if (!Directory.Exists(Configuracion.RepositoryFolder))
        {
            _logger.Information("Se ha creado el directorio {Directorio}", Configuracion.RepositoryFolder);
            Directory.CreateDirectory(Configuracion.RepositoryFolder);
        }
    }
    

    public IEnumerable<Vehiculo> GetAll()
    {
        try
        {
            var query = _context.Vehiculo.AsQueryable();
            return query.ToModel();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al obtener los vehiculos");
            return Enumerable.Empty<Vehiculo>();
        }
    }

    public Result<Vehiculo, DomainError> Agregar(Vehiculo value)
    {
        try
        {
            var entity = value.ToEntity();
            if (ExistMatricula(entity.Matricula))
                return Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoAlredyExist.MatriculaAlreadyExists(entity.Matricula))
                    .TapError(v => _logger.Error("Fallo al agregar: La matricula {Matricula} ya está registrada", entity.Matricula));
            
            if (!ContarVehiculos(entity.DniDueño))
                return Result.Failure<Vehiculo, DomainError>(new VehiculoError.OwnerWithThreeOrMoreVehiculos(entity.DniDueño))
                    .TapError(v => _logger.Error("Límite alcanzado: El dueño con DNI {Dni} no puede tener más vehículos", entity.DniDueño));
            
            _context.Vehiculo.Add(entity);
            _context.SaveChanges();
            return Result.Success<VehiculoEntity, DomainError>(entity).Map(v => v.ToModel())
                .Tap((l => _logger.Information("Se ha añadido el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al agregar"));
        }
    }

    public Result<Vehiculo, DomainError> Borrar(int key, bool isLogical = true)
    {
        try
        {
            var entity = _context.Vehiculo.Find(key);
            if (entity == null)
                return Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundId(key))
                    .TapError((l => _logger.Error("No se ha encontrado el vehiculo con el ID al intentar borrarlo{Id}", key)));

            if (isLogical)
            {
                entity.IsDeleted = 1;
                _context.SaveChanges();
                return Result.Success<Vehiculo, DomainError>(entity.ToModel())
                    .Tap(l => _logger.Information("Se ha borrado (lógico) el vehiculo con el ID {Id}", key));
            }

            _context.Vehiculo.Remove(entity);
            _context.SaveChanges();
            return Result.Success<Vehiculo, DomainError>(entity.ToModel())
                .Tap(l => _logger.Information("Se ha borrado (físico) el vehiculo con el ID {Id}", key));
        }
        catch (Exception ex)
        {
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al borrar"));
        }
    }

    public Result<Vehiculo, DomainError> BuscarId(int key)
    {
        try
        {
            var entity = _context.Vehiculo.Find(key);
            
            return entity == null
                ? Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundId(key))
                    .TapError((l => _logger.Error("No se ha encontrado el vehiculo con el ID {Id}", key)))
                : Result.Success<VehiculoEntity, DomainError>(entity).Map(v => v.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al buscar por id"));
        }
    }

    public Result<Vehiculo, DomainError> BuscarMatricula(string key)
    {
        try
        {
            var entity = _context.Vehiculo.Find(key);

            return entity == null
                ? Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundMatricula(key))
                    .TapError((l =>
                        _logger.Error("No se ha encontrado el vehiculo con la matricula {Matricula}", key)))
                : Result.Success<VehiculoEntity, DomainError>(entity).Map(v => v.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con la matricula {Matricula}",
                        l.Matricula)));
        }
        catch (Exception ex)
        {
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al buscar por matrícula"));
        }
    }

    public Result<Vehiculo, DomainError> Actualizar(int key, Vehiculo value)
    {
        try
        {
            var existente = _context.Vehiculo.Find(key);
            if (existente == null)
                return Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundId(key))
                    .TapError((l => _logger.Error("No se ha encontrado el vehiculo con el ID al intentar borrarlo{Id}", key)));

            var datosActualizados = value.ToEntity();
            existente.Modelo = datosActualizados.Modelo;
            existente.Marca = datosActualizados.Marca;
            existente.Motor = datosActualizados.Motor;
            existente.Cilindrada = datosActualizados.Cilindrada;
            existente.DniDueño = datosActualizados.DniDueño;
            _context.SaveChanges(); 
            
            return Result.Success<Vehiculo, DomainError>(existente.ToModel())
                .Tap(l => _logger.Information("Se ha borrado (físico) el vehiculo con el ID {Id}", key));
        }
        catch (Exception ex)
        {
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al actualizar"));
        }
    }

    public bool ExistId(int key)
    {
        return _context.Vehiculo.Any(v => v.Id == key);
    }

    public bool ExistMatricula(string key)
    {
        return _context.Vehiculo.Any(v => v.Matricula == key);
    }

    public void DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        _context.Vehiculo.RemoveRange(_context.Vehiculo);
        _context.SaveChanges();
    }
    
    /// <summary>
    /// Busca los vehiculos asocidos a un dni en especifico y verifica si hay menos que el máximo configurado
    /// </summary>
    /// <param name="key">El dni</param>
    /// <returns>True si hay menos que el máximo configurado</returns>
    private bool ContarVehiculos(string key)
    {
        var vehiculos = GetAll();
        if (vehiculos.Count(v => v.DniDueño == key) >= _limiteVehciulos) 
        {
            _logger.Warning("Límite alcanzado: El cliente con DNI {Dni} ya tiene el máximo de vehículos permitido", key);
            return false;
        }
        return true;
    }
}