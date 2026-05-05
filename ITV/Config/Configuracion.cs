
using System.IO;

namespace ITV.Config;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Configuración estática que lee los datos almacenados en el appsetting para obtener las variables necesarias y otras variables necesarias para crear rutas
/// </summary>
public static class Configuracion
{
    public static readonly IConfigurationRoot Config = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json", false, true)
        .Build();
    
    /// <summary>
    /// El máximo que puede ocupar la cache
    /// </summary>
    public static int Cache = Config.GetValue<int>("Cache:Max");

    //Repositorio
    /// <summary>
    /// El tipo de repositorio configurado
    /// </summary>
    public static string RepositoryType = Config.GetValue<string>("Repository:Type") ?? "efc";
    /// <summary>
    /// El fichero a usar cuando se use un repositorio conn una base de datos
    /// </summary>
    public static string DataBaseString = Config.GetValue<string>("Repository:Database") ?? "Data Source=Repository/citas.db";
    /// <summary>
    /// EL directorio en el que irán los archivos que se guarden del repositorio
    /// </summary>
    public static string RepositoryFolder = Config.GetValue<string>("Repository:Folder") ?? "Repository";
    
    //Storage
    /// <summary>
    /// El tipo de storage configurado
    /// </summary>
    public static string StorageType = Config.GetValue<string>("Storage:Type") ?? "json";
    /// <summary>
    /// El nombre del fichero en el que se guardarán los datos
    /// </summary>
    public static string StorageFile = Config.GetValue<string>("Storage:File") ?? "Vehiculos";
    /// <summary>
    /// El nombre del directorio que usará <see cref="IStorage"/>
    /// </summary>
    public static string StorageFolder = Config.GetValue<string>("Storage:Folder") ?? "Data";
    /// <summary>
    /// La ruta al archivo <see cref="StorageFile"/> con la extensión correcta dependiendo dle tipo de <see cref="IStorage"/>
    /// </summary>
    public static string StorageFilePath = Path.Combine(StorageFolder, StorageFile + "." + StorageType);
    
    //BackUp
    /// <summary>
    /// 
    /// </summary>
    public static string BackUpFile = Config.GetValue<string>("BuckUp:File") ?? "_BackUp";
    public static string BackUpFolder = Config.GetValue<string>("BuckUp:Folder") ?? "_BackUp.zip";
}