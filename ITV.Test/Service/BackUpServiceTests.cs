using System.IO;
using CSharpFunctionalExtensions;
using FluentAssertions;
using ITV.Error.BuckUp;
using ITV.Error.Common;
using ITV.Models;
using ITV.Service;
using ITV.Storage.Common;

namespace ITV.Test.Service;
using Moq;

[TestFixture]
public class BackUpServiceTests
{
   [TestFixture]
   public class CasosValidos
   {
      private Mock<IStorage<Vehiculo>> _mockStorage = null!;
      private IBackUpService<Vehiculo> _backUpService = null!;
      private string _file = "VehiculosTest";
      private string _folder = "BackUpTest";

      [SetUp]
      public void SetUp()
      {
         _mockStorage = new Mock<IStorage<Vehiculo>>();
         _backUpService = new BackupService(_mockStorage.Object, _file, _folder);
      }

      [TearDown]
      public void TearDown()
      {
         var parentDir = Directory.GetCurrentDirectory();
         var directories = Directory.GetDirectories(parentDir, $"*{_folder}*");

         foreach (var dir in directories)
            Directory.Delete(dir, true);

         if (Directory.Exists("tempBackup"))
            Directory.Delete("tempBackup", true);
      }

      [Test]
      public void GuardarBuckUp_Guarda_DevuelveRuta()
      {
         var vehiculo1 = new Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
         var vehiculo2 = new Vehiculo("2222BBB", "AAA", "ElMejor", 3.3, Motor.Gasolina, "12345678Z");
         var vehiculo3 = new Vehiculo("3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");

         var vehiculos = new List<Vehiculo> { vehiculo1, vehiculo2, vehiculo3 };

         var result = _backUpService.Guardar(vehiculos);

         result.IsSuccess.Should().BeTrue();
         result.Value.Should().BeOfType<string>();
    
         _mockStorage.Verify(s => s.Salvar(It.Is<IEnumerable<Vehiculo>>(v => v.Count() == 3)), Times.Once);
      }

      [Test]
      public void Restuarar()
      {
         var vehiculosOriginales = new List<Vehiculo> 
         {
            new("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z"),
            new("2222BBB", "AAA", "ElMejor", 3.3, Motor.Gasolina, "12345678Z"),
            new("3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z")
         };

         var mocoResult = Result.Success<IEnumerable<Vehiculo>, DomainError>(vehiculosOriginales.AsEnumerable());
         _mockStorage.Setup(s => s.Cargar()).Returns(mocoResult);

         var ruta = _backUpService.Guardar(vehiculosOriginales);
         var result = _backUpService.Restuarar(ruta.Value);

         result.IsSuccess.Should().BeTrue();
         result.Value.Count().Should().Be(3);
    
         _mockStorage.Verify(s => s.Cargar(), Times.Once);
      }

      [Test]
      public void Listar_DebeRetornarAlMenosUnArchivo()
      {
         var vehiculos = new List<Vehiculo> { new("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z") };
         _mockStorage.Setup(s => s.Salvar(It.IsAny<IEnumerable<Vehiculo>>()));
    
         var guardarResult = _backUpService.Guardar(vehiculos);
         var result = _backUpService.Listar();
    
         result.Should().NotBeEmpty();
         result.Should().Contain(guardarResult.Value);
    
         _mockStorage.Verify(s => s.Salvar(It.IsAny<IEnumerable<Vehiculo>>()), Times.AtLeastOnce);
      }

      [Test]
      public void Listar_ListaVaciaRetornaEnumerableVacio()
      {
         var result = _backUpService.Listar();

         result.Should().BeEmpty();
      }
   }

   [TestFixture]
   public class CasosInvalidos
   {
      private Mock<IStorage<Vehiculo>> _mockStorage = null!;
      private IBackUpService<Vehiculo> _backUpService = null!;
      private string _file = "VehiculosTest";
      private string _folder = "BackUpTest";

      [SetUp]
      public void SetUp()
      {
         _mockStorage = new Mock<IStorage<Vehiculo>>();
         _backUpService = new BackupService(_mockStorage.Object, _file, _folder);
      }

      [TearDown]
      public void TearDown()
      {
         var parentDir = Directory.GetCurrentDirectory();
         var directories = Directory.GetDirectories(parentDir, $"*{_folder}*");

         foreach (var dir in directories)
            Directory.Delete(dir, true);

         if (Directory.Exists("tempBackup"))
            Directory.Delete("tempBackup", true);
      }

      [Test]
      public void Guardar_CuandoStorageFalla_RetornaFailure()
      {
         var vehiculos = new List<Vehiculo> { new("1111BBB", "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z") };

         _mockStorage.Setup(s => s.Salvar(It.IsAny<IEnumerable<Vehiculo>>()))
            .Throws(new Exception("Error de escritura en disco"));

         var result = _backUpService.Guardar(vehiculos);
    
         result.IsFailure.Should().BeTrue();
         result.Error.Should().BeOfType<BackUpError>();
    
         _mockStorage.Verify(s => s.Salvar(It.IsAny<IEnumerable<Vehiculo>>()), Times.Once);
      }

      [Test]
      public void Restuarar_CuandoArchivoNoExiste_RetornaDirectoryNotFound()
      {
         string rutaInexistente = "ruta/ficticia/archivo_que_no_existe.zip";
    
         var result = _backUpService.Restuarar(rutaInexistente);
    
         result.IsFailure.Should().BeTrue();
         result.Error.Should().BeOfType<BackUpError.DirectoryNotFound>();
    
         _mockStorage.Verify(s => s.Cargar(), Times.Never);
      }

      [Test]
      public void Restuarar_CuandoZipEstaCorrupto_RetornaFailure()
      {
         string rutaCorrupta = Path.Combine(Directory.GetCurrentDirectory(), "corrupto.zip");
         File.WriteAllText(rutaCorrupta, "Esto no es un archivo zip");
    
         var result = _backUpService.Restuarar(rutaCorrupta);
    
         result.IsFailure.Should().BeTrue();
    
         _mockStorage.Verify(s => s.Cargar(), Times.Never);
    
         if (File.Exists(rutaCorrupta)) File.Delete(rutaCorrupta);
      }

      [Test]
      public void Restuarar_CuandoStorageCargarFalla_RetornaFailure()
      {
         var vehiculos = new List<Vehiculo>();
         var rutaResult = _backUpService.Guardar(vehiculos);

         _mockStorage.Setup(s => s.Cargar())
            .Returns(Result.Failure<IEnumerable<Vehiculo>, DomainError>(new BackUpError("Error")));
    
         var result = _backUpService.Restuarar(rutaResult.Value);
    
         result.IsFailure.Should().BeTrue();
    
         _mockStorage.Verify(s => s.Cargar(), Times.Once);
      }
   }
}