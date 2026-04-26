using FluentAssertions;
using ITV.Cache;
using ITV.Models;

namespace ITV.Test.Cache;

public class CacheLruTest
{
   private LruCache _cache = null!;
  
   [SetUp]
   public void SetUp()
   {
       _cache = new LruCache(2);
   }


   [TestFixture]
   public class CasosValidos()
   {
       private LruCache _cache = null!;


       [SetUp]
       public void SetUp()
       {
           _cache = new LruCache(2);
       }
      
       public void Añadir_MaxCache_SeEliminaMenosUsado()
       {
           var vehiculo1 = new Vehiculo(
               "1234BBB",
               "Seat",
               "Ibiza",
               1200.0,
               Motor.Gasolina,
               "12345678Z"
           );


           var vehiculo2 = new Vehiculo(
               "9876FGH",
               "Aston Martin",
               "Vantage",
               4000.0,
               Motor.Gasolina,
               "00000000T"
           );


           var vehiculo3 = new Vehiculo(
               "0000DWX",
               "Yamaha",
               "MT Zero",
               600.0,
               Motor.Gasolina,
               "99999999R"
           );
          
           _cache.Agregar(vehiculo1.Matricula, vehiculo1);
           _cache.Agregar(vehiculo2.Matricula, vehiculo2);
          
           var obtenerV1 = _cache.Obtener(vehiculo1.Matricula);
           obtenerV1.Should().NotBeNull();
           obtenerV1.Matricula.Should().Be(vehiculo1.Matricula);
          
           var obtenervV2 = _cache.Obtener(vehiculo2.Matricula);
           obtenervV2.Should().BeNull();
          
           _cache.Agregar(vehiculo3.Matricula, vehiculo3);
       }
      
       public void Añadir_VehiculoYaAgregado_SeActualizaPosicion()
       {
           var vehiculo1 = new Vehiculo(
               "1234BBB",
               "Seat",
               "Ibiza",
               1200.0,
               Motor.Gasolina,
               "12345678Z"
           );


           var vehiculo2 = new Vehiculo(
               "9876FGH",
               "Aston Martin",
               "Vantage",
               4000.0,
               Motor.Gasolina,
               "00000000T"
           );


           var vehiculo3 = new Vehiculo(
               "0000DWX",
               "Yamaha",
               "MT Zero",
               600.0,
               Motor.Gasolina,
               "99999999R"
           );
          
           _cache.Agregar(vehiculo1.Matricula, vehiculo1);
           _cache.Agregar(vehiculo2.Matricula, vehiculo2);
           _cache.Agregar(vehiculo1.Matricula, vehiculo1);
           _cache.Obtener(vehiculo1.Matricula).Should().NotBeNull().And.BeSameAs(vehiculo1);
           _cache.Agregar(vehiculo3.Matricula, vehiculo3);
           _cache.Obtener(vehiculo1.Matricula).Should().NotBeNull().And.BeSameAs(vehiculo1);
       }


       //misma comprobación para agregar simple
       public void Obtener_SeObtieneVehiculo()
       {
           var vehiculo = new Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
           _cache.Agregar(vehiculo.Matricula, vehiculo);


           var obtenido = _cache.Obtener(vehiculo.Matricula);
           obtenido.Should().NotBe(null);
           obtenido.Matricula.Should().Be(vehiculo.Matricula);
       }
      
       public void Obtener_SeActualizaPosicion()
       {
           var vehiculo1 = new Vehiculo(
               "1234BBB",
               "Seat",
               "Ibiza",
               1200.0,
               Motor.Gasolina,
               "12345678Z"
           );


           var vehiculo2 = new Vehiculo(
               "9876FGH",
               "Aston Martin",
               "Vantage",
               4000.0,
               Motor.Gasolina,
               "00000000T"
           );


           var vehiculo3 = new Vehiculo(
               "0000DWX",
               "Yamaha",
               "MT Zero",
               600.0,
               Motor.Gasolina,
               "99999999R"
           );
          
           _cache.Agregar(vehiculo1.Matricula, vehiculo1);
           _cache.Agregar(vehiculo2.Matricula, vehiculo2);
           _cache.Agregar(vehiculo1.Matricula, vehiculo1);
           _cache.Agregar(vehiculo3.Matricula, vehiculo3);
           _cache.Obtener(vehiculo1.Matricula).Should().NotBeNull().And.BeSameAs(vehiculo1);
       }


       public void Actualizar_ActualizaOrden()
       {
           var vehiculo1 = new Vehiculo(
               "1234BBB",
               "Seat",
               "Ibiza",
               1200.0,
               Motor.Gasolina,
               "12345678Z"
           );


           var vehiculo2 = new Vehiculo(
               "9876FGH",
               "Aston Martin",
               "Vantage",
               4000.0,
               Motor.Gasolina,
               "00000000T"
           );


           var vehiculo3 = new Vehiculo(
               "0000DWX",
               "Yamaha",
               "MT Zero",
               600.0,
               Motor.Gasolina,
               "99999999R"
           );
          
           _cache.Agregar(vehiculo1.Matricula, vehiculo1);
           _cache.Agregar(vehiculo2.Matricula, vehiculo2);
           _cache.Actualizar(vehiculo1.Matricula);
           _cache.Agregar(vehiculo3.Matricula, vehiculo3);
           _cache.Obtener(vehiculo1.Matricula).Should().NotBeNull().And.BeSameAs(vehiculo1);
       }
      
       public void Borrar_SeBorrar()
       {
           var vehiculo = new Vehiculo(
               "1234BBB",
               "Seat",
               "Ibiza",
               1200.0,
               Motor.Gasolina,
               "12345678Z"
           );
          
           _cache.Agregar(vehiculo.Matricula, vehiculo);
           _cache.Borrar(vehiculo.Matricula).Should().BeTrue();
           _cache.Obtener(vehiculo.Matricula).Should().BeNull();
       }
   }
  
   public class CasosInvalidos()
   {
       private LruCache _cache = null!;


       [SetUp]
       public void SetUp()
       {
           _cache = new LruCache(2);
       }
      
       public void Agregar_NoSeAgregaSiKeyYMatriculaDiferentes()
       {
           var vehiculo = new Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
           _cache.Agregar("111BBB", vehiculo);


           var obtenido = _cache.Obtener(vehiculo.Matricula);
           obtenido.Should().NotBeNull();
       }


       public void Obtener_NoAgregado_Null()
       {
           var vehiculo = new Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
           _cache.Obtener(vehiculo.Matricula).Should().BeNull();
       }
      
       public void Actualizar_NoAgregado_Null()
       {
           var vehiculo = new Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
           _cache.Actualizar(vehiculo.Matricula);
           _cache.Obtener(vehiculo.Matricula).Should().BeNull();
       }
      
       public void Borrar_NoAgregado_Null()
       {
           var vehiculo = new Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
           _cache.Borrar(vehiculo.Matricula).Should().BeFalse();
           _cache.Obtener(vehiculo.Matricula).Should().BeNull();
       }
   }
}