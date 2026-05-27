using ITV.Config;
using ITV.Models;
using ITV.Storage.Common;
using ITV.Storage.CSV;
using ITV.Storage.XML;

namespace ITV.Factories.Provider;

public static class StorageFactory
{
    /// <summary>
    /// Método para crear el storage deseado segín el parámetro dado
    /// </summary>
    /// <param name="formato">Nombre del storage que se quiere</param>
    /// <returns></returns>
    public static IStorage<Cita> ObtenerStorage(string formato)
    {
        return formato.ToLower().Trim() switch
        {
            "csv" => new StorageCitaCsv(Configuracion.StorageFile, Configuracion.StorageFolder),
            "xml" => new StorageCitaJson(Configuracion.StorageFile, Configuracion.StorageFolder),
            "json" => new StorageCitaXml(Configuracion.StorageFile, Configuracion.StorageFolder),
            _ => new StorageCitaCsv(Configuracion.StorageFile, Configuracion.StorageFolder)
        };
    }
}