using ITV.Models;
using ITV.Storage.Common;

namespace ITV.Test.Storage;

using System.IO;
using FluentAssertions;

[TestFixture]
public class StorageJsonTests
{
        [TestFixture]
        public class CasosValidos
        {
            private IStorageVehiculo _storage = null!;
            private string _filePath = "VehiculosTest";
            private string _directoryPath = "DataTest";

            [SetUp]
            public void SetUp()
            {
                _storage = new StorageVehiculoJson(_filePath, _directoryPath);
            }

            [TearDown]
            public void SetDown()
            {
                if (Directory.Exists(_directoryPath))
                {
                    Directory.Delete(_directoryPath, true);
                }
            }

            [Test]
            public void Salvar_True()
            {
                var vehiculosValidos = GetVehiculosDePrueba();
                var result = _storage.Salvar(vehiculosValidos);

                result.Should().NotBe(null);
                result.IsSuccess.Should().BeTrue();
                result.Value.Should().Be(true);
            }

            [Test]
            public void Cargar_EnumerableVehiculos()
            {
                var vehiculosValidos = GetVehiculosDePrueba();
                _storage.Salvar(vehiculosValidos);

                var result = _storage.Cargar();
                
                result.Should().NotBe(null);
                result.IsSuccess.Should().BeTrue();
                result.Value.Should().HaveCount(5);
            }

            [TestCase("VehiculosTest", "DataTest")]
            public void Init_Exist_True(string file, string directory)
            {
                var storage = new StorageVehiculoJson(file, directory);
                var result = Directory.Exists(directory);
                result.Should().BeTrue();
            }
        }
        
        [TestFixture]
        public class CasosInvalidos
        {
            private IStorageVehiculo _storage = null!;
            private string _filePath = "VehiculosTest";
            private string _directoryPath = "DataTest";

            [SetUp]
            public void SetUp()
            {
                _storage = new StorageVehiculoJson(_filePath, _directoryPath);
            }

            [TearDown]
            public void SetDown()
            {
                if (Directory.Exists(_directoryPath))
                {
                    Directory.Delete(_directoryPath, true);
                }
            }

            [Test]
            public void Cargar_DeserializacionFallida_StorageError()
            {
                var result = _storage.Cargar();
                result.Should().NotBe(null);
                result.IsFailure.Should().BeTrue();
            }

            [TestCase("Vehiculos", "")]
            [TestCase("", "DataTest")]
            public void 
                Salvar_PathInvalido_StorageError(string file, string directory)
            {
                var storage = new StorageVehiculoJson(file, directory);
                var vehiculosValidos = GetVehiculosDePrueba();

                var result = storage.Salvar(vehiculosValidos);

                result.Should().NotBe(null);
                result.IsFailure.Should().BeFalse();
            }
        }
    
    private static List<Vehiculo> GetVehiculosDePrueba()
    {
        return new List<Vehiculo>
        {
            new Vehiculo("1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z"),
            new Vehiculo("9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T"),
            new Vehiculo("0000DWX", "Yamaha", "MT Zero", 600.0, Motor.Gasolina, "99999999R"),
            new Vehiculo("5544LNP", "Tesla", "Model Three", 0.0, Motor.Electrico, "54321098B"),
            new Vehiculo("8210ZRT", "Peugeot", "Dos mil ocho", 1500.0, Motor.Diesel, "11111111H")
        };
    }
}