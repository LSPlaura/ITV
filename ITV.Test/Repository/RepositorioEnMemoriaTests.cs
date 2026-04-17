using FluentAssertions;
using ITV.Error.Vehiculos;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Repository.Memory;

namespace ITV.Test.Repository;

[TestFixture]
public class RepositorioEnMemoriaTests
{
    [TestFixture]
    public class CasosValidos()
    {
        private IRepositorioVehiculos _repositorio = null!;

        [SetUp]
        public void SetUp()
        {
            _repositorio = new RepositorioEnMemoria();
        }

        [TearDown]
        public void TearDown()
        {
            _repositorio.DeleteAll();
        }

        [Test]
        public void Agregar_SinErrores()
        {
            var vehiculo = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");

            var result = _repositorio.Agregar(vehiculo);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().BeGreaterThan(0);
            result.Value.IsDeleted.Should().Be(false);
            
        }
    }
    
    [TestFixture]
    public class CasosInvalidos()
    {
        private IRepositorioVehiculos _repositorio = null!;

        [SetUp]
        public void SetUp()
        {
            _repositorio = new RepositorioEnMemoria();
        }

        [TearDown]
        public void TearDown()
        {
            _repositorio.DeleteAll();
        }

        [Test]
        public void Agregar_ErrorMatricula()
        {
            var vehiculo1 = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var vehiculo2 = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            
            var result1 = _repositorio.Agregar(vehiculo1);
            var result2 = _repositorio.Agregar(vehiculo2);
            result1.Should().NotBeNull();
            result1.IsSuccess.Should().BeTrue();
            result1.Value.Id.Should().BeGreaterThan(0);
            result1.Value.IsDeleted.Should().Be(false);
            
            result2.Should().NotBeNull();
            result2.IsFailure.Should().BeTrue();
            result2.Should().BeOfType<VehiculoError.VehiculoAlredyExist.MatriculaAlreadyExists>();
        }
        
        [Test]
        public void Agregar_ErrorOwnerCon3Vehiculos()
        {
            var vehiculo1 = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var vehiculo2 = new  Vehiculo("2222BBB", "AAA", "ElMejor", 3.3, Motor.Gasolina, "12345678Z");
            var vehiculo3 = new  Vehiculo("3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");
            var vehiculo4 = new  Vehiculo("4444BBB", "NKNN", "ElMejor", 3.3, Motor.Hidrogeno, "12345678Z");
            
            var result1 = _repositorio.Agregar(vehiculo1);
            var result2 = _repositorio.Agregar(vehiculo2);
            var result3 = _repositorio.Agregar(vehiculo3);
            var result4 = _repositorio.Agregar(vehiculo4);
            
            result1.Should().NotBeNull();
            result1.IsSuccess.Should().BeTrue();
            result1.Value.Id.Should().BeGreaterThan(0);
            
            result2.Should().NotBeNull();
            result2.IsSuccess.Should().BeTrue();
            result2.Value.Id.Should().BeGreaterThan(1);
            
            result3.Should().NotBeNull();
            result3.IsSuccess.Should().BeTrue();
            result3.Value.Id.Should().BeGreaterThan(2);
            
            result4.Should().NotBeNull();
            result4.IsFailure.Should().BeTrue();
            result4.Should().BeOfType<VehiculoError.OwnerWithThreeOrMoreVehiculos>();
        }
    }
}