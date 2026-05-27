using System.IO;
using System.Text;
using CSharpFunctionalExtensions;
using ITV.Config;
using ITV.Dto;
using ITV.Error.Common;
using ITV.Error.Storage;
using ITV.Mappers;
using ITV.Models;
using ITV.Storage.Common;
using Serilog;

namespace ITV.Storage.CSV;

public class StorageCitaCsv : IStorageCita
{
    private readonly ILogger _logger = Log.ForContext<StorageCitaCsv>();
    private string _filePath;
    private string _directoryPath;
    private string _fullPath;
    
    /// <summary>
    /// Constructor para implementar la creacion de la carpeta, si fuese necesario
    /// </summary>
    public StorageCitaCsv(string filePath, string directoryPath)
    {
        _filePath = filePath;
        _directoryPath = directoryPath;
        Init();
        _fullPath = Path.Combine(directoryPath, filePath + ".csv");
    }

    /// <inheritdoc/>
    public Result<bool, DomainError> Salvar(IEnumerable<Cita> items)
    {
        try
        {
            _logger.Information("Iniciando exportación de datos a CSV en la ruta: {Path}", _fullPath);

            using var writer = new StreamWriter(_fullPath, false, Encoding.UTF8);
            writer.WriteLine("Id;FechaMatriculación;FechaInspeccion;Matrícula;Marca;Modelo;Cilíndrica;Motor;DniDueño;IsDeleted;CreatedAt;UpdatedAt");
            
            var lista = items.Select(p => p.ToDto()).ToList();
            lista.ForEach(c =>
                writer.WriteLine(
                    $"{c.Id};{c.FechaMatriculacion};{c.FechaInspeccion};{c.Matricula};{c.Marca};{c.Modelo};{c.Cilindrada};{c.Motor};{c.DniDueño};{c.IsDeleted};{c.CreatedAt};{c.UpdatedAt}"));

            return Result.Success<bool, DomainError>(true).Tap(l =>
                _logger.Information("Exportación a CSV finalizada con éxito. Registros guardados: {Count}",
                    lista.Count));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>(new StorageError(ex.Message))
                .TapError(err => _logger.Error(ex, "Error crítico al intentar salvar el archivo CSV en {Path}", _fullPath));
        }
    }

    /// <inheritdoc/>
    public Result<IEnumerable<Cita>, DomainError> Cargar()
    {
        if (!File.Exists(_fullPath))
            return Result
                .Failure<IEnumerable<Cita>, DomainError>(
                    new StorageError(($"El archivo {_fullPath} no existe")))
                .TapError(l =>
                    _logger.Error("No se puede cargar el CSV: El archivo no existe en la ruta {Path}", _fullPath));

        try
        {
            _logger.Information("Iniciando lectura de datos desde CSV: {Path}", _fullPath);

            var resultado = File.ReadLines(_fullPath)
                .Skip(1)
                .Select(l => l.Split(";"))
                .Select(campos => new CitaDto(
                    int.Parse(campos[0]),
                    campos[1],
                    campos[2],
                    campos[3],
                    campos[4],
                    campos[5],
                    double.Parse(campos[6]),
                    int.Parse(campos[7]),
                    campos[8],
                    int.Parse(campos[9]),
                    campos[10],
                    campos[11]
                ).ToModel())
                .ToList(); // Lo pasamos a lista para confirmar la lectura antes de cerrar el bloque

            return Result.Success<IEnumerable<Cita>, DomainError>(resultado)
                .Tap(l => _logger.Information(
                    "Carga de CSV completada satisfactoriamente. Registros recuperados: {Count}", resultado.Count));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Cita>, DomainError>(new StorageError(ex.Message))
                .TapError(l =>
                    _logger.Error(ex, "Error crítico durante el procesamiento del archivo CSV en {Path}", _fullPath));
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