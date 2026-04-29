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

public class AdoRepository : IRepositorioVehiculos
{
    private readonly ILogger _logger = Log.ForContext<AdoRepository>();
    private readonly Func<SqliteConnection> _connectionFactory;
    private readonly int _limiteVehciulos = 3;
    
    private SqliteConnection CreateConnection() => _connectionFactory();

    // Backwards-compatible constructor: recibe cadena de conexión
    public AdoRepository(string connection) : this(() => new SqliteConnection(connection))
    {
    }

    // Constructor inyectable para tests
    public AdoRepository(Func<SqliteConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
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
        // Normalizamos el nombre de la columna: DniDueno (sin acento/ñ)
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Vehiculo(
                Id INTEGER PRIMARY KEY,
                Matricula VARCHAR(9) NOT NULL UNIQUE,
                Modelo  VARCHAR(100) NOT NULL,
                Marca VARCHAR(100) NOT NULL,
                Motor INTEGER NOT NULL,
                Cilindrada REAL CHECK (Cilindrada > 0) NOT NULL,
                DniDueno VARCHAR(9) NOT NULL,
                IsDeleted INTEGER DEFAULT 0
                )");
        _logger.Debug("Se ha creado la tabla, creo");
    }
    
    public IEnumerable<Vehiculo> GetAll()
    {
        var vehiculos = new List<Vehiculo>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Vehiculo";
        using var reader = command.ExecuteReader();
        while(reader.Read()) vehiculos.Add(MapVehiculo(reader).ToModel());
        return vehiculos;
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
            
            using var connection = CreateConnection();
            connection.Open();

            // Primero insert
            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO Vehiculo(Matricula, Modelo, Marca, Motor, Cilindrada, DniDueno)
                        VALUES (@Matricula, @Modelo, @Marca, @Motor, @Cilindrada, @DniDueno)";
            insertCmd.Parameters.AddWithValue("@Matricula", entity.Matricula);
            insertCmd.Parameters.AddWithValue("@Modelo", entity.Modelo);
            insertCmd.Parameters.AddWithValue("@Marca", entity.Marca);
            insertCmd.Parameters.AddWithValue("@Motor", entity.Motor);
            insertCmd.Parameters.AddWithValue("@Cilindrada", entity.Cilindrada);
            insertCmd.Parameters.AddWithValue("@DniDueno", entity.DniDueño);
            insertCmd.ExecuteNonQuery();

            // Luego recuperamos la fila insertada
            using var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Vehiculo WHERE rowid = last_insert_rowid()";
            using var reader = selectCmd.ExecuteReader();
            var vehiculo = reader.Read() ? MapVehiculo(reader) : null;
            
            return vehiculo == null
                ? Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundId(entity.Id))
                    .TapError(l => _logger.Error("No se ha encontrado el vehiculo con el Id {Id}", entity.Id))
                : Result.Success<Vehiculo, DomainError>(vehiculo.ToModel())
                    .Tap((l => _logger.Information("Se ha creado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al agregar");
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al agregar"));
        }
    }

    public Result<Vehiculo, DomainError> Borrar(int key, bool isLogical = true)
    {
        try
        {
            var encontrado = BuscarId(key);
            if (encontrado.IsFailure)
                return encontrado.TapError( l => _logger.Error("No se ha encontrado el vehiculo con el ID {Id} para poder borrarlo", key));
            
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            if (isLogical)
            {
                command.CommandText = "UPDATE Vehiculo SET IsDeleted = @IsDeleted WHERE Id = @Id";
                command.Parameters.AddWithValue("@IsDeleted", 1);
                command.Parameters.AddWithValue("@Id", key);
                command.ExecuteNonQuery();
                return BuscarId(key)
                    .Tap((l => _logger.Information("Se ha borrado (lógico) el vehiculo con el ID {Id}", l.Id)));
                
            }
            command.CommandText = "DELETE FROM Vehiculo WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", key);
            return command.ExecuteNonQuery() > 0 ? 
                encontrado.Tap((l => _logger.Information("Se ha borrado (físico) el vehiculo con el ID {Id}", l.Id)))
                : encontrado.TapError( l => _logger.Error("No se ha encontrado el vehiculo con el ID {Id} para poder borrarlo (físico)", key));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al borrar");
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al borrar"));
        }
    }

    public Result<Vehiculo, DomainError> BuscarId(int key)
    {
        try
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Vehiculo WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", key);
            using var reader = command.ExecuteReader();
            var vehiculo = reader.Read() ? MapVehiculo(reader) : null;
            
            return vehiculo == null
                ? Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundId(key))
                    .TapError(l => _logger.Fatal("No se ha encontrado el vehiculo con el Id {Id}", key))
                : Result.Success<Vehiculo, DomainError>(vehiculo.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al buscar por id");
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al buscar por id"));
        }
    }
    
    public Result<Vehiculo, DomainError> BuscarMatricula(string key)
    {
        try
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Vehiculo WHERE Matricula = @Matricula";
            command.Parameters.AddWithValue("@Matricula", key);
            using var reader = command.ExecuteReader();
            var vehiculo = reader.Read() ? MapVehiculo(reader) : null;
            
            return vehiculo == null
                ? Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundMatricula(key))
                    .TapError(l => _logger.Fatal("No se ha encontrado el vehiculo con la matricula {Matricula}", key))
                : Result.Success<Vehiculo, DomainError>(vehiculo.ToModel())
                    .Tap((l => _logger.Information("Se ha encontrado el vehiculo con la matricula {Matricula}", l.Matricula)));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al buscar por matrícula");
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al buscar por matrícula"));
        }
    }

    public Result<Vehiculo, DomainError> Actualizar(int key, Vehiculo value)
    {
        try
        {
            var entity = value.ToEntity();
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE Vehiculo SET
                                    Matricula = @Matricula, Modelo = @Modelo, Marca = @Marca, Motor = @Motor,
                                    Cilindrada = @Cilindrada, DniDueno = @DniDueno, IsDeleted = @IsDeleted
                                    WHERE Id = @Id";
            
            command.Parameters.AddWithValue("@Matricula", entity.Matricula);
            command.Parameters.AddWithValue("@Modelo", entity.Modelo);
            command.Parameters.AddWithValue("@Marca", entity.Marca);
            command.Parameters.AddWithValue("@Motor", entity.Motor);
            command.Parameters.AddWithValue("@Cilindrada", entity.Cilindrada);
            command.Parameters.AddWithValue("@DniDueno", entity.DniDueño);
            command.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);
            command.Parameters.AddWithValue("@Id", key);
            command.ExecuteNonQuery();
            
            return BuscarId(key).Tap((l => _logger.Information("Se ha actualizado el vehiculo con el ID {Id}", l.Id)));
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Error en la base de datos al actualizar");
            return Result.Failure<Vehiculo, DomainError>(new DataBaseError(ex.ToString()))
                .TapError(l => _logger.Fatal("Error en la base de datos al actualizar"));
        }
    }

    public bool ExistId(int key)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Vehiculo WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", key);
        var val = command.ExecuteScalar();
        var numero = val == null ? 0 : Convert.ToInt32(val);
        return numero == 1;
    }
    
    public bool ExistMatricula(string key)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Vehiculo WHERE Matricula = @Matricula";
        command.Parameters.AddWithValue("@Matricula", key);
        var val = command.ExecuteScalar();
        var numero = val == null ? 0 : Convert.ToInt32(val);
        return numero == 1;
    }

    public void DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM Vehiculo";
        command.ExecuteNonQuery();
    }
    private VehiculoEntity MapVehiculo(SqliteDataReader reader)
    {
        return new VehiculoEntity(
            reader.GetInt32(reader.GetOrdinal("Id")),
            reader.GetString(reader.GetOrdinal("Matricula")),
            reader.GetString(reader.GetOrdinal("Modelo")),
            reader.GetString(reader.GetOrdinal("Marca")),
            reader.GetDouble(reader.GetOrdinal("Cilindrada")),
            reader.GetInt32(reader.GetOrdinal("Motor")),
            reader.GetString(reader.GetOrdinal("DniDueno")),
            reader.GetInt32(reader.GetOrdinal("IsDeleted"))
        );
    }
    
    /// <summary>
    /// Busca los vehiculos asocidos a un dni en especifico y verifica si hay menos que el máximo configurado
    /// </summary>
    /// <param name="key">El dni</param>
    /// <returns>True si hay menos que el máximo configurado</returns>
    private bool ContarVehiculos(string key)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Vehiculo WHERE DniDueno = @Dni";
        cmd.Parameters.AddWithValue("@Dni", key);
        var val = cmd.ExecuteScalar();
        var count = val == null ? 0 : Convert.ToInt32(val);
        if (count >= _limiteVehciulos)
        {
            _logger.Warning("Límite alcanzado: El cliente con DNI {Dni} ya tiene el máximo de vehículos permitido", key);
            return false;
        }
        return true;
    }
}