using ITV.Cache.CommonLRU;
using ITV.Config;
using ITV.Models;
using Serilog;

namespace ITV.Cache;

public class LruCache : ILruVehiculo
{
    private int _max = Configuracion.Cache;
    private Dictionary<string, Vehiculo> _data = new Dictionary<string, Vehiculo>();
    private LinkedList<string> _orderOfUsege = new LinkedList<string>();
    private readonly ILogger _logger = Log.ForContext<LruCache>();
    
    public void Agregar(string key, Vehiculo value)
    {
        _logger.Debug("Intentando agregar vehículo a la caché. ID: {Key}", key);
        
        if (_orderOfUsege.Contains(key))
        {
            _logger.Information("El vehículo con ID {Key} ya existe en la caché. Actualizando posición.", key);
            Actualizar(key);
            return;
        }

        if (_orderOfUsege.Count >= _max)
        {
            var idAEliminar = _orderOfUsege.First!.Value;
            _logger.Information("Caché llena (Capacidad: {Max}). Eliminando el elemento menos usado: {OldKey}", _max, idAEliminar);
            
            _orderOfUsege.RemoveFirst();
            _data.Remove(idAEliminar);
        }

        _logger.Debug("Añadiendo nuevo vehículo a la caché. ID: {Key}", key);
        _data.Add(key, value);
        _orderOfUsege.AddLast(key);
    }

    public Vehiculo? Obtener(string key)
    {
        _logger.Debug("Buscando vehículo en caché. ID: {Key}", key);
        
        if (_data.TryGetValue(key, out var value))
        {
            _logger.Debug("Vehículo {Key} encontrado. Marcando como usado recientemente.", key);
            Actualizar(key);
            return value;
        }
        
        _logger.Warning("No se ha podido encontrar el vehículo con ID {Key} en la caché", key);
        return null;
    }

    public void MostrarCache()
    {
        _logger.Information("Listando estado actual de la caché LRU (Total: {Count})", _orderOfUsege.Count);
        int contador = 1;
        _orderOfUsege.ToList().ForEach(p =>
        {
            Console.WriteLine($"Puesto {contador}: {p}");
            contador++;
        });
    }
    
    public void Actualizar(string key)
    {
        _logger.Debug("Actualizando prioridad del ID: {Key}", key);
        var removido = _orderOfUsege.Remove(key);
        
        if (removido)
        {
            _orderOfUsege.AddLast(key);
            _logger.Debug("ID {Key} movido al final de la lista (Reciente)", key);
        }
        else
        {
            _logger.Error("Error crítico: Se intentó actualizar el ID {Key} pero no existía en la lista de uso", key);
        }
    }
    
    public bool Borrar(string key)
    {
        _logger.Information("Eliminando vehículo de la caché. ID: {Key}", key);
        bool eliminadoLista = _orderOfUsege.Remove(key);
        bool eliminadoData = _data.Remove(key);
        
        if (eliminadoLista && eliminadoData)
        {
            _logger.Debug("ID {Key} eliminado correctamente de ambas estructuras", key);
            return true;
        }
        
        _logger.Warning("Intento de borrado fallido o parcial para ID {Key}. (En Lista: {InList}, En Diccionario: {InDict})", 
            key, eliminadoLista, eliminadoData);
        return false;
    }
}