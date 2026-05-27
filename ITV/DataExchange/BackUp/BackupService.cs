using System.IO;
using System.IO.Compression;
using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Error.BackUp;
using ITV.Error.Common;
using ITV.Models;
using ITV.Storage.Common;
using Serilog;

namespace ITV.Service.BackUp;

public class BackupService : IBackUpServiceCitas
{
    private readonly ILogger _logger = Log.ForContext<BackupService>();
    private readonly string _file;
    private readonly string _directory;
    private readonly string _tempName = "tempBackup";
    private readonly IStorage<Cita> _storage;

    public BackupService(IStorage<Cita> storage, string file, string directory)
    {
        _storage = storage;
        _file = file;
        _directory = directory;
    }
    

    public Result<string, DomainError> Guardar(IEnumerable<Cita> lista)
    { 
        string fecha = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var finalFileName = fecha + _file + "." + Configuracion.StorageType.ToLower();
        try
        {
            Directory.CreateDirectory(_directory);
            
            // Directorio temporal
            string tempDirectory = Directory.CreateDirectory(_tempName).FullName;
            
            _logger.Information("Exportando datos temporales para compresión");
            _storage.Salvar(lista);
           
            // Ruta del zip
            var zipPath = Path.Combine(_directory, finalFileName);
            
            if (File.Exists(zipPath)) File.Delete(zipPath);
            
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

    public Result<IEnumerable<Cita>, DomainError> Restuarar(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return Result.Failure<IEnumerable<Cita>, DomainError>(new BackUpError.DirectoryNotFound(path))
                    .TapError(l => _logger.Error("El archivo no existe en la ruta especificada: {Path}", path));
            }

            // Directorio temporal
            string tempDirectory = Directory.CreateDirectory(_tempName).FullName;
            
            _logger.Information("Extrayendo contenido del backup...");
            ZipFile.ExtractToDirectory(path, tempDirectory);

            var vehiculos = _storage.Cargar();
            
            // Limpieza
            Directory.Delete(tempDirectory, true);
            
            return Result.Success<IEnumerable<Cita>, DomainError>(vehiculos.Value)
                .Tap(l => _logger.Information("Vehiculos cargados exitosamente desde la ruta: {Path}", path));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Cita>, DomainError>(new BackUpError(ex.Message))
                .TapError(l => _logger.Error(ex, "Error crítico al intentar restaurar el backup desde {Path}", path));
        }
    }

    public IEnumerable<string> Listar() 
    {
        
        if (!Directory.Exists(_directory)) 
        {
            return Enumerable.Empty<string>();
        }
        
        var archivos = Directory.GetFiles(_directory, "*.*") 
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();

        return archivos;
    }
}