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
using Microsoft.Data.Sqlite;
using Serilog;

namespace ITV.Repository.Ado;

public class AdoRepository: IRepositorioCita
{
    private readonly ILogger _logger = Log.ForContext<AdoRepository>();
    private SqliteConnection _connection;
    
    public AdoRepository(SqliteConnection connection)
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
        _logger.Debug("Creando la tabla Cita");
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

        _logger.Debug("Tabla Cita verificada/creada correctamente");
    }
    public IEnumerable<Cita> GetAll()
    {
        var vehiculos = new List<Cita>();
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT Id, Matricula, Marca, Modelo, Cilindrada, Motor, DniDueno AS DniDueño, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted FROM Cita";
        using var reader = command.ExecuteReader();
        while(reader.Read()) vehiculos.Add(MapVehiculo(reader).ToModel());
        return vehiculos;
    }

    public Result<Cita, DomainError> Agregar(Cita value)
    {
        try
        {
            var entity = value.ToEntity();
            // Primero insert
            using var insertCmd = _connection.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO Cita(Matricula, Modelo, Marca, Motor, Cilindrada, DniDueno,FechaMatriculacion,FechaInspeccion,CreatedAt,UpdatedAt)
                        VALUES (@Matricula, @Modelo, @Marca, @Motor, @Cilindrada, @DniDueño, @FechaMatriculacion, @FechaInspeccion, @CreatedAt, @UpdatedAt)";
            insertCmd.Parameters.AddWithValue("@Matricula", entity.Matricula);
            insertCmd.Parameters.AddWithValue("@Modelo", entity.Modelo);
            insertCmd.Parameters.AddWithValue("@Marca", entity.Marca);
            insertCmd.Parameters.AddWithValue("@Motor", entity.Motor);
            insertCmd.Parameters.AddWithValue("@Cilindrada", entity.Cilindrada);
            insertCmd.Parameters.AddWithValue("@DniDueño", entity.DniDueño);
            insertCmd.Parameters.AddWithValue("@FechaMatriculacion", entity.FechaMatriculacion);
            insertCmd.Parameters.AddWithValue("@FechaInspeccion", entity.FechaInspeccion);
            insertCmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
            insertCmd.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt);
            insertCmd.ExecuteNonQuery();

            // Luego recuperamos la fila insertada
            using var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = "SELECT Id, Matricula, Marca, Modelo, Cilindrada, Motor, DniDueno AS DniDueño, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted FROM Cita WHERE rowid = last_insert_rowid()";
            using var reader = selectCmd.ExecuteReader();
            var vehiculo = reader.Read() ? MapVehiculo(reader) : null;
            
            return vehiculo == null
                ? Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(entity.Id))
                    .TapError(l => _logger.Error("No se ha encontrado el vehiculo con el Id {Id}", entity.Id))
                : Result.Success<Cita, DomainError>(vehiculo.ToModel())
                    .Tap((l => _logger.Information("Se ha creado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al agregar");
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.ToString()))
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
            
            using var command = _connection.CreateCommand();

            if (isLogical)
            {
                command.CommandText = "UPDATE Cita SET IsDeleted = @IsDeleted, UpdatedAt = @UpdatedAt WHERE Id = @Id";
                command.Parameters.AddWithValue("@IsDeleted", 1);
                command.Parameters.AddWithValue("@Id", key);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("s"));
                command.ExecuteNonQuery();
                return BuscarId(key)
                    .Tap((l => _logger.Information("Se ha borrado (lógico) el vehiculo con el ID {Id}", l.Id)));
                
            }
            command.CommandText = "DELETE FROM Cita WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", key);
            return command.ExecuteNonQuery() > 0 ? 
                encontrado.Tap((l => _logger.Information("Se ha borrado (físico) el vehiculo con el ID {Id}", l.Id)))
                : encontrado.TapError( l => _logger.Error("No se ha encontrado el vehiculo con el ID {Id} para poder borrarlo (físico)", key));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al borrar");
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al borrar"));
        }
    }

    public Result<Cita, DomainError> BuscarId(int key)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"SELECT Id, Matricula, Marca, Modelo, Cilindrada, Motor, DniDueno AS DniDueño, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted FROM Cita WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", key);
            using var reader = command.ExecuteReader();
            var vehiculo = reader.Read() ? MapVehiculo(reader) : null;
            
            return vehiculo == null
                ? Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(key))
                    .TapError(l => _logger.Fatal("No se ha encontrado el vehiculo con el Id {Id}", key))
                : Result.Success<Cita, DomainError>(vehiculo.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al buscar por id");
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al buscar por id"));
        }
    }
    
    public Result<Cita, DomainError> BuscarMatricula(string key)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"SELECT Id, Matricula, Marca, Modelo, Cilindrada, Motor, DniDueno AS DniDueño, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted 
                                    FROM Cita WHERE Matricula = @Matricula";
            command.Parameters.AddWithValue("@Matricula", key);
            using var reader = command.ExecuteReader();
            var vehiculo = reader.Read() ? MapVehiculo(reader) : null;
            
            return vehiculo == null
                ? Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundMatricula(key))
                    .TapError(l => _logger.Fatal("No se ha encontrado el vehiculo con la matricula {Matricula}", key))
                : Result.Success<Cita, DomainError>(vehiculo.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con la matricula {Matricula}", l.Matricula)));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al buscar por matrícula");
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al buscar por matrícula"));
        }
    }

    public Result<Cita, DomainError> Actualizar(int key, Cita value)
    {
        try
        {
            value = value with
            {
                UpdatedAt = DateTime.Now
            };
            var entity = value.ToEntity();
           
            using var command = _connection.CreateCommand();
            command.CommandText = @"UPDATE Cita SET
                                    FechaMatriculacion = @FechaMatriculacion, FechaInspeccion = @FechaInspeccion, Matricula = @Matricula, Modelo = @Modelo, Marca = @Marca, Motor = @Motor,
                                    Cilindrada = @Cilindrada, DniDueno = @DniDueno, UpdatedAt = @UpdatedAt
                                    WHERE Id = @Id";
            
            command.Parameters.AddWithValue("@FechaMatriculacion", entity.FechaMatriculacion);
            command.Parameters.AddWithValue("@FechaInspeccion", entity.FechaInspeccion);
            command.Parameters.AddWithValue("@Matricula", entity.Matricula);
            command.Parameters.AddWithValue("@Modelo", entity.Modelo);
            command.Parameters.AddWithValue("@Marca", entity.Marca);
            command.Parameters.AddWithValue("@Motor", entity.Motor);
            command.Parameters.AddWithValue("@Cilindrada", entity.Cilindrada);
            command.Parameters.AddWithValue("@DniDueno", entity.DniDueño);
            command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt);
            command.Parameters.AddWithValue("@Id", key);
            command.ExecuteNonQuery();
            
            return BuscarId(key).Tap((l => _logger.Information("Se ha actualizado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al actualizar");
            return Result.Failure<Cita, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al actualizar"));
        }
    }

    public bool ExistId(int key)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Cita WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", key);
        var val = command.ExecuteScalar();
        var numero = val == null ? 0 : Convert.ToInt32(val);
        return numero > 0;
    }
    
    public bool ExistMatricula(string key)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Cita WHERE Matricula = @Matricula";
        command.Parameters.AddWithValue("@Matricula", key);
        var val = command.ExecuteScalar();
        var numero = val == null ? 0 : Convert.ToInt32(val);
        return numero > 0;
    }

    public void DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        using var command = _connection.CreateCommand();
        command.CommandText = @"DELETE FROM Cita";
        command.ExecuteNonQuery();
    }
    private CitaEntity MapVehiculo(SqliteDataReader reader)
    {
        return new CitaEntity(
            reader.GetInt32(reader.GetOrdinal("Id")),
            reader.GetString(reader.GetOrdinal("FechaMatriculacion")),
            reader.GetString(reader.GetOrdinal("FechaInspeccion")),
            reader.GetString(reader.GetOrdinal("Matricula")),
            reader.GetString(reader.GetOrdinal("Modelo")),
            reader.GetString(reader.GetOrdinal("Marca")),
            reader.GetDouble(reader.GetOrdinal("Cilindrada")),
            reader.GetInt32(reader.GetOrdinal("Motor")),
            reader.GetString(reader.GetOrdinal("DniDueño")),
            reader.GetInt32(reader.GetOrdinal("IsDeleted")),
            reader.GetString(reader.GetOrdinal("CreatedAt")),
            reader.GetString(reader.GetOrdinal("UpdatedAt"))
        );
    }
}