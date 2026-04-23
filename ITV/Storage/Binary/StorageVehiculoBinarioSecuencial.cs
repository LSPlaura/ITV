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

namespace ITV.Storage.Binary;

public class StorageVehiculoBinarioSecuencial : IStorageVehiculo
{
    private readonly ILogger _logger = Log.ForContext<StorageVehiculoBinarioSecuencial>();
    private string _filePath;
    private string _directoryPath;
    private string _fullPath;
    
    public StorageVehiculoBinarioSecuencial(string filePath, string directoryPath)
    {
        _filePath = filePath;
        _directoryPath = directoryPath;
        Init();
        _fullPath = Path.Combine(directoryPath, filePath + ".bin");
    }
    public  Result<bool, DomainError> Salvar(IEnumerable<Vehiculo> items)
    {
        _logger.Information("Iniciando guardado binario en: {Path}", _fullPath);
        
        try
        {
            using var stream = File.Create(_fullPath);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            var lista = items.ToList();
            writer.Write(lista.Count);
            
            foreach (var dto in lista.Select(i => i.ToDto()))
            {
                writer.Write(dto.Id);
                writer.Write(dto.Matricula);
                writer.Write(dto.Marca);
                writer.Write(dto.Modelo);
                writer.Write(dto.Cilindrada);
                writer.Write(dto.Motor);
                writer.Write(dto.DniDueño);
                writer.Write(dto.IsDeleted);
            }

            return Result.Success<bool, DomainError>(true).Tap(l =>
                _logger.Information("Guardado binario completado con éxito. Total: {Count} registros", lista.Count));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>(new StorageError(ex.Message))
                .TapError(err => _logger.Error(ex, "Error crítico no esperado al salvar vehículos en {Path}", _fullPath));
        }
    }

    public Result<IEnumerable<Vehiculo>, DomainError> Cargar()
    {
        _logger.Information("Leyendo archivo binario desde: {Path}", _fullPath);
        
        if (!File.Exists(_fullPath))
        {
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError(($"El archivo {_fullPath} no existe")))
                .TapError(l => _logger.Error("El archivo no existe en la ruta especificada: {Path}", _fullPath));
        }

        try
        {
            using var stream = File.OpenRead(_fullPath);
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            var count = reader.ReadInt32();
            var vehiculos = new List<Vehiculo>();
            
            for (var i = 0; i < count; i++)
            {
                var vehiculo = new VehiculoDto(
                    reader.ReadInt32(),
                    reader.ReadString(),
                    reader.ReadString(),
                    reader.ReadString(),
                    reader.ReadDouble(),
                    reader.ReadInt32(),
                    reader.ReadString(),
                    reader.ReadInt32()
                ).ToModel();
                vehiculos.Add(vehiculo);
            }

            return Result.Success<IEnumerable<Vehiculo>, DomainError>(vehiculos)
                .Tap(l => _logger.Information("Carga binaria finalizada. Se han recuperado {Count} registros",
                        vehiculos.Count));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(new StorageError(ex.Message))
                .TapError(l => _logger.Error(ex, "Error crítico no esperado al cargar vehículos en {Path}", _fullPath));
        }
    }

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