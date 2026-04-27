using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Dto;
using ITV.Error.Common;
using ITV.Error.Vehiculos;
using ITV.Mappers;
using ITV.Models;
using ITV.Repository.Common;
using Serilog;

namespace ITV.Repository.JSON;

public class RepositorioJson : IRepositorioVehiculos
{
    private readonly ILogger _logger = Log.ForContext<RepositorioJson>();
    private readonly Dictionary<int, string> _ids = new Dictionary<int, string>();
    private readonly Dictionary<string, Vehiculo> _vehiculos = new Dictionary<string, Vehiculo>();
    private int _counter;
    private readonly int _limiteVehciulos = 3;
    private readonly string _filePath;
    
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public RepositorioJson(string directory, string file = "repositorio.json")
    {
        _filePath = Path.Combine(directory, file);
        EnsureDirectory();
        Load();
    }
    
    private void EnsureDirectory() {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
            _logger.Information("Creando directorio para almacenar el archivo del repositorio en la {Ruta}]", Configuracion.RepositoryFolder);
            Directory.CreateDirectory(dir);
        }
    }

    private void Load()
    {
        if (!File.Exists(_filePath)) return;
        try
        {
            _logger.Information("Cargando archivo con los datos del repositorio desde {Ruta}", _filePath);
            using var file = File.OpenRead(_filePath);
            var vehiculos = JsonSerializer.Deserialize<List<VehiculoDto>>(file, _options)
                ?.Select(v => v.ToModel());
            if (vehiculos != null)
            {
                foreach (var v in vehiculos)
                {
                    _ids.Add(v.Id, v.Matricula);
                    _vehiculos.Add(v.Matricula, v);
                }
            }
            //en caso de que los vehiculos exportados no sigan un orden especifico de ids, de esta manera que el contador
            //por el siguiente número al ID más alto para que no haya errores de duplicación de IDs
            var idMayor = _ids.Keys.MaxBy(k => k);
            _counter = _vehiculos.Values.First(v => v.Id == idMayor).Id;
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message, "Error crítico al cargar el archivo");
        }
    }

    private void Save()
    {
        try
        {
            var dtos = _vehiculos.Values.Select(v => v.ToDto());
            using var file = File.Open(_filePath, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(file, dtos, _options);
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message, "Error crítico al guardar el archivo");
        }
    }
    
    public IEnumerable<Vehiculo> GetAll()
    {
        _logger.Information("Obteniendo todos los vehículos");
        return _vehiculos.Values;
    }

    public Result<Vehiculo, DomainError> Agregar(Vehiculo vehiculo)
    {
        vehiculo = vehiculo with
        {
            Id = ++_counter
        };
        
        if (ExistId(vehiculo.Id)) 
            return Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoAlredyExist.IdAlreadyExists(vehiculo.Id))
                .TapError(v => _logger.Error("Fallo al agregar: El ID {Id} ya existe en el sistema", vehiculo.Id));
            
        if (ExistMatricula(vehiculo.Matricula))
            return Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoAlredyExist.MatriculaAlreadyExists(vehiculo.Matricula))
                .TapError(v => _logger.Error("Fallo al agregar: La matricula {Matricula} ya está registrada", vehiculo.Matricula));

        if (!ContarVehiculos(vehiculo.DniDueño))
            return Result.Failure<Vehiculo, DomainError>(new VehiculoError.OwnerWithThreeOrMoreVehiculos(vehiculo.DniDueño))
                .TapError(v => _logger.Error("Límite alcanzado: El dueño con DNI {Dni} no puede tener más vehículos", vehiculo.DniDueño));
        
        _ids.Add(vehiculo.Id, vehiculo.Matricula);
        _vehiculos.Add(vehiculo.Matricula, vehiculo);
        Save();
        return Result.Success<Vehiculo, DomainError>(vehiculo)
            .Tap(v => _logger.Information("El vehiculo con la matricula {Matricula} ha sido agregado", vehiculo.Matricula));
    }

    public Result<Vehiculo, DomainError> Borrar(int key, bool isLogical = true)
    {
        if ( _ids.TryGetValue(key, out var matricula) && _vehiculos.TryGetValue(matricula, out var vehiculo ))
        {
            if (isLogical)
            {
                vehiculo = vehiculo with { IsDeleted = true };
                Save();
                return Result.Success<Vehiculo, DomainError>(vehiculo)
                    .Tap(v => _logger.Information("El vehiculo con la matricula {Matricula} ha sido borrado (Lógico)", vehiculo.Matricula));
            }
        
            if (_ids.Remove(vehiculo.Id) && _vehiculos.Remove(matricula)) 
            {
                Save();
                return Result.Success<Vehiculo, DomainError>(vehiculo)
                    .Tap(v => _logger.Information("El vehiculo con la matricula {Matricula} ha sido borrado (Físico)", vehiculo.Matricula));
            }
        }
        return Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundId(key))
            .TapError(v => _logger.Information("El vehiculo con el ID {Id} no ha podido ser borrado", key));
    }

    public Result<Vehiculo, DomainError> BuscarId(int key)
    {
        return _ids.TryGetValue(key, out var matricula) && _vehiculos.TryGetValue(matricula, out var vehiculo)
            ? Result.Success<Vehiculo, DomainError>(vehiculo)
                .Tap(v => _logger.Information("El vehiculo con el ID {Id} encontrado", vehiculo.Id))
            : Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundId(key))
                .TapError(v => _logger.Information("El vehiculo con el ID {Id} no ha podido ser encontrado", key));
    }
    
    public Result<Vehiculo, DomainError> BuscarMatricula(string key)
    {
        var vehiculo = _vehiculos.GetValueOrDefault(key);
        return vehiculo != null ? Result.Success<Vehiculo, DomainError>(vehiculo)
                .Tap(v => _logger.Information("El vehiculo con la matricula {Matricula} encontrado", vehiculo.Matricula))
            : Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundMatricula(key))
                .TapError(v => _logger.Information("El vehiculo con la matricula {Matricula} no ha podido ser encontrado", key));
    }
    
    public Result<Vehiculo, DomainError> Actualizar(int key, Vehiculo value)
    {
        var vehiculo = _ids.TryGetValue(key, out var matricula) && _vehiculos.TryGetValue(matricula, out var encontrado)
            ? encontrado : null;

        if (vehiculo == null)
            return Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundId(key))
                .TapError(v => _logger.Information("El vehiculo con el ID {Id} no ha podido ser encontrado", key));
        
        var actualizado = value with
        {
            Id = vehiculo.Id,
            Matricula = vehiculo.Matricula
        };
        
        if (_ids.Remove(vehiculo.Id) && _vehiculos.Remove(vehiculo.Matricula))
        {
            _ids.Add(actualizado.Id, actualizado.Matricula);
            _vehiculos.Add(actualizado.Matricula, actualizado);
            Save();
            return Result.Success<Vehiculo, DomainError>(vehiculo)
                .Tap(v => _logger.Information("El vehiculo con la matrícula {Matricula} actualizado", vehiculo.Matricula));
        }
        return Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundMatricula(vehiculo.Matricula))
            .TapError(v => _logger.Information("El vehiculo con el ID {Id} no ha podido ser encontrado", key));
    }
    
    public bool ExistId(int key)
    {
        if (_ids.ContainsKey(key)) return true;
        return false;
    }

    public void DeleteAll()
    {
        _ids.Clear();
        _vehiculos.Clear();
        Save(); 
    }

    public bool ExistMatricula(string key)
    {
        if (_vehiculos.ContainsKey(key)) return true;
        return false;
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