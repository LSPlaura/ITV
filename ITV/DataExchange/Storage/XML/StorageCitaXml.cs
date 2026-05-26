using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Dto;
using ITV.Error.Common;
using ITV.Error.Storage;
using ITV.Mappers;
using ITV.Models;
using ITV.Storage.Common;
using Serilog;

namespace ITV.Storage.XML;

public class StorageCitaXml : IStorageCita
{
    private readonly ILogger _logger = Log.ForContext<StorageCitaXml>();
    private readonly XmlSerializerNamespaces _xmlSerializerNamespaces = new();
    private readonly XmlWriterSettings _xmlWriterSettings = new() {
        Indent = true,
        Encoding = Encoding.UTF8
    };
    private string _filePath;
    private string _directoryPath;
    private string _fullPath;
    
    /// <summary>
    /// Constructor para implementar la creacion de la carpeta, si fuese necesario
    /// </summary>
    public StorageCitaXml(string filePath, string directoryPath)
    {
        _filePath = filePath;
        _directoryPath = directoryPath;
        Init();
        _fullPath = Path.Combine(directoryPath, filePath + ".xml");
    }

    public Result<bool, DomainError> Salvar(IEnumerable<Cita> items)
    {
        _logger.Information("Iniciando exportación de datos a XML en: {Path}", _fullPath);
        
        try
        {
            var dtos = items.Select(p => p.ToDto()).ToList();
            var serializer = new XmlSerializer(typeof(List<CitaDto>));

            using var streamWriter = new StreamWriter(_fullPath, false, Encoding.UTF8);
            using var xmlWriter = XmlWriter.Create(streamWriter, _xmlWriterSettings);
            serializer.Serialize(xmlWriter, dtos, _xmlSerializerNamespaces);
            
            return Result.Success<bool, DomainError>(true).Tap(l =>  _logger.Information("Exportación XML finalizada con éxito. Registros procesados: {Count}", dtos.Count));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>(new StorageError(ex.Message))
                .TapError(err => _logger.Error(ex, "Error crítico al intentar salvar el archivo XML en {Path}", _fullPath));
        }
    }

    public Result<IEnumerable<Cita>, DomainError> Cargar()
    {
        _logger.Information("Iniciando carga de datos desde XML: {Path}", _fullPath);

        if (!File.Exists(_fullPath))
            return Result.Failure<IEnumerable<Cita>, DomainError>(new StorageError(($"El archivo {_fullPath} no existe")))
                .TapError(l => _logger.Error("No se pudo cargar el XML: El archivo no existe en la ruta {Path}", _fullPath));

        try
        {
            var serializer = new XmlSerializer(typeof(List<CitaDto>));
            using var stream = File.OpenRead(_fullPath);
            var dtos = serializer.Deserialize(stream) as List<CitaDto>;
            
            if (dtos == null) 
                return Result.Failure<IEnumerable<Cita>, DomainError>(new StorageError("No se pudieron deserializar los DTOs."))
                    .TapError(l => _logger.Error("Deserialización fallida: El archivo en {Path} devolvió una lista nula o incompatible.", _fullPath));

            var resultado = dtos.Select(dto => dto.ToModel()).ToList();
            
            return Result.Success<IEnumerable<Cita>, DomainError>(resultado).Tap(l => _logger.Information("Carga XML completada. Se han recuperado {Count} registros", resultado.Count));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Cita>, DomainError>(new StorageError(ex.Message))
                .TapError(l =>_logger.Error(ex, "Error crítico durante el procesamiento del archivo XML en {Path}", _fullPath));
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