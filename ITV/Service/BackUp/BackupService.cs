using System.IO;
using System.IO.Compression;
using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Error.BuckUp;
using ITV.Error.Common;
using ITV.Models;
using ITV.Storage.Common;
using Serilog;

namespace ITV.Service;

public class BackupService : IBuckUpServiceVehiculos
{
    private readonly ILogger _logger = Log.ForContext<BackupService>();
    private static readonly string _fecha = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    private readonly string _finalFileName;
    private readonly string _finalFolderName;
    private readonly string _tempName = "tempBackup";
    private readonly IStorage<Vehiculo> _storage;

    public BackupService(IStorage<Vehiculo> storage, string file, string directory)
    {
        _finalFileName = _fecha + file + "." + Configuracion.StorageType.ToLower();
        _finalFolderName = _fecha + directory;
        _storage = storage;
    }
    

    public Result<string, DomainError> Guardar(IEnumerable<Vehiculo> lista)
    {
        try
        {
            Directory.CreateDirectory(_finalFolderName);
            
            // Directorio temporal
            string tempDirectory = Directory.CreateDirectory(_tempName).FullName;
            
            _logger.Information("Exportando datos temporales para compresión");
            _storage.Salvar(lista);
           
            // Ruta del zip
            var zipPath = Path.Combine(_finalFolderName, _finalFileName);
            
            _logger.Information("Comprimiendo archivos en: {Path}", zipPath);
            ZipFile.CreateFromDirectory(tempDirectory, zipPath);

            // Limpieza
            Directory.Delete(tempDirectory, true);
            
            return Result.Success<string, DomainError>(zipPath)
                .Tap(l => _logger.Information("Se han guardado los datos almacenados en un archivo .zip en la ruta {Ruta}", zipPath));
        }
        catch (Exception ex)
        {
            return Result.Failure<string, DomainError>(new BackUpError(ex.Message))
                .TapError(l => _logger.Error(ex, "Error crítico durante la creación del backup"));
        }
    }

    public Result<IEnumerable<Vehiculo>, DomainError> Resturar(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new BackUpError.DirectoryNotFound(path))
                    .TapError(l => _logger.Error("El archivo no existe en la ruta especificada: {Path}", path));
            }

            // Directorio temporal
            string tempDirectory = Directory.CreateDirectory(_tempName).FullName;
            
            _logger.Information("Extrayendo contenido del backup...");
            ZipFile.ExtractToDirectory(path, tempDirectory);

            var vehiculos = _storage.Cargar();
            
            // Limpieza
            Directory.Delete(tempDirectory, true);
            
            return Result.Success<IEnumerable<Vehiculo>, DomainError>(vehiculos.Value)
                .Tap(l => _logger.Information("Vehiculos cargados exitosamente desde la ruta: {Path}", path));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new BackUpError(ex.Message))
                .TapError(l => _logger.Error(ex, "Error crítico al intentar restaurar el backup desde {Path}", path));
        }
    }

    public IEnumerable<string> Listar() 
    {
        _logger.Information("Consultando lista de copias de seguridad disponibles");
        
        if (!Directory.Exists(_finalFolderName)) 
        {
            _logger.Warning("El directorio de backups {Directorio} no existe", _finalFolderName);
            return Enumerable.Empty<string>();
        }

        var archivos = Directory.GetFiles(_finalFolderName, "*.zip")
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();

        _logger.Information("Se han encontrado {Total} archivos de backup", archivos.Count);
        return archivos;
    }
}