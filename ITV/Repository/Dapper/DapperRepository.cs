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
using Microsoft.Data.Sqlite;
using Serilog;

namespace ITV.Repository.Dapper;

public class DapperRepository : IRepositorioVehiculos
{
    private readonly ILogger _logger = Log.ForContext<DapperRepository>();
    private readonly string _connection;
    private readonly int _limiteVehciulos = 3;
    
    private SqliteConnection CreateConnection() => new(_connection);
    
    public DapperRepository(string connection)
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
        using var connection = CreateConnection();
        connection.Open();
        connection.Execute(@"
              CREATE TABLE IF NOT EXISTS Cita(
                Id INTEGER PRIMARY KEY,
                FechaMatriculacion VARCHAR(100) NOT NULL,
                FechaInspeccion VARCHAR(100) NOT NULL
                Matricula VARCHAR(9) NOT NULL UNIQUE,
                Modelo  VARCHAR(100) NOT NULL,
                Marca VARCHAR(100) NOT NULL,
                Motor INTEGER NOT NULL,
                Cilindrada REAL CHECK (Cilindrada > 0) NOT NULL,
                DniDueno VARCHAR(9) NOT NULL,
                IsDeleted INTEGER DEFAULT 0,
                CreatedAt VARCHAR(100) NOT NULL,
                UpdatedAt VARCHAR(100) NOT NULL
              );");
        _logger.Debug("Se ha creado la tabla, creo");
    }
    
    public IEnumerable<Cita> GetAll()
    {
        using var connection = CreateConnection();
        var sql = "SELECT Id, Matricula, Modelo, Marca, Motor, Cilindrada, DniDueno AS DniDueño, IsDeleted FROM Cita";
        var entities = connection.Query<CitaEntity>(sql).ToList();
        return CitaMapper.ToModel(entities);
    }

    public Result<Cita, DomainError> Agregar(Cita value)
    {
        try
        {
            using var connection = CreateConnection();
            var entity = value.ToEntity();
            
            if (ExistMatricula(entity.Matricula))
                return Result.Failure<Cita, DomainError>(new CitaError.CitaAlredyExist.MatriculaAlreadyExists(entity.Matricula))
                    .TapError(v => _logger.Error("Fallo al agregar: La matricula {Matricula} ya está registrada", entity.Matricula));
            
            if (!ContarVehiculos(entity.DniDueño))
                return Result.Failure<Cita, DomainError>(new CitaError.OwnerWithThreeOrMoreCitas(entity.DniDueño))
                    .TapError(v => _logger.Error("Límite alcanzado: El dueño con DNI {Dni} no puede tener más vehículos", entity.DniDueño));
            
            var sql = @"INSERT INTO Cita(FechaMatriculacion, FechaInspeccion, Matricula, Modelo, Marca, Motor, Cilindrada, DniDueno, CreatedAt, UpdatedAt)
                        VALUES (@FechaMatriculacion, @FechaInspeccion, @Matricula, @Modelo, @Marca, @Motor, @Cilindrada, @DniDueño, @CreatedAt, @UpdatedAt);
                         SELECT last_insert_rowid()";

            entity.Id = connection.ExecuteScalar<int>(sql, entity);
        
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
            using var connection = CreateConnection();
            
            var encontrado = BuscarId(key);
            if (encontrado.IsFailure)
                return encontrado.TapError( l => _logger.Error("No se ha encontrado el vehiculo con el ID {Id} para poder borrarlo", key));

            if (isLogical)
            {
                var borradoLogico = "UPDATE VEHICULO SET IsDeleted = @IsDeleted WHERE Id = @Id";
                connection.Execute(borradoLogico, new { IsDeleted = 1, Id = key });
                return BuscarId(key)
                    .Tap((l => _logger.Information("Se ha borrado (lógico) el vehiculo con el ID {Id}", l.Id)));
            }

            var borradoFisico = "DELETE FROM Cita WHERE Id = @Id";
            connection.Execute(borradoFisico, new { Id = key });

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
            using var connection = CreateConnection();
            var sql = "SELECT Id, Matricula, Modelo, Marca, Motor, Cilindrada, DniDueno AS DniDueño, IsDeleted FROM Cita WHERE Id = @Id";
            var entity = connection.QueryFirstOrDefault<CitaEntity>(sql, new { Id = key });
        
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
            using var connection = CreateConnection();
            var sql = "SELECT Id, Matricula, Modelo, Marca, Motor, Cilindrada, DniDueno AS DniDueño, IsDeleted FROM Cita WHERE Matricula = @Matricula";
            var entity = connection.QueryFirstOrDefault<CitaEntity>(sql, new { Matricula = key });
        
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
            using var connection = CreateConnection();
        
            var sqlBuscar = "SELECT * FROM Cita WHERE Id = @Id";
            var encontrado = connection.QueryFirstOrDefault<CitaEntity>(sqlBuscar, new { Id = key });
        
            if (encontrado == null) 
                return Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(key))
                    .TapError( l => _logger.Error("No se ha encontrado el vehiculo con el ID {Id} para poder actualizarllo", key));
            //var desactualizado = encontrado.ToModel();

            //metadatos a agregar
            var entity = value.ToEntity();
            // entity = entity with
            // {
            //
            // };
        
            var sql = @"UPDATE Cita SET 
                    Matricula = @Matricula, Modelo = @Modelo, Marca = @Marca, Motor = @Motor, Cilindrada = @Cilindrada,
                    IsDeleted = @IsDeleted WHERE Id = @Id";
            connection.Execute(sql, entity);
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
        using var connection = CreateConnection();
        var sql = "SELECT COUNT(1) FROM Cita WHERE Id = @Id";
        return connection.ExecuteScalar<int>(sql, new { Id = key }) > 0;
    }
    
    public bool ExistMatricula(string key)
    {
        using var connection = CreateConnection();
        var sql = "SELECT COUNT(1) FROM Cita WHERE Matricula = @Matricula";
        return connection.ExecuteScalar<int>(sql, new { Matricula = key }) > 0;
    }

    public void DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        using var connection = CreateConnection();
        connection.Execute("DELETE FROM Cita");
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