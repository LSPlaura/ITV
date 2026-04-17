using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Dto;
using ITV.Error.Common;
using ITV.Error.Storage;
using ITV.Error.Vehiculos;
using ITV.Mappers;
using ITV.Models;
using ITV.Storage.Common;
using Serilog;

namespace ITV.Storage.JSON;

public class StorageVehiculoJson : IStorageVehiculo
{
    private readonly ILogger _logger = Log.ForContext<StorageVehiculoJson>();
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public Result<bool, DomainError> Salvar(IEnumerable<Vehiculo> items, string path)
    {
        try
        {
            _logger.Information("Iniciando persistencia de datos en formato JSON en: {Path}", path);
            
            using var file = File.Create(path);
            var dtos = items.Select(d => d.ToDto()).ToList();
            JsonSerializer.Serialize(file, dtos, _options);
            
            return Result.Success<bool, DomainError>(true).Tap(l =>  _logger.Information("Persistencia JSON finalizada con éxito. Registros exportados: {Count}", dtos.Count));
        } 
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>(new StorageError(ex.Message))
                .TapError(err => _logger.Error(ex, "Error crítico al intentar salvar el archivo JSON en {Path}", path));
        }
    }

    public Result<IEnumerable<Vehiculo>, DomainError> Cargar(string path)
    {
        if (!File.Exists(path))
        {
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError(($"El archivo {path} no existe")))
                .TapError(l => _logger.Error("No se encontró el archivo JSON para cargar en la ruta: {Path}", path));
        }

        try
        {
            _logger.Information("Iniciando lectura de datos desde JSON: {Path}", path);
            
            using var file = File.OpenRead(path);
            var dtos = JsonSerializer.Deserialize<List<VehiculoDto>>(file, _options);
            if (dtos == null) 
                return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError("No se pudieron deserializar los DTOs."))
                    .TapError(l => _logger.Error("Deserialización fallida: El archivo en {Path} devolvió una lista nula o incompatible.", path));

            var result = dtos.Select(d => d.ToModel()).ToList();
            return Result.Success<IEnumerable<Vehiculo>, DomainError>(result)
                .Tap(l => _logger.Information(
                    "Lectura JSON completada. Se han recuperado {Count} registros desde {Path}", result.Count, path));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError(ex.Message))
                .TapError(l =>_logger.Error(ex, "Error crítico al intentar cargar el archivo JSON desde {Path}", path));
        }
    }
    
    /// <summary>
    /// Crea la carpeta en la que se guardan los ficheros, en el caso de que no existiese
    /// </summary>
    private void Init()
    {
        if (!Directory.Exists(Configuracion.StorageFolder)) 
        {
            _logger.Information("Configurando entorno de datos. Creando directorio: {Path}", Configuracion.StorageFolder);
            Directory.CreateDirectory(Configuracion.StorageFolder);
        }
    }
    
    /// <summary>
    /// Constructor para implementar la creacion de la carpeta, si fuese necesario
    /// </summary>
    public StorageVehiculoJson()
    {
        Init();
    }
}