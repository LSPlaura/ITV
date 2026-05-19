using System.Data;
using System.IO;
using CSharpFunctionalExtensions;
using Dapper;
using ITV.Config;
using ITV.Entity;
using ITV.Error.Common;
using ITV.Error.DataBase;
using ITV.Error.Vehiculos;
using ITV.Mappers;
using ITV.Models;
using ITV.Repository.Common;
using Serilog;

namespace ITV.Repository.Dapper;

public class DapperRepository : IRepositorioVehiculos
{
    private readonly ILogger _logger = Log.ForContext<DapperRepository>();
    private IDbConnection _connection;
    
    public DapperRepository(IDbConnection connection)
    {
        _connection = connection;
        EnsureDirectory();
        CreateTable();
    }
    
    private void EnsureDirectory()
    {
        if (!Directory.Exists(Configuracion.RepositoryFolder))
        {
            Directory.CreateDirectory(Configuracion.RepositoryFolder);
        }
    }

    private void CreateTable()
    {
        _logger.Debug("Creando la tabla");
        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        _connection.Execute(@"
              CREATE TABLE IF NOT EXISTS Cita(
                Id INTEGER PRIMARY KEY,
                FechaMatriculacion TEXT NOT NULL,
                FechaInspeccion TEXT NOT NULL,
                Matricula TEXT NOT NULL UNIQUE,
                Modelo  TEXT NOT NULL,
                Marca TEXT NOT NULL,
                Motor TEXT NOT NULL,
                Cilindrada REAL CHECK (Cilindrada > 0) NOT NULL,
                DniDueno TEXT NOT NULL,
                IsDeleted INTEGER DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
              );");
        _logger.Debug("Se ha creado la tabla, creo");
    }
    
    public IEnumerable<Cita> GetAll()
    {
        var sql = "SELECT Id, Matricula, Marca, Modelo, Cilindrada, Motor, DniDueno AS DniDueño, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted FROM Cita";
        var entities = _connection.Query<CitaEntity>(sql).ToList();
        return CitaMapper.ToModel(entities);
    }

    public Result<Cita, DomainError> Agregar(Cita value)
    {
        try
        {
            var entity = value.ToEntity();
            
            var sql = @"INSERT INTO Cita(FechaMatriculacion, FechaInspeccion, Matricula, Modelo, Marca, Motor, Cilindrada, DniDueno, CreatedAt, UpdatedAt)
                        VALUES (@FechaMatriculacion, @FechaInspeccion, @Matricula, @Modelo, @Marca, @Motor, @Cilindrada, @DniDueño, @CreatedAt, @UpdatedAt);
                         SELECT last_insert_rowid()";

            entity.Id = _connection.ExecuteScalar<int>(sql, entity);
        
            return BuscarId(entity.Id)
                .Tap((l => _logger.Information("Se ha creado el vehiculo con el ID {Id}", l.Id)));
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
            var encontrado = BuscarId(key);
            if (encontrado.IsFailure)
                return encontrado.TapError( l => _logger.Error("No se ha encontrado el vehiculo con el ID {Id} para poder borrarlo", key));

            if (isLogical)
            {
                var borradoLogico = "UPDATE Cita SET IsDeleted = @IsDeleted, UpdatedAt = @UpdatedAt WHERE Id = @Id";
                _connection.Execute(borradoLogico, new { IsDeleted = 1, Id = key, UpdatedAt = DateTime.Now.ToString("s") });
                return BuscarId(key)
                    .Tap((l => _logger.Information("Se ha borrado (lógico) el vehiculo con el ID {Id}", l.Id)));
            }

            var borradoFisico = "DELETE FROM Cita WHERE Id = @Id";
            _connection.Execute(borradoFisico, new { Id = key });

            return encontrado.Tap((l => _logger.Information("Se ha borrado (físico) el vehiculo con el ID {Id}", l.Id)));
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
            var  sql = "SELECT Id, Matricula, Marca, Modelo, Cilindrada, Motor, DniDueno AS DniDueño, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted FROM Cita WHERE Id = @Id";
            var entity = _connection.QueryFirstOrDefault<CitaEntity>(sql, new { Id = key });
        
            return entity == null ? 
                Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(key))
                    .TapError((l => _logger.Information("No se ha encontrado el vehiculo con el ID {Id}", key)))
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
            var  sql = "SELECT Id, Matricula, Marca, Modelo, Cilindrada, Motor, DniDueno AS DniDueño, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted FROM Cita WHERE Id = @Id";
            var entity = _connection.QueryFirstOrDefault<CitaEntity>(sql, new { Matricula = key });
        
            return entity == null ? 
                Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundMatricula(key))
                    .TapError((l => _logger.Information("No se ha encontrado el vehiculo con la matricula {Matricula}", key)))
                : Result.Success<CitaEntity, DomainError>(entity).Map(v => v.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con la matricula {Matricula}", l.Matricula)));
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
            value = value with { UpdatedAt = DateTime.Now };
            var entity = value.ToEntity();
            
            var sql = @"UPDATE Cita SET 
                    Matricula = @Matricula, Modelo = @Modelo, Marca = @Marca, Motor = @Motor, Cilindrada = @Cilindrada,
                    DniDueno = @DniDueño, FechaMatriculacion = @FechaMatriculacion, FechaInspeccion = @FechaInspeccion,
                    UpdatedAt = @UpdatedAt WHERE Id = @Id";
            _connection.Execute(sql, new { 
                entity.Matricula, 
                entity.Modelo, 
                entity.Marca, 
                entity.Motor, 
                entity.Cilindrada, 
                entity.DniDueño,
                entity.FechaInspeccion,
                entity.FechaMatriculacion,
                entity.UpdatedAt, 
                Id = key
            });
            return BuscarId(key).Tap((l => _logger.Information("Se ha actualizado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.Message))
                .TapError(l => _logger.Fatal("Error en la base de datos al actualizar"));
        }
    }

    public bool ExistId(int key)
    {
        var sql = "SELECT COUNT(1) FROM Cita WHERE Id = @Id";
        return _connection.ExecuteScalar<int>(sql, new { Id = key }) > 0;
    }
    
    public bool ExistMatricula(string key)
    {
        var sql = "SELECT COUNT(1) FROM Cita WHERE Matricula = @Matricula";
        return _connection.ExecuteScalar<int>(sql, new { Matricula = key }) > 0;
    }

    public void DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        _connection.Execute("DELETE FROM Cita");
    }
}