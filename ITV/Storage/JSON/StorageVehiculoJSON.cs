using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Dto;
using ITV.Error.Common;
using ITV.Error.Storage;
using ITV.Mappers;
using ITV.Models;
using ITV.Storage.Common;
using Serilog;

public class StorageVehiculoJson : IStorageVehiculo
{
   private readonly ILogger _logger = Log.ForContext<StorageVehiculoJson>();
   private readonly JsonSerializerOptions _options = new()
   {
       WriteIndented = true,
       PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
       Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
   };
   private string _filePath;
   private string _directoryPath;
   private string _fullPath;
  
   /// <summary>
   /// Constructor para implementar la creacion de la carpeta, si fuese necesario
   /// </summary>
   public StorageVehiculoJson(string filePath, string directoryPath)
   {
       _filePath = filePath;
       _directoryPath = directoryPath;
       Init();
       _fullPath = Path.Combine(directoryPath, filePath + ".json");
   }

   public Result<bool, DomainError> Salvar(IEnumerable<Vehiculo> items)
   {
       try
       {
           _logger.Information("Iniciando persistencia de datos en formato JSON en: {Path}", _fullPath);
          
           using var file = File.Create(_fullPath);
           var dtos = items.Select(d => d.ToDto()).ToList();
           JsonSerializer.Serialize(file, dtos, _options);
          
           return Result.Success<bool, DomainError>(true).Tap(l =>  _logger.Information("Persistencia JSON finalizada con éxito. Registros exportados: {Count}", dtos.Count));
       }
       catch (Exception ex)
       {
           return Result.Failure<bool, DomainError>(new StorageError(ex.Message))
               .TapError(err => _logger.Error(ex, "Error crítico al intentar salvar el archivo JSON en {Path}", _fullPath));
       }
   }


   public Result<IEnumerable<Vehiculo>, DomainError> Cargar()
   {
       if (!File.Exists(_fullPath))
       {
           return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError(($"El archivo {_fullPath} no existe")))
               .TapError(l => _logger.Error("No se encontró el archivo JSON para cargar en la ruta: {Path}", _fullPath));
       }
       
       try
       {
           _logger.Information("Iniciando lectura de datos desde JSON: {Path}", _fullPath);
          
           using var file = File.OpenRead(_fullPath);
           var dtos = JsonSerializer.Deserialize<List<VehiculoDto>>(file, _options);
           if (dtos == null)
               return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError("No se pudieron deserializar los DTOs."))
                   .TapError(l => _logger.Error("Deserialización fallida: El archivo en {Path} devolvió una lista nula o incompatible.", _fullPath));


           var result = dtos.Select(d => d.ToModel()).ToList();
           return Result.Success<IEnumerable<Vehiculo>, DomainError>(result)
               .Tap(l => _logger.Information(
                   "Lectura JSON completada. Se han recuperado {Count} registros desde {Path}", result.Count, _fullPath));
       }
       catch (Exception ex)
       {
           return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError(ex.Message))
               .TapError(l =>_logger.Error(ex, "Error crítico al intentar cargar el archivo JSON desde {Path}", _fullPath));
       }
   }
  
   /// <summary>
   /// Crea la carpeta en la que se guardan los ficheros, en el caso de que no existiese
   /// </summary>
   private void Init()
   {
       if (string.IsNullOrEmpty(_directoryPath)) _directoryPath = Configuracion.StorageFolder;
       if (!Directory.Exists(_directoryPath))
       {
           _logger.Information("Configurando entorno de datos. Creando directorio: {Path}", _directoryPath);
           Directory.CreateDirectory(_directoryPath);
       }
       if (string.IsNullOrEmpty(_filePath)) _directoryPath = Configuracion.StorageFolder;
   }
}
