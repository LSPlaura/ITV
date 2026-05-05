using FluentAssertions;
using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Error.Vehiculos;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Service;
using ITV.Storage.Common;
using ITV.Validador;
using Moq;

[TestFixture]
public class CitaServiceTests
{
    [TestFixture]
    public class CasosValidos
    {
        private Mock<IRepositorioVehiculos> _mockRepository = null!;
        private Mock<IBackUpService<Cita>> _mockBackUpService = null!;
        private Mock<IStorage<Cita>> _mockStorage = null!;
        private Mock<ICache<string, Cita>> _mockCacheLru = null!;
        private Mock<IValidate<Cita>> _mockValidador = null!;
        private IService<string, Cita> _service = null!;
        
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);
        
        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepositorioVehiculos>(); 
            _mockBackUpService = new Mock<IBackUpService<Cita>>();
            _mockStorage = new Mock<IStorage<Cita>>();
            _mockCacheLru = new Mock<ICache<string, Cita>>();
            _mockValidador = new Mock<IValidate<Cita>>();
            _service = new ServiceVehiculos(
                _mockRepository.Object,
                _mockBackUpService.Object,
                _mockStorage.Object,
                _mockCacheLru.Object,
                _mockValidador.Object
                );
        }
        
        [TearDown]
        public void TearDown()
        {
            _mockRepository = null!;
            _mockBackUpService = null!;
            _mockStorage = null!;
            _mockCacheLru = null!;
            _mockValidador = null!;
        }

        [Test]
        public void Agregar_VehiculoValido_DevuelveResultSuccess()
        {
           var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
           
           _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
               .Returns(Result.Success<bool, DomainError>(true));
           _mockRepository.Setup(r => r.Agregar(It.IsAny<Cita>()))
               .Returns((Cita v) => Result.Success<Cita, DomainError>(v));
           
           var resultado = _service.Agregar(vehiculo);
           
           resultado.IsSuccess.Should().BeTrue();
           _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
           _mockRepository.Verify(r => r.Agregar(It.IsAny<Cita>()), Times.Once);
        }
        
        [Test]
        public void Estandarizar_UpperCaseDni()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111bbB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z");
            
            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.Agregar(It.IsAny<Cita>()))
                .Returns((Cita v) => Result.Success<Cita, DomainError>(v));
            
            var resultado = _service.Agregar(vehiculo);
            
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.DniDueño.Should().BeUpperCased();
        }
        
        [Test]
        public void Estandarizar_ToCapitalice()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111bbB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z");
            
            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.Agregar(It.IsAny<Cita>()))
                .Returns((Cita v) => Result.Success<Cita, DomainError>(v));
            
            var resultado = _service.Agregar(vehiculo);
            
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Marca.Should().Be("Toyota");
            resultado.Value.Modelo.Should().Be("Malo");
        }
        
        [Test]
        public void Estandarizar_UpperCaseMatricula()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111bbB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z");
            
            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.Agregar(It.IsAny<Cita>()))
                .Returns((Cita v) => Result.Success<Cita, DomainError>(v));
            
            var resultado = _service.Agregar(vehiculo);
            
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Matricula.Should().BeUpperCased();
        }
        
        [Test]
        public void Borrar_RealizaBorrado()
        {
            var matricula = "1111BBB";
            var vehiculoExistente = new Cita(FechaMat, FechaInsp, matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockRepository.Setup(r => r.BuscarMatricula(matricula))
                .Returns(Result.Success<Cita, DomainError>(vehiculoExistente));

            _mockRepository.Setup(r => r.Borrar(It.IsAny<int>()))
                .Returns(Result.Success<Cita, DomainError>(vehiculoExistente));

            var result = _service.Borrar(matricula);
           
            result.IsSuccess.Should().BeTrue();
            
            _mockRepository.Verify(r => r.BuscarMatricula(matricula), Times.Once);
            _mockRepository.Verify(r => r.Borrar(It.IsAny<int>()), Times.Once);
            _mockCacheLru.Verify(c => c.Borrar(matricula), Times.Once); 
        }
        
        [Test]
        public void GetById_EstaCacheado_NoLlegaAlRepositorio()
        {
            var matricula = "1111BBB";
            var vehiculoExistente = new Cita(FechaMat, FechaInsp, matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockCacheLru.Setup(r => r.Obtener(matricula))
                .Returns(vehiculoExistente);
            
            var result = _service.GetById(matricula);
           
            result.IsSuccess.Should().BeTrue();
            
            _mockCacheLru.Verify(r => r.Obtener(matricula), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(It.IsAny<string>()), Times.Never);
            _mockCacheLru.Verify(c => c.Agregar(matricula, vehiculoExistente), Times.Never); 
        }
        
        [Test]
        public void GetById_NoEstaCacheado_AccedeAlRepositorio()
        {
            var matricula = "1111BBB";
            var vehiculoExistente = new Cita(FechaMat, FechaInsp, matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockCacheLru.Setup(r => r.Obtener(matricula))
                .Returns((Cita)null!);

            _mockRepository.Setup(r => r.BuscarMatricula(It.IsAny<string>()))
                .Returns(Result.Success<Cita, DomainError>(vehiculoExistente));

            var result = _service.GetById(matricula);
            result.IsSuccess.Should().BeTrue();
            
            _mockCacheLru.Verify(r => r.Obtener(matricula), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(It.IsAny<string>()), Times.Once);
            _mockCacheLru.Verify(c => c.Agregar(matricula, vehiculoExistente), Times.Once); 
        }
        
        [Test]
        public void Actualizar_DebeFuncionarCorrectamente()
        {
            var matricula = "1111BBB";
            var vehiculoAntiguo = new Cita(FechaMat, FechaInsp, matricula, "Toyota", "Horrible", 1.0, Motor.Diesel, "12345678Z") { Id = 50 }; 
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp, matricula, "toyota", "malo", 1.0, Motor.Diesel, "12345678z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistMatricula(matricula)).Returns(true);
            _mockRepository.Setup(r => r.BuscarMatricula(matricula))
                .Returns(Result.Success<Cita, DomainError>(vehiculoAntiguo));
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<int>(), It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(vehiculoNuevo));

            var resultado = _service.Actualizar(matricula, vehiculoNuevo);
            
            resultado.IsSuccess.Should().BeTrue();
            
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.ExistMatricula(matricula), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(matricula), Times.Once);
            _mockRepository.Verify(r => r.Actualizar(50, It.IsAny<Cita>()), Times.Once);
            _mockCacheLru.Verify(c => c.Borrar(matricula), Times.Once); 
        }

        [Test]
        public void GetAll_ObtieneListaPaginada()
        {
            List<Cita> lista = [
                new Cita(FechaMat, FechaInsp, "1111AAA", "Exito", "...", 1000, Motor.Gasolina, "123z"),
                new Cita(FechaMat, FechaInsp, "2222BBB", "Error", "...", 1500, Motor.Diesel, "456x"),
                new Cita(FechaMat, FechaInsp, "3333CCC", "NoLlega", "...", 1200, Motor.Gasolina, "789y")
            ];
            _mockRepository.Setup(r => r.GetAll()).Returns(lista);
            var result = _service.GetAll(1, 1);
            result.Should().NotBeNull();
            
            result.Should().HaveCount(1);
            result.Should().Contain(lista[1]);
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
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
            var lista = new List<Cita>()
            {
                new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z"),
                new Cita(FechaMat, FechaInsp, "9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T")
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
        private Mock<IRepositorioVehiculos> _mockRepository = null!;
        private Mock<IBackUpService<Cita>> _mockBackUpService = null!;
        private Mock<IStorage<Cita>> _mockStorage = null!;
        private Mock<ICache<string, Cita>> _mockCacheLru = null!;
        private Mock<IValidate<Cita>> _mockValidador = null!;
        private IService<string, Cita> _service = null!;

        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepositorioVehiculos>();
            _mockBackUpService = new Mock<IBackUpService<Cita>>();
            _mockStorage = new Mock<IStorage<Cita>>();
            _mockCacheLru = new Mock<ICache<string, Cita>>();
            _mockValidador = new Mock<IValidate<Cita>>();
            _service = new ServiceVehiculos(
                _mockRepository.Object,
                _mockBackUpService.Object,
                _mockStorage.Object,
                _mockCacheLru.Object,
                _mockValidador.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository = null!;
            _mockBackUpService = null!;
            _mockStorage = null!;
            _mockCacheLru = null!;
            _mockValidador = null!;
        }

        [Test]
        public void Agregar_VehiculoInvalido_DevuelveFailure()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "11BBB", "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Failure<bool, DomainError>(new CitaError.ValidationError.ValidationMatricula(vehiculo.Matricula)));
            
            var resultado = _service.Agregar(vehiculo);

            resultado.IsFailure.Should().BeTrue();
            
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.Agregar(It.IsAny<Cita>()), Times.Never);
        }
        
        [Test]
        public void Actualizar_VehiculoNuevoIncorrecto_DevuelveFailure()
        {
            var matricula = "1111BBB";
            var vehiculoAntiguo = new Cita(FechaMat, FechaInsp, matricula, "Toyota", "Horrible", 1.0, Motor.Diesel, "12345678Z") { Id = 50 };
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp, matricula, "toyota", "malo", -1.0, Motor.Diesel, "12345678z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Failure<bool, DomainError>(new CitaError.ValidationError.ValidationCilindrica(vehiculoNuevo.Cilindrada)));
            
            var resultado = _service.Actualizar(matricula, vehiculoNuevo);

            resultado.IsFailure.Should().BeTrue();

            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(matricula), Times.Never);
            _mockRepository.Verify(r => r.Actualizar(50, It.IsAny<Cita>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(matricula), Times.Never);
        }
        
        [Test]
        public void Actualizar_MatriculaAntiguaNuevaDistintas_DevuelveFailure()
        {
            var vehiculoAntiguo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "Horrible", 1.0, Motor.Diesel, "12345678Z") { Id = 50 };
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp, "1111CcC", "toyota", "malo", -1.0, Motor.Diesel, "12345678z");
            
            var resultado = _service.Actualizar(vehiculoAntiguo.Matricula, vehiculoNuevo);

            resultado.IsFailure.Should().BeTrue();

            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Never);
            _mockRepository.Verify(r => r.BuscarMatricula(vehiculoAntiguo.Matricula), Times.Never);
            _mockRepository.Verify(r => r.Actualizar(50, It.IsAny<Cita>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(vehiculoAntiguo.Matricula), Times.Never);
        }
        
        [Test]
        public void Actualizar_SiNoEncuentraMatricula_DevuelveFailure()
        {
            var matricula = "1111BBB";
            var vehiculoAntiguo = new Cita(FechaMat, FechaInsp, matricula, "Toyota", "Horrible", 1.0, Motor.Diesel, "12345678Z") { Id = 50 }; 
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp, matricula, "toyota", "malo", 1.0, Motor.Diesel, "12345678z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistMatricula(matricula)).Returns(false);
            _mockRepository.Setup(r => r.BuscarMatricula(matricula))
                .Returns(Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundMatricula(matricula)));

            var resultado = _service.Actualizar(matricula, vehiculoNuevo);
            
            resultado.IsFailure.Should().BeTrue();
            
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(matricula), Times.Never);
            _mockRepository.Verify(r => r.Actualizar(50, It.IsAny<Cita>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(matricula), Times.Never); 
        }
        
        [Test]
        public void Borrar_NoEncuentraMatricula_DevuelveFailure()
        {
            var matricula = "1111BBB";
            var vehiculoExistente = new Cita(FechaMat, FechaInsp, matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockRepository.Setup(r => r.BuscarMatricula(matricula))
                .Returns(Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundMatricula(matricula)));

            var result = _service.Borrar(matricula);
           
            result.IsFailure.Should().BeTrue();
            
            _mockRepository.Verify(r => r.BuscarMatricula(matricula), Times.Once);
            _mockRepository.Verify(r => r.Borrar(It.IsAny<int>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(matricula), Times.Never); 
        }
        
        [Test]
        public void GetById_NoEstaRepositorio_DevuelveFailure()
        {
            var matricula = "1111BBB";
            var vehiculoExistente = new Cita(FechaMat, FechaInsp, matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockCacheLru.Setup(r => r.Obtener(matricula))
                .Returns((Cita)null!);

            _mockRepository.Setup(r => r.BuscarMatricula(It.IsAny<string>()))
                .Returns(Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundMatricula(matricula)));

            var result = _service.GetById(matricula);
            result.IsFailure.Should().BeTrue();
            
            _mockCacheLru.Verify(r => r.Obtener(matricula), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(It.IsAny<string>()), Times.Once);
            _mockCacheLru.Verify(c => c.Agregar(matricula, vehiculoExistente), Times.Never); 
        }
        
        [Test]
        public void GetAll_PaginacionFueraDeRango_ListaVacia()
        {
            List<Cita> lista = [
                new Cita(FechaMat, FechaInsp, "1111AAA", "Exito", "...", 1000, Motor.Gasolina, "123z"),
                new Cita(FechaMat, FechaInsp, "2222BBB", "Error", "...", 1500, Motor.Diesel, "456x"),
                new Cita(FechaMat, FechaInsp, "3333CCC", "NoLlega", "...", 1200, Motor.Gasolina, "789y")
            ];
            _mockRepository.Setup(r => r.GetAll()).Returns(lista);
            var result = _service.GetAll(3, 1);
            result.Should().NotBeNull();
            
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Test]
        public void Restaurar_UnVehiculoDevuelveFailureAlAgregarse_DevuelveFailure()
        {
            var vehiculos = new List<Cita> { 
                new Cita(FechaMat, FechaInsp, "1111AAA", "Exito", "...", 1000, Motor.Gasolina, "123z"),
                new Cita(FechaMat, FechaInsp, "2222BBB", "Error", "...", 1500, Motor.Diesel, "456x"),
                new Cita(FechaMat, FechaInsp, "3333CCC", "NoLlega", "...", 1200, Motor.Gasolina, "789y")
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