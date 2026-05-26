using CSharpFunctionalExtensions;
using FluentAssertions;
using ITV.Error.Citas;
using ITV.Error.Common;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Service;
using ITV.Service.DataService; // Asumiendo que aquí reside tu interfaz/clase de DataService
using Moq;

namespace ITV.Test.Service;

[TestFixture]
public class DataServiceTest
{
    [TestFixture]
    public class CasosValidos
    {
        private static readonly DateTime Now = DateTime.UtcNow;
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        private Mock<IRepositorioCitas> _mockRepository = null!;
        private Mock<IBackUpService<Cita>> _mockBackUpService = null!; 
        private IDataService<Cita> _service = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepositorioCitas>();
            _mockBackUpService = new Mock<IBackUpService<Cita>>();
            
            _service = new DataService(
                _mockBackUpService.Object,
                _mockRepository.Object
            );
        }

        [Test]
        public void Guardar_DevuelveRuta()
        {
            var lista = new List<Cita>();
            _mockBackUpService.Setup(bs => bs.Guardar(lista)).Returns("RutaTest.zip");

            var result = _service.GuardarBuckUp();

            result.IsSuccess.Should().BeTrue();
            _mockBackUpService.Verify(bs => bs.Guardar(lista), Times.Once);
        }

        [Test]
        public void Restaurar_DevuelveCantidadVehiculos()
        {
            var lista = new List<Cita>
            {
                new Cita(101, FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z", false, Now, Now),
                new Cita(102, FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T", false, Now, Now)
            };
            var path = "RutaTest.zip";
            
            _mockBackUpService.Setup(bs => bs.Restuarar(path))
                .Returns(Result.Success<IEnumerable<Cita>, DomainError>(lista));

            var result = _service.RestaurarBuckUp(path);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(2);

            _mockBackUpService.Verify(bs => bs.Restuarar(path), Times.Once);
            _mockRepository.Verify(r => r.DeleteAll(), Times.Once);
        }
    }
    [TestFixture]
    public class CasosInvalidos
    {
        private static readonly DateTime Now = DateTime.UtcNow;
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        private Mock<IRepositorioCitas> _mockRepository = null!;
        private Mock<IBackUpService<Cita>> _mockBackUpService = null!; 
        private IDataService<Cita> _service = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepositorioCitas>();
            _mockBackUpService = new Mock<IBackUpService<Cita>>();
            
            _service = new DataService(
                _mockBackUpService.Object,
                _mockRepository.Object
            );
        }

        [Test]
        public void Restaurar_UnVehiculoDevuelveFailureAlAgregarse_DevuelveFailure()
        {
            var vehiculos = new List<Cita>
            {
                new Cita(101, FechaMat, FechaInsp, "A", "Exito", "...", 1000, Motor.Gasolina, "A", false, Now, Now),
                new Cita(102, FechaMat, FechaInsp, "B", "Error", "...", 1500, Motor.Diesel, "B", false, Now, Now),
                new Cita(103, FechaMat, FechaInsp, "C", "NoLlega", "...", 1200, Motor.Gasolina, "C", false, Now, Now)
            };
            var path = "RutaTest.zip";
            
            _mockBackUpService.Setup(bs => bs.Restuarar(path))
                .Returns(Result.Success<IEnumerable<Cita>, DomainError>(vehiculos));
                
            _mockRepository.Setup(r => r.Agregar(vehiculos[0]))
                .Returns(Result.Success<Cita, DomainError>(vehiculos[0]));
                
            _mockRepository.Setup(r => r.Agregar(vehiculos[1]))
                .Returns(Result.Failure<Cita, DomainError>(new CitaError.CitaAlredyExist.IdAlreadyExists(vehiculos[1].Id)));

            var result = _service.RestaurarBuckUp(path);

            result.IsFailure.Should().BeTrue();
            _mockRepository.Verify(r => r.Agregar(It.IsAny<Cita>()), Times.Exactly(2));
        }
    }
}