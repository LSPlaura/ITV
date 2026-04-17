using System.IO;
using System.Text;
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

namespace ITV.Storage.CSV;

public class StorageVehiculoCsv : IStorageVehiculo
{
    private readonly ILogger _logger = Log.ForContext<StorageVehiculoCsv>();

    /// <inheritdoc/>
    public Result<bool, DomainError> Salvar(IEnumerable<Vehiculo> items, string path)
    {
        try
        {
            _logger.Information("Iniciando exportación de datos a CSV en la ruta: {Path}", path);

            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine("Id;Matrícula;Marca;Modelo;Cilíndrica;Motor;DniDueño;IsDeleted");
            
            var lista = items.Select(p => p.ToDto()).ToList();
            lista.ForEach(c =>
                    writer.WriteLine(
                        $"{c.Id};{c.Matricula};{c.Marca};{c.Modelo};{c.Cilindrada};{c.Motor};{c.DniDueño};{c.IsDeleted}"));

            return Result.Success<bool, DomainError>(true).Tap(l =>
                _logger.Information("Exportación a CSV finalizada con éxito. Registros guardados: {Count}",
                    lista.Count));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>(new StorageError(ex.Message))
                .TapError(err => _logger.Error(ex, "Error crítico al intentar salvar el archivo CSV en {Path}", path));
        }
    }

    /// <inheritdoc/>
    public Result<IEnumerable<Vehiculo>, DomainError> Cargar(string path)
    {
        if (!File.Exists(path))
            return Result
                .Failure<IEnumerable<Vehiculo>, DomainError>(
                    new StorageError(($"El archivo {path} no existe")))
                .TapError(l =>
                    _logger.Error("No se puede cargar el CSV: El archivo no existe en la ruta {Path}", path));

        try
        {
            _logger.Information("Iniciando lectura de datos desde CSV: {Path}", path);

            var resultado = File.ReadLines(path)
                .Skip(1)
                .Select(l => l.Split(";"))
                .Select(campos => new VehiculoDto(
                    int.Parse(campos[0]),
                    campos[1],
                    campos[2],
                    campos[3],
                    double.Parse(campos[4]),
                    int.Parse(campos[5]),
                    campos[6],
                    int.Parse(campos[7])
                ).ToModel())
                .ToList(); // Lo pasamos a lista para confirmar la lectura antes de cerrar el bloque

            return Result.Success<IEnumerable<Vehiculo>, DomainError>(resultado)
                .Tap(l => _logger.Information(
                    "Carga de CSV completada satisfactoriamente. Registros recuperados: {Count}", resultado.Count));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError(ex.Message))
                .TapError(l =>
                    _logger.Error(ex, "Error crítico durante el procesamiento del archivo CSV en {Path}", path));
        }
    }
    /// <summary>
    /// Crea la carpeta en la que se guardan los ficheros, en el caso de que no existiese
    /// </summary>
    private void Init()
    {
        if (!Directory.Exists(Configuracion.StorageFolder)) 
        {
            _logger.Information("Directorio base no encontrado. Creando carpeta para CSV en: {Path}", Configuracion.StorageFolder);
            Directory.CreateDirectory(Configuracion.StorageFolder);
        }
    }

    /// <summary>
    /// Constructor para implementar la creacion de la carpeta, si fuese necesario
    /// </summary>
    public StorageVehiculoCsv()
    {
        Init();
    }
}