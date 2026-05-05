using FluentAssertions;
using ITV.Cache;
using ITV.Models;

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
            var vehiculo1 = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z");
            var vehiculo2 = new Cita(FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T");
            var vehiculo3 = new Cita(FechaMat, FechaInsp, "0000DWX", "Yamaha", "MT Zero", 600.0, Motor.Gasolina, "99999999R");
          
            _cache.Agregar(vehiculo1.Matricula, vehiculo1);
            _cache.Agregar(vehiculo2.Matricula, vehiculo2);
          
            var obtenerV1 = _cache.Obtener(vehiculo1.Matricula);
            obtenerV1.Should().NotBeNull();
            obtenerV1.Matricula.Should().Be(vehiculo1.Matricula);
           
            _cache.Agregar(vehiculo3.Matricula, vehiculo3);
            var obtenervV2 = _cache.Obtener(vehiculo2.Matricula);
            obtenervV2.Should().BeNull();
        }
      
        [Test]
        public void Añadir_VehiculoYaAgregado_SeActualizaPosicion()
        {
            var vehiculo1 = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z");
            var vehiculo2 = new Cita(FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T");
            var vehiculo3 = new Cita(FechaMat, FechaInsp, "0000DWX", "Yamaha", "MT Zero", 600.0, Motor.Gasolina, "99999999R");
          
            _cache.Agregar(vehiculo1.Matricula, vehiculo1);
            _cache.Agregar(vehiculo2.Matricula, vehiculo2);
            _cache.Agregar(vehiculo1.Matricula, vehiculo1);
            _cache.Obtener(vehiculo1.Matricula).Should().NotBeNull().And.BeSameAs(vehiculo1);
            _cache.Agregar(vehiculo3.Matricula, vehiculo3);
            _cache.Obtener(vehiculo1.Matricula).Should().NotBeNull().And.BeSameAs(vehiculo1);
        }

        [Test]
        public void Obtener_SeObtieneVehiculo()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            _cache.Agregar(vehiculo.Matricula, vehiculo);

            var obtenido = _cache.Obtener(vehiculo.Matricula);
            obtenido.Should().NotBeNull();
            obtenido.Matricula.Should().Be(vehiculo.Matricula);
        }
      
        [Test]
        public void Obtener_SeActualizaPosicion()
        {
            var vehiculo1 = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z");
            var vehiculo2 = new Cita(FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T");
            var vehiculo3 = new Cita(FechaMat, FechaInsp, "0000DWX", "Yamaha", "MT Zero", 600.0, Motor.Gasolina, "99999999R");
          
            _cache.Agregar(vehiculo1.Matricula, vehiculo1);
            _cache.Agregar(vehiculo2.Matricula, vehiculo2);
            _cache.Agregar(vehiculo1.Matricula, vehiculo1);
            _cache.Agregar(vehiculo3.Matricula, vehiculo3);
            _cache.Obtener(vehiculo1.Matricula).Should().NotBeNull().And.BeSameAs(vehiculo1);
        }

        [Test]
        public void Actualizar_ActualizaOrden()
        {
            var vehiculo1 = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z");
            var vehiculo2 = new Cita(FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T");
            var vehiculo3 = new Cita(FechaMat, FechaInsp, "0000DWX", "Yamaha", "MT Zero", 600.0, Motor.Gasolina, "99999999R");
          
            _cache.Agregar(vehiculo1.Matricula, vehiculo1);
            _cache.Agregar(vehiculo2.Matricula, vehiculo2);
            _cache.Actualizar(vehiculo1.Matricula);
            _cache.Agregar(vehiculo3.Matricula, vehiculo3);
            _cache.Obtener(vehiculo1.Matricula).Should().NotBeNull().And.BeSameAs(vehiculo1);
        }
      
        [Test]
        public void Borrar_SeBorrar()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z");
          
            _cache.Agregar(vehiculo.Matricula, vehiculo);
            _cache.Borrar(vehiculo.Matricula).Should().BeTrue();
            _cache.Obtener(vehiculo.Matricula).Should().BeNull();
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
        public void Agregar_NoSeAgregaSiKeyYMatriculaDiferentes()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            _cache.Agregar("111BBB", vehiculo);

            var obtenido = _cache.Obtener(vehiculo.Matricula);
            obtenido.Should().BeNull();
        }
       
        [Test]
        public void Obtener_NoAgregado_Null()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            _cache.Obtener(vehiculo.Matricula).Should().BeNull();
        }
       
        [Test]
        public void Actualizar_NoAgregado_Null()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            _cache.Actualizar(vehiculo.Matricula);
            _cache.Obtener(vehiculo.Matricula).Should().BeNull();
        }
       
        [Test]
        public void Borrar_NoAgregado_Null()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            _cache.Borrar(vehiculo.Matricula).Should().BeFalse();
            _cache.Obtener(vehiculo.Matricula).Should().BeNull();
        }
    }
}