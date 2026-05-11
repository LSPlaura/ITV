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
    

    public IEnumerable<Cita> GetAll()
    {
        try
        {
            var query = _context.Vehiculo.AsQueryable();
            return query.ToModel();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al obtener los vehiculos");
            return Enumerable.Empty<Cita>();
        }
    }

    public Result<Cita, DomainError> Agregar(Cita value)
    {
        try
        {
            var entity = value.ToEntity();
            
            _context.Vehiculo.Add(entity);
            _context.SaveChanges();
            return Result.Success<CitaEntity, DomainError>(entity).Map(v => v.ToModel())
                .Tap((l => _logger.Information("Se ha añadido el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al agregar"));
        }
    }

    public Result<Cita, DomainError> Borrar(int key, bool isLogical = true)
    {
        try
        {
            var entity = _context.Vehiculo.Find(key);
            if (entity == null)
                return Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(key))
                    .TapError((l => _logger.Error("No se ha encontrado el vehiculo con el ID al intentar borrarlo{Id}", key)));

            if (isLogical)
            {
                entity.IsDeleted = 1;
                entity.UpdatedAt = DateTime.Now.ToString("s");
                _context.SaveChanges();
                return Result.Success<Cita, DomainError>(entity.ToModel())
                    .Tap(l => _logger.Information("Se ha borrado (lógico) el vehiculo con el ID {Id}", key));
            }

            _context.Vehiculo.Remove(entity);
            _context.SaveChanges();
            return Result.Success<Cita, DomainError>(entity.ToModel())
                .Tap(l => _logger.Information("Se ha borrado (físico) el vehiculo con el ID {Id}", key));
        }
        catch (Exception ex)
        {
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al borrar"));
        }
    }

    public Result<Cita, DomainError> BuscarId(int key)
    {
        try
        {
            var entity = _context.Vehiculo.Find(key);
            
            return entity == null
                ? Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(key))
                    .TapError((l => _logger.Error("No se ha encontrado el vehiculo con el ID {Id}", key)))
                : Result.Success<CitaEntity, DomainError>(entity).Map(v => v.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al buscar por id"));
        }
    }

    public Result<Cita, DomainError> BuscarMatricula(string key)
    {
        try
        {
            var entity = _context.Vehiculo.FirstOrDefault(e =>e.Matricula== key);

            return entity == null
                ? Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundMatricula(key))
                    .TapError((l =>
                        _logger.Error("No se ha encontrado el vehiculo con la matricula {Matricula}", key)))
                : Result.Success<CitaEntity, DomainError>(entity).Map(v => v.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con la matricula {Matricula}",
                        l.Matricula)));
        }
        catch (Exception ex)
        {
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al buscar por matrícula"));
        }
    }

    public Result<Cita, DomainError> Actualizar(int key, Cita value)
    {
        try
        {
            var existente = _context.Vehiculo.Find(key);
            if (existente == null)
                return Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(key))
                    .TapError((l => _logger.Error("No se ha encontrado el vehiculo con el ID al intentar borrarlo{Id}", key)));

            value = value with { UpdatedAt = DateTime.Now };
            var datosActualizados = value.ToEntity();
            existente.Matricula = datosActualizados.Matricula;
            existente.Modelo = datosActualizados.Modelo;
            existente.Marca = datosActualizados.Marca;
            existente.Motor = datosActualizados.Motor;
            existente.Cilindrada = datosActualizados.Cilindrada;
            existente.DniDueño = datosActualizados.DniDueño;
            existente.UpdatedAt = datosActualizados.UpdatedAt;
            _context.SaveChanges(); 
            
            return Result.Success<Cita, DomainError>(existente.ToModel())
                .Tap(l => _logger.Information("Se ha borrado (físico) el vehiculo con el ID {Id}", key));
        }
        catch (Exception ex)
        {
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.Message))
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
}