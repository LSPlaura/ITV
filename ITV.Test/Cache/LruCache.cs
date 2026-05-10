using FluentAssertions;
using ITV.Cache;
using ITV.Models;
using NUnit.Framework;

namespace ITV.Test.Cache;

[TestFixture]
public class CacheLruTest
{
    [TestFixture]
    public class CasosValidos
    {
        private LruCache _cache = null!;
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-2);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(10);

        [SetUp]
        public void SetUp()
        {
            _cache = new LruCache(2);
        }
      
        [Test]
        public void Añadir_MaxCache_SeEliminaMenosUsado()
        {
            var vehiculo1 = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z") { Id = 1 };
            var vehiculo2 = new Cita(FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T") { Id = 2 };
            var vehiculo3 = new Cita(FechaMat, FechaInsp, "0000DWX", "Yamaha", "MT Zero", 600.0, Motor.Gasolina, "99999999R") { Id = 3 };
          
            _cache.Agregar(vehiculo1.Id, vehiculo1);
            _cache.Agregar(vehiculo2.Id, vehiculo2);
          
            var obtenerV1 = _cache.Obtener(vehiculo1.Id); // Accedemos a V1
            obtenerV1.Should().NotBeNull();
           
            _cache.Agregar(vehiculo3.Id, vehiculo3); // Expulsa a V2 porque V1 fue usado recientemente
            var obtenervV2 = _cache.Obtener(vehiculo2.Id);
            obtenervV2.Should().BeNull();
        }
      
        [Test]
        public void Añadir_VehiculoYaAgregado_SeActualizaPosicion()
        {
            var v1 = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z") { Id = 1 };
            var v2 = new Cita(FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T") { Id = 2 };
            var v3 = new Cita(FechaMat, FechaInsp, "0000DWX", "Yamaha", "MT Zero", 600.0, Motor.Gasolina, "99999999R") { Id = 3 };
          
            _cache.Agregar(v1.Id, v1);
            _cache.Agregar(v2.Id, v2);
            _cache.Agregar(v1.Id, v1); // Actualiza v1 al ser "nuevo" de nuevo
            
            _cache.Agregar(v3.Id, v3); // Elimina v2
            _cache.Obtener(v1.Id).Should().NotBeNull().And.BeSameAs(v1);
        }

        [Test]
        public void Obtener_SeObtieneVehiculo()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z") { Id = 10 };
            _cache.Agregar(vehiculo.Id, vehiculo);

            var obtenido = _cache.Obtener(vehiculo.Id);
            obtenido.Should().NotBeNull();
            obtenido!.Id.Should().Be(vehiculo.Id);
        }
      
        [Test]
        public void Obtener_SeActualizaPosicion()
        {
            var v1 = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z") { Id = 1 };
            var v2 = new Cita(FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T") { Id = 2 };
            var v3 = new Cita(FechaMat, FechaInsp, "0000DWX", "Yamaha", "MT Zero", 600.0, Motor.Gasolina, "99999999R") { Id = 3 };
          
            _cache.Agregar(v1.Id, v1);
            _cache.Agregar(v2.Id, v2);
            _cache.Obtener(v1.Id); // Al obtener v1, v2 se queda como el LRU (menos usado)
            
            _cache.Agregar(v3.Id, v3); // Expulsa a v2
            _cache.Obtener(v1.Id).Should().NotBeNull();
            _cache.Obtener(v2.Id).Should().BeNull();
        }
      
        [Test]
        public void Borrar_SeBorrar()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z") { Id = 5 };
          
            _cache.Agregar(vehiculo.Id, vehiculo);
            _cache.Borrar(vehiculo.Id).Should().BeTrue();
            _cache.Obtener(vehiculo.Id).Should().BeNull();
        }
    }
   
    [TestFixture]
    public class CasosInvalidos
    {
        private LruCache _cache = null!;
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-2);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(10);

        [SetUp]
        public void SetUp()
        {
            _cache = new LruCache(2);
        }
        
        [Test]
        public void ParametroPorDefecto_EstablecidoSiCapacidadNoValida()
        {
            _cache = new LruCache(0);
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z") { Id = 1 };
            _cache.Agregar(vehiculo.Id, vehiculo);
            var result = _cache.Obtener(vehiculo.Id);
            result.Should().NotBeNull();
        }
      
        [Test]
        public void Agregar_NoSeAgregaSiKeyYMatriculaDiferentes()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z") { Id = 1 };
            // Intentamos agregar con una key (99) distinta a su ID (1)
            _cache.Agregar(99, vehiculo);

            var obtenido = _cache.Obtener(vehiculo.Id);
            obtenido.Should().BeNull();
        }
       
        [Test]
        public void Obtener_NoAgregado_Null()
        {
            _cache.Obtener(999).Should().BeNull();
        }
       
        [Test]
        public void Borrar_NoAgregado_Null()
        {
            _cache.Borrar(999).Should().BeFalse();
        }
    }
}