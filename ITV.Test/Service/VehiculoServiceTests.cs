using FluentAssertions;
using ITV.Error.Common;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Service;
using ITV.Storage.Common;
using ITV.Validador;
using CSharpFunctionalExtensions;
using ITV.Error.Vehiculos;
using Moq;

namespace ITV.Test.Service;

[TestFixture]
public class VehiculoServiceTests
{

    [TestFixture]
    public class CasosValidos
    {
        private Mock<IRepositorioVehiculos> _mockRepository = null!;
        private Mock<IBackUpService<Vehiculo>>_mockBackUpService = null!;
        private Mock<IStorage<Vehiculo>> _mockStorage = null!;
        private Mock<ICache<string, Vehiculo>> _mockCacheLru = null!;
        private Mock<IValidate<Vehiculo>> _mockValidador = null!;
        private IService<string, Vehiculo> _service = null!;
        
        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepositorioVehiculos>(); 
            _mockBackUpService = new Mock<IBackUpService<Vehiculo>>();
            _mockStorage = new Mock<IStorage<Vehiculo>>();
            _mockCacheLru = new Mock<ICache<string, Vehiculo>>();
            _mockValidador = new Mock<IValidate<Vehiculo>>();
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
           var vehiculo = new Vehiculo("1111BBB", "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
           
           _mockValidador.Setup(v => v.Validar(It.IsAny<Vehiculo>()))
               .Returns(Result.Success<bool, DomainError>(true));
           _mockRepository.Setup(r => r.Agregar(It.IsAny<Vehiculo>()))
               .Returns((Vehiculo v) => Result.Success<Vehiculo, DomainError>(v));
           
           var resultado = _service.Agregar(vehiculo);
           
           resultado.IsSuccess.Should().BeTrue();
           _mockValidador.Verify(v => v.Validar(It.IsAny<Vehiculo>()), Times.Once);
           _mockRepository.Verify(r => r.Agregar(It.IsAny<Vehiculo>()), Times.Once);
        }
        
        
        [Test]
        public void Estandarizar_UpperCaseDni()
        {
            var vehiculo = new Vehiculo("1111bbB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z");
            
            _mockValidador.Setup(v => v.Validar(It.IsAny<Vehiculo>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.Agregar(It.IsAny<Vehiculo>()))
                .Returns((Vehiculo v) => Result.Success<Vehiculo, DomainError>(v));
            
            var resultado = _service.Agregar(vehiculo);
            
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.DniDueño.Should().BeUpperCased();
        }
        
        [Test]
        public void Estandarizar_ToCapitalice()
        {
            var vehiculo = new Vehiculo("1111bbB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z");
            
            _mockValidador.Setup(v => v.Validar(It.IsAny<Vehiculo>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.Agregar(It.IsAny<Vehiculo>()))
                .Returns((Vehiculo v) => Result.Success<Vehiculo, DomainError>(v));
            
            var resultado = _service.Agregar(vehiculo);
            
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Marca.Should().Be("Toyota");
            resultado.Value.Modelo.Should().Be("Malo");
        }
        
        [Test]
        public void Estandarizar_UpperCaseMatricula()
        {
            var vehiculo = new Vehiculo("1111bbB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z");
            
            _mockValidador.Setup(v => v.Validar(It.IsAny<Vehiculo>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.Agregar(It.IsAny<Vehiculo>()))
                .Returns((Vehiculo v) => Result.Success<Vehiculo, DomainError>(v));
            
            
            var resultado = _service.Agregar(vehiculo);
            
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Matricula.Should().BeUpperCased();
        }
        
        [Test]
        public void Borrar_RealizaBorrado()
        {
            var matricula = "1111BBB";
            var vehiculoExistente = new Vehiculo(matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockRepository.Setup(r => r.BuscarMatricula(matricula))
                .Returns(Result.Success<Vehiculo, DomainError>(vehiculoExistente));

            _mockRepository.Setup(r => r.Borrar(It.IsAny<int>()))
                .Returns(Result.Success<Vehiculo, DomainError>(vehiculoExistente));

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
            var vehiculoExistente = new Vehiculo(matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
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
            var vehiculoExistente = new Vehiculo(matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockCacheLru.Setup(r => r.Obtener(matricula))
                .Returns((Vehiculo)null!);

            _mockRepository.Setup(r => r.BuscarMatricula(It.IsAny<string>()))
                .Returns(Result.Success<Vehiculo, DomainError>(vehiculoExistente));

            var result = _service.GetById(matricula);
            Console.WriteLine(result.Value);
            result.IsSuccess.Should().BeTrue();
            
            _mockCacheLru.Verify(r => r.Obtener(matricula), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(It.IsAny<string>()), Times.Once);
            _mockCacheLru.Verify(c => c.Agregar(matricula, vehiculoExistente), Times.Once); 
        }
        
        [Test]
        public void Actualizar_DebeFuncionarCorrectamente()
        {
            var matricula = "1111BBB";
            var vehiculoAntiguo = new Vehiculo(matricula, "Toyota", "Horrible", 1.0, Motor.Diesel, "12345678Z") { Id = 50 }; 
            var vehiculoNuevo = new Vehiculo(matricula, "toyota", "malo", 1.0, Motor.Diesel, "12345678z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Vehiculo>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistMatricula(matricula)).Returns(true);
            _mockRepository.Setup(r => r.BuscarMatricula(matricula))
                .Returns(Result.Success<Vehiculo, DomainError>(vehiculoAntiguo));
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<int>(), It.IsAny<Vehiculo>()))
                .Returns(Result.Success<Vehiculo, DomainError>(vehiculoNuevo));

            var resultado = _service.Actualizar(matricula, vehiculoNuevo);
            
            resultado.IsSuccess.Should().BeTrue();
            
            _mockValidador.Verify(v => v.Validar(It.IsAny<Vehiculo>()), Times.Once);
            _mockRepository.Verify(r => r.ExistMatricula(matricula), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(matricula), Times.Once);
            _mockRepository.Verify(r => r.Actualizar(50, It.IsAny<Vehiculo>()), Times.Once);
            _mockCacheLru.Verify(c => c.Borrar(matricula), Times.Once); 
        }

        [Test]
        public void GetAll_ObtieneLista()
        {
            var lista = new List<Vehiculo>();
            _mockRepository.Setup(r => r.GetAll()).Returns(lista);
            var result = _service.GetAll();
            result.Should().NotBeNull();
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }
        
        [Test]
        public void Guardar_DevuelveRuta()
        {
            var lista = new List<Vehiculo>();
            _mockBackUpService.Setup(bs => bs.Guardar(lista)).Returns("RutaTest.zip");
            
            var result = _service.GuardarBuckUp();
            result.IsSuccess.Should().BeTrue();
            
            _mockBackUpService.Verify(bs => bs.Guardar(lista), Times.Once);
        }
        
        [Test]
        public void Restaurar_DevuelveCantidadVehiculos()
        {
            var lista = new List<Vehiculo>()
            {
                new Vehiculo("1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z"),
                new Vehiculo("9876FGH", "Aston Martin", "Vantage", 4000.0, Motor.Gasolina, "00000000T")
            };
            var path = "RutaTest.zip";
            _mockBackUpService.Setup(bs => bs.Restuarar(path))
                .Returns(Result.Success<IEnumerable<Vehiculo>, DomainError>(lista));
            
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
        private Mock<IBackUpService<Vehiculo>> _mockBackUpService = null!;
        private Mock<IStorage<Vehiculo>> _mockStorage = null!;
        private Mock<ICache<string, Vehiculo>> _mockCacheLru = null!;
        private Mock<IValidate<Vehiculo>> _mockValidador = null!;
        private IService<string, Vehiculo> _service = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepositorioVehiculos>();
            _mockBackUpService = new Mock<IBackUpService<Vehiculo>>();
            _mockStorage = new Mock<IStorage<Vehiculo>>();
            _mockCacheLru = new Mock<ICache<string, Vehiculo>>();
            _mockValidador = new Mock<IValidate<Vehiculo>>();
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
            var vehiculo = new Vehiculo("11BBB", "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Vehiculo>()))
                .Returns(Result.Failure<bool, DomainError>(new VehiculoError.ValidationError.ValidationMatricula(vehiculo.Matricula)));
            
            var resultado = _service.Agregar(vehiculo);

            resultado.IsFailure.Should().BeTrue();
            
            _mockValidador.Verify(v => v.Validar(It.IsAny<Vehiculo>()), Times.Once);
            _mockRepository.Verify(r => r.Agregar(It.IsAny<Vehiculo>()), Times.Never);
        }
        
        [Test]
        public void Actualizar_VehiculoNuevoIncorrecto_DevuelveFailure()
        {
            var matricula = "1111BBB";
            var vehiculoAntiguo = new Vehiculo(matricula, "Toyota", "Horrible", 1.0, Motor.Diesel, "12345678Z")
                { Id = 50 };
            var vehiculoNuevo = new Vehiculo(matricula, "toyota", "malo", -1.0, Motor.Diesel, "12345678z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Vehiculo>()))
                .Returns(Result.Failure<bool, DomainError>(new VehiculoError.ValidationError.ValidationCilindrica(vehiculoNuevo.Cilindrada)));
            
            var resultado = _service.Actualizar(matricula, vehiculoNuevo);

            resultado.IsFailure.Should().BeTrue();

            _mockValidador.Verify(v => v.Validar(It.IsAny<Vehiculo>()), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(matricula), Times.Never);
            _mockRepository.Verify(r => r.Actualizar(50, It.IsAny<Vehiculo>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(matricula), Times.Never);
        }
        
        [Test]
        public void Actualizar_MatriculaAntiguaNuevaDistintas_DevuelveFailure()
        {
            var vehiculoAntiguo = new Vehiculo("1111BBB", "Toyota", "Horrible", 1.0, Motor.Diesel, "12345678Z")
                { Id = 50 };
            var vehiculoNuevo = new Vehiculo("1111CcC", "toyota", "malo", -1.0, Motor.Diesel, "12345678z");
            

            var resultado = _service.Actualizar(vehiculoAntiguo.Matricula, vehiculoNuevo);

            resultado.IsFailure.Should().BeTrue();

            _mockValidador.Verify(v => v.Validar(It.IsAny<Vehiculo>()), Times.Never);
            _mockRepository.Verify(r => r.BuscarMatricula(vehiculoAntiguo.Matricula), Times.Never);
            _mockRepository.Verify(r => r.Actualizar(50, It.IsAny<Vehiculo>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(vehiculoAntiguo.Matricula), Times.Never);
        }
        
        [Test]
        public void Actualizar_SiNoEncuentraMatricula_DevuelveFailure()
        {
            var matricula = "1111BBB";
            var vehiculoAntiguo = new Vehiculo(matricula, "Toyota", "Horrible", 1.0, Motor.Diesel, "12345678Z") { Id = 50 }; 
            var vehiculoNuevo = new Vehiculo(matricula, "toyota", "malo", 1.0, Motor.Diesel, "12345678z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Vehiculo>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistMatricula(matricula)).Returns(false);
            _mockRepository.Setup(r => r.BuscarMatricula(matricula))
                .Returns(Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundMatricula(matricula)));

            var resultado = _service.Actualizar(matricula, vehiculoNuevo);
            
            resultado.IsFailure.Should().BeTrue();
            
            _mockValidador.Verify(v => v.Validar(It.IsAny<Vehiculo>()), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(matricula), Times.Never);
            _mockRepository.Verify(r => r.Actualizar(50, It.IsAny<Vehiculo>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(matricula), Times.Never); 
        }
        
        [Test]
        public void Borrar_NoEncuentraMatricula_DevuelveFailure()
        {
            var matricula = "1111BBB";
            var vehiculoExistente = new Vehiculo(matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockRepository.Setup(r => r.BuscarMatricula(matricula))
                .Returns(Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundMatricula(matricula)));

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
            var vehiculoExistente = new Vehiculo(matricula, "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z");
    
            _mockCacheLru.Setup(r => r.Obtener(matricula))
                .Returns((Vehiculo)null!);

            _mockRepository.Setup(r => r.BuscarMatricula(It.IsAny<string>()))
                .Returns(Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoNotFoundMatricula(matricula)));

            var result = _service.GetById(matricula);
            result.IsFailure.Should().BeTrue();
            
            _mockCacheLru.Verify(r => r.Obtener(matricula), Times.Once);
            _mockRepository.Verify(r => r.BuscarMatricula(It.IsAny<string>()), Times.Once);
            _mockCacheLru.Verify(c => c.Agregar(matricula, vehiculoExistente), Times.Never); 
        }

        [Test]
        public void Restaurar_UnVehiculoDevuelveFailureAlAgregarse_DevuelveFailure()
        {
            var vehiculos = new List<Vehiculo> { 
                new Vehiculo("1111AAA", "Exito", "...", 1000, Motor.Gasolina, "123z"),
                new Vehiculo("2222BBB", "Error", "...", 1500, Motor.Diesel, "456x"),
                new Vehiculo("3333CCC", "NoLlega", "...", 1200, Motor.Gasolina, "789y")
            };
            var path = "RutaTest.zip";
            
            _mockBackUpService.Setup(bs => bs.Restuarar(path))
                .Returns(Result.Success<IEnumerable<Vehiculo>, DomainError>(vehiculos));
            
            _mockRepository.Setup(r => r.Agregar(vehiculos[0]))
                .Returns(Result.Success<Vehiculo, DomainError>(vehiculos[0]));
            _mockRepository.Setup(r => r.Agregar(vehiculos[1]))
                .Returns(Result.Failure<Vehiculo, DomainError>(new VehiculoError.VehiculoAlredyExist.IdAlreadyExists(vehiculos[1].Id)));
            
            var result = _service.RestaurarBuckUp(path);
            
            result.IsFailure.Should().BeTrue();
            _mockRepository.Verify(r => r.Agregar(It.IsAny<Vehiculo>()), Times.Exactly(2)); 
        }
        
    }
}