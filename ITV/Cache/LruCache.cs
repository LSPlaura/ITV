using ITV.Cache;
using ITV.Config;
using ITV.Models;
using Serilog;


namespace ITV.Cache;


public class LruCache : ICache<int, Cita>
{
   private int _max;
   private Dictionary<int, Cita> _data = new Dictionary<int, Cita>();
   private LinkedList<int> _orderUsage = new LinkedList<int>();
   private readonly ILogger _logger = Log.ForContext<LruCache>();

   public LruCache(int capacidadCache)
   {
       if (capacidadCache <= 0) capacidadCache = Configuracion.Cache;
       _max = capacidadCache;
   }
  
   public void Agregar(int key, Cita value)
   {
       _logger.Debug("Intentando agregar vehículo a la caché. ID: {Key}", key);
      
       if (key != value.Id)
       {
           _logger.Warning("La matrícula introducida como clave {Key} no coincide con la matrícula del vehículo {Matricula}, no se añadirá a la cache", key, value.Matricula);
           return;
       }
      
       if (_data.ContainsKey(key))
       {
           _logger.Information("El vehículo con ID {Key} ya existe en la caché. Actualizando posición.", key);
           _data[key] = value;
           Actualizar(key);
           return;
       }


       if (_data.Count >= _max)
       {
           var idAEliminar = _orderUsage.First!.Value;
           _logger.Information("Caché llena (Capacidad: {Max}). Eliminando el elemento menos usado: {OldKey}", _max, idAEliminar);
          
           _orderUsage.RemoveFirst();
           _data.Remove(idAEliminar);
       }


       _logger.Information("Añadiendo nuevo vehículo a la caché. ID: {Key}", key);
       _data.Add(key, value);
       _orderUsage.AddLast(key);
   }


   public Cita? Obtener(int key)
   {
       _logger.Debug("Buscando vehículo en caché. ID: {Key}", key);
      
       if (_data.TryGetValue(key, out var value))
       {
           _logger.Information("Vehículo {Key} encontrado. Actualizándolo y devolviéndolo", key);
           Actualizar(key);
           return value;
       }
      
       _logger.Warning("No se ha podido encontrar el vehículo con ID {Key} en la caché", key);
       return null;
   }


   public void MostrarCache()
   {
       _logger.Information("Listando estado actual de la caché LRU (Total: {Count})", _orderUsage.Count);
       int contador = 1;
       _orderUsage.ToList().ForEach(p =>
       {
           Console.WriteLine($"Puesto {contador}: {p}");
           contador++;
       });
   }
  
   private void Actualizar(int key)
   {
       _logger.Debug("Actualizando prioridad del ID: {Key}", key);
      
       if (_orderUsage.Remove(key))
       {
           _orderUsage.AddLast(key);
           _logger.Debug("ID {Key} movido al final de la lista (Reciente)", key);
       }
       else
       {
           _logger.Error("Error crítico: Se intentó actualizar el ID {Key} pero no existía en la lista de uso", key);
       }
   }
  
   public bool Borrar(int key)
   {
       _logger.Information("Eliminando vehículo de la caché. ID: {Key}", key);
      
      
       if (_orderUsage.Remove(key) && _data.Remove(key))
       {
           _logger.Debug("ID {Key} eliminado correctamente de ambas estructuras", key);
           return true;
       }
      
       _logger.Warning("Intento de borrado fallido o parcial para ID {Key}",key);
       return false;
   }
}