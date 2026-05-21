using CSharpFunctionalExtensions;
using FluentAssertions;
using ITV.Error.Common;
using ITV.Error.Vehiculos;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Service.Citas;
using ITV.Validador;
using Moq;

namespace ITV.Test.Service;

[TestFixture]
public class CitaServiceTests
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
    private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

    [TestFixture]
    public class CasosValidos
    {
        private Mock<IRepositorioCita> _mockRepository = null!;
        private Mock<ICache<int, Cita>> _mockCacheLru = null!;
        private Mock<IValidate<Cita>> _mockValidador = null!;
        private IService<int, Cita> _service = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepositorioCita>();
            _mockCacheLru = new Mock<ICache<int, Cita>>();
            _mockValidador = new Mock<IValidate<Cita>>();
            _service = new ServiceVehiculos(
                _mockRepository.Object,
                _mockCacheLru.Object,
                _mockValidador.Object);
        }

        [Test]
        public void Agregar_VehiculoValido_DevuelveResultSuccess()
        {
            var vehiculo = new Cita(
                FechaMat, FechaInsp, "1111BBB", "Toyota", "Malo", 1.0, Motor.Diesel, "12345678Z"
            );

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.Agregar(It.IsAny<Cita>()))
                .Returns((Cita v) => Result.Success<Cita, DomainError>(
                    new Cita(1, v.FechaMatriculacion, v.FechaInspeccion, v.Matricula, v.Marca, v.Modelo, v.Cilindrada,
                        v.Motor, v.DniDueño, false, Now, Now)
                ));

            var resultado = _service.Agregar(vehiculo);

            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Id.Should().Be(1);
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
                .Returns((Cita v) => Result.Success<Cita, DomainError>(
                    new Cita(2, v.FechaMatriculacion, v.FechaInspeccion, v.Matricula, v.Marca, v.Modelo, v.Cilindrada,
                        v.Motor, v.DniDueño.ToUpper(), false, Now, Now)
                ));

            var resultado = _service.Agregar(vehiculo);

            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.DniDueño.Should().Be(vehiculo.DniDueño.ToUpper());
        }

        [Test]
        public void Estandarizar_ToCapitalice()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111bbB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.Agregar(It.IsAny<Cita>()))
                .Returns((Cita v) => Result.Success<Cita, DomainError>(
                    new Cita(3, v.FechaMatriculacion, v.FechaInspeccion, v.Matricula,
                        "Toyota", "Malo", v.Cilindrada, v.Motor, v.DniDueño, false, Now, Now)
                ));

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
                .Returns((Cita v) => Result.Success<Cita, DomainError>(
                    new Cita(4, v.FechaMatriculacion, v.FechaInspeccion, v.Matricula.ToUpper(), v.Marca, v.Modelo,
                        v.Cilindrada, v.Motor, v.DniDueño, false, Now, Now)
                ));

            var resultado = _service.Agregar(vehiculo);

            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Matricula.Should().Be(vehiculo.Matricula.ToUpper());
        }

        [Test]
        public void Borrar_RealizaBorrado()
        {
            var idVehiculo = 5;
            var vehiculoExistente = new Cita(idVehiculo, FechaMat, FechaInsp, "1111BBB", "Toyota", "Malo", 1.0,
                Motor.Diesel, "12345678Z", false, Now, Now);

            _mockRepository.Setup(r => r.BuscarId(idVehiculo))
                .Returns(Result.Success<Cita, DomainError>(vehiculoExistente));
            _mockRepository.Setup(r => r.Borrar(idVehiculo))
                .Returns(Result.Success<Cita, DomainError>(vehiculoExistente));

            var result = _service.Borrar(idVehiculo);

            result.IsSuccess.Should().BeTrue();

            _mockRepository.Verify(r => r.BuscarId(idVehiculo), Times.Once);
            _mockRepository.Verify(r => r.Borrar(idVehiculo), Times.Once);
            _mockCacheLru.Verify(c => c.Borrar(idVehiculo), Times.Once);
        }

        [Test]
        public void GetById_EstaCacheado_NoLlegaAlRepositorio()
        {
            var idVehiculo = 6;
            var vehiculoExistente = new Cita(idVehiculo, FechaMat, FechaInsp, "2222BBB", "Renault", "Clio", 1.2,
                Motor.Gasolina, "99887766A", false, Now, Now);

            _mockCacheLru.Setup(r => r.Obtener(idVehiculo))
                .Returns(vehiculoExistente);

            var result = _service.GetById(idVehiculo);

            result.IsSuccess.Should().BeTrue();

            _mockCacheLru.Verify(r => r.Obtener(idVehiculo), Times.Once);
            _mockRepository.Verify(r => r.BuscarId(It.IsAny<int>()), Times.Never);
            _mockCacheLru.Verify(c => c.Agregar(idVehiculo, vehiculoExistente), Times.Never);
        }

        [Test]
        public void GetById_NoEstaCacheado_AccedeAlRepositorio()
        {
            var idVehiculo = 7;
            var vehiculoExistente = new Cita(idVehiculo, FechaMat, FechaInsp, "3333CCC", "Seat", "Ibiza", 1.4,
                Motor.Gasolina, "11223344B", false, Now, Now);

            _mockCacheLru.Setup(r => r.Obtener(idVehiculo))
                .Returns((Cita)null!);
            _mockRepository.Setup(r => r.BuscarId(idVehiculo))
                .Returns(Result.Success<Cita, DomainError>(vehiculoExistente));

            var result = _service.GetById(idVehiculo);

            result.IsSuccess.Should().BeTrue();

            _mockCacheLru.Verify(r => r.Obtener(idVehiculo), Times.Once);
            _mockRepository.Verify(r => r.BuscarId(idVehiculo), Times.Once);
            _mockCacheLru.Verify(c => c.Agregar(idVehiculo, vehiculoExistente), Times.Once);
        }

        [Test]
        public void Actualizar_DebeFuncionarCorrectamente()
        {
            var idVehiculo = 50;
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp.AddDays(1), "4444DDD", "CITROEN", "Saxo", 1.0,
                Motor.Diesel, "55443322X");

            var lista = new List<Cita>()
            {
                new Cita(idVehiculo, FechaMat, FechaInsp, "4444DDD", "Citroen", "Saxo", 1.0, Motor.Diesel, "55443322X",
                    false, Now, Now)
            };

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(idVehiculo)).Returns(true);
            _mockRepository.Setup(r => r.GetAll()).Returns(lista.AsEnumerable);
            _mockRepository.Setup(r => r.Actualizar(idVehiculo, It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(vehiculoNuevo));

            var result = _service.Actualizar(idVehiculo, vehiculoNuevo);

            result.IsSuccess.Should().BeTrue();
            result.Value.Marca.Should().Be("CITROEN");

            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.ExistId(idVehiculo), Times.Once);
            _mockRepository.Verify(r => r.GetAll(), Times.AtLeastOnce);
            _mockRepository.Verify(r => r.Actualizar(idVehiculo, It.IsAny<Cita>()), Times.Once);
            _mockCacheLru.Verify(c => c.Borrar(idVehiculo), Times.Once);
        }

        [Test]
        public void Actualizar_MismoVehiculoMismaFecha_IsSuccess()
        {
            var idVehiculo = 50;
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp, "4444DDD", "CITROEN", "Saxo", 1.0, Motor.Diesel,
                "55443322X");

            var lista = new List<Cita>()
            {
                new Cita(idVehiculo, FechaMat, FechaInsp, "4444DDD", "Citroen", "Saxo", 1.0, Motor.Diesel, "55443322X",
                    false, Now, Now)
            };

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(idVehiculo)).Returns(true);
            _mockRepository.Setup(r => r.GetAll()).Returns(lista.AsEnumerable);
            _mockRepository.Setup(r => r.Actualizar(idVehiculo, It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(vehiculoNuevo));

            var result = _service.Actualizar(idVehiculo, vehiculoNuevo);

            result.IsSuccess.Should().BeTrue();
            result.Value.Marca.Should().Be("CITROEN");

            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.ExistId(idVehiculo), Times.Once);
            _mockRepository.Verify(r => r.GetAll(), Times.AtLeastOnce);
            _mockRepository.Verify(r => r.Actualizar(idVehiculo, It.IsAny<Cita>()), Times.Once);
            _mockCacheLru.Verify(c => c.Borrar(idVehiculo), Times.Once);
        }

        [Test]
        public void Actualizar_UnaCitaDeUnDniCon3CitasMismoDia_IsSucess()
        {
            var idVehiculo = 31;
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp, "1111AAA", "Guay", "Rojo", 1000, Motor.Electrico, "123z");
            var vehiculoActualizado =
                new Cita(FechaMat, FechaInsp, "1111AAA", "Guay", "Rojo", 1000, Motor.Electrico, "123z")
                    { Id = idVehiculo };
            List<Cita> lista = new()
            {
                new Cita(31, FechaMat, FechaInsp, "1111AAA", "Exito", "...", 1000, Motor.Gasolina, "123z", false, Now,
                    Now),
                new Cita(32, FechaMat, FechaInsp, "2222BBB", "Error", "...", 1500, Motor.Diesel, "456x", false, Now,
                    Now),
                new Cita(33, FechaMat, FechaInsp, "3333CCC", "NoLlega", "...", 1200, Motor.Gasolina, "789y", false, Now,
                    Now)
            };

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(idVehiculo)).Returns(true);
            _mockRepository.Setup(r => r.GetAll()).Returns(lista.AsEnumerable);
            _mockRepository.Setup(r => r.Actualizar(idVehiculo, It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(vehiculoActualizado));

            var result = _service.Actualizar(idVehiculo, vehiculoNuevo);

            result.IsSuccess.Should().BeTrue();

            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.ExistId(idVehiculo), Times.Once);
            _mockRepository.Verify(r => r.GetAll(), Times.AtLeastOnce);
            _mockRepository.Verify(r => r.Actualizar(idVehiculo, It.IsAny<Cita>()), Times.Once);
            _mockCacheLru.Verify(c => c.Borrar(idVehiculo), Times.Once);
        }

        [Test]
        public void GetAll_ObtieneListaPaginada()
        {
            List<Cita> lista = new()
            {
                new Cita(31, FechaMat, FechaInsp, "1111AAA", "Exito", "...", 1000, Motor.Gasolina, "123z", false, Now,
                    Now),
                new Cita(32, FechaMat, FechaInsp, "2222BBB", "Error", "...", 1500, Motor.Diesel, "456x", false, Now,
                    Now),
                new Cita(33, FechaMat, FechaInsp, "3333CCC", "NoLlega", "...", 1200, Motor.Gasolina, "789y", false, Now,
                    Now)
            };
            _mockRepository.Setup(r => r.GetAll()).Returns(lista);
            var result = _service.GetAll(1, 1);
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().Contain(lista[1]);
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }
    }

    [TestFixture]
    public class CasosInvalidos
    {
        private Mock<IRepositorioCita> _mockRepository = null!;
        private Mock<ICache<int, Cita>> _mockCacheLru = null!;
        private Mock<IValidate<Cita>> _mockValidador = null!;
        private IService<int, Cita> _service = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepositorioCita>();
            _mockCacheLru = new Mock<ICache<int, Cita>>();
            _mockValidador = new Mock<IValidate<Cita>>();

            _service = new ServiceVehiculos(
                _mockRepository.Object,
                _mockCacheLru.Object,
                _mockValidador.Object
            );
        }

        [Test]
        public void Agregar_VehiculoInvalido_DevuelveFailure()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "BAD", "AAA", "ZZZ", 0, Motor.Gasolina, "00000000B");

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Failure<bool, DomainError>(
                    new CitaError.ValidationError.ValidationMatricula(vehiculo.Matricula)));

            var resultado = _service.Agregar(vehiculo);

            resultado.IsFailure.Should().BeTrue();
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.Agregar(It.IsAny<Cita>()), Times.Never);
        }

        [Test]
        public void Agregar_MismoVehiculoMismaFecha_Falla()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z")
            {
                Id = 1
            };

            var lista = new List<Cita>()
            {
                new Cita(FechaMat, FechaInsp, "1111BBB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z")
            };

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(c => c.GetAll()).Returns(lista.AsEnumerable());

            var result = _service.Agregar(vehiculo);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<CitaError.FechaYaEstablecida>();
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(v => v.GetAll(), Times.Once);
            _mockRepository.Verify(v => v.Agregar(It.IsAny<Cita>()), Times.Never);
        }

        [Test]
        public void Agregar_4VehiculosMismFechaMismoDni_Falla()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "4444BBB", "tOYotA", "maLo", 1.0, Motor.Diesel, "12345678z")
            {
                Id = 4
            };

            var lista = new List<Cita>()
            {
                new Cita(FechaMat, FechaInsp, "1111BBB", "Algo", "Uno", 1.0, Motor.Diesel, "12345678z") { Id = 1 },
                new Cita(FechaMat, FechaInsp, "2222BBB", "Feo", "Dos", 1.0, Motor.Diesel, "12345678z") { Id = 2 },
                new Cita(FechaMat, FechaInsp, "3333BBB", "Toto", "Tres", 1.0, Motor.Diesel, "12345678z") { Id = 3 }
            };

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(c => c.GetAll()).Returns(lista.AsEnumerable());

            var result = _service.Agregar(vehiculo);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<CitaError.OwnerWithThreeOrMoreCitas>();
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(v => v.GetAll(), Times.AtLeastOnce);
            _mockRepository.Verify(v => v.Agregar(It.IsAny<Cita>()), Times.Never);
        }

        [Test]
        public void Actualizar_VehiculoNuevoIncorrecto_DevuelveFailure()
        {
            var idVehiculo = 50;
            var vehiculoAntiguo = new Cita(idVehiculo, FechaMat, FechaInsp, "4444DDD", "Citroen", "Saxo", 1.0,
                Motor.Diesel, "55443322X", false, Now, Now);
            var vehiculoNuevo = new Cita(idVehiculo, FechaMat, FechaInsp, "4444DDD", "Citroen", "Saxo", -5.0,
                Motor.Diesel, "55443322X", false, Now, Now);

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Failure<bool, DomainError>(
                    new CitaError.ValidationError.ValidationCilindrica(vehiculoNuevo.Cilindrada)));

            var resultado = _service.Actualizar(idVehiculo, vehiculoNuevo);

            resultado.IsFailure.Should().BeTrue();
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.BuscarId(idVehiculo), Times.Never);
            _mockRepository.Verify(r => r.Actualizar(idVehiculo, It.IsAny<Cita>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(idVehiculo), Times.Never);
        }

        [Test]
        public void Actualizar_IdAntiguoNuevoDistintos_DevuelveFailure()
        {
            var vehiculoAntiguo = new Cita(50, FechaMat, FechaInsp, "4444DDD", "Citroen", "Saxo", 1.0, Motor.Diesel,
                "55443322X", false, Now, Now);
            var vehiculoNuevo = new Cita(99, FechaMat, FechaInsp, "4444DDD", "Citroen", "Saxo", 1.0, Motor.Diesel,
                "55443322X", false, Now, Now);

            var resultado = _service.Actualizar(vehiculoAntiguo.Id, vehiculoNuevo);

            resultado.IsFailure.Should().BeTrue();
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.ExistId(vehiculoAntiguo.Id), Times.Once);
            _mockRepository.Verify(r => r.Actualizar(vehiculoAntiguo.Id, It.IsAny<Cita>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(vehiculoAntiguo.Id), Times.Never);
        }

        [Test]
        public void Actualizar_SiNoEncuentraId_DevuelveFailure()
        {
            var idVehiculo = 60;
            var vehiculoNuevo = new Cita(idVehiculo, FechaMat, FechaInsp, "4444DDD", "Citroen", "Saxo", 1.0,
                Motor.Diesel, "55443322X", false, Now, Now);

            _mockValidador.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(idVehiculo)).Returns(false);

            var resultado = _service.Actualizar(idVehiculo, vehiculoNuevo);

            resultado.IsFailure.Should().BeTrue();
            _mockValidador.Verify(v => v.Validar(It.IsAny<Cita>()), Times.Once);
            _mockRepository.Verify(r => r.Actualizar(idVehiculo, It.IsAny<Cita>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(idVehiculo), Times.Never);
        }

        [Test]
        public void Borrar_NoEncuentraId_DevuelveFailure()
        {
            var idVehiculo = 70;
            _mockRepository.Setup(r => r.BuscarId(idVehiculo))
                .Returns(Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(idVehiculo)));

            var result = _service.Borrar(idVehiculo);

            result.IsFailure.Should().BeTrue();
            _mockRepository.Verify(r => r.BuscarId(idVehiculo), Times.Once);
            _mockRepository.Verify(r => r.Borrar(It.IsAny<int>()), Times.Never);
            _mockCacheLru.Verify(c => c.Borrar(idVehiculo), Times.Never);
        }

        [Test]
        public void GetById_NoEstaRepositorio_DevuelveFailure()
        {
            var idVehiculo = 80;
            _mockCacheLru.Setup(r => r.Obtener(idVehiculo)).Returns((Cita)null!);
            _mockRepository.Setup(r => r.BuscarId(idVehiculo))
                .Returns(Result.Failure<Cita, DomainError>(new CitaError.CitaNotFoundId(idVehiculo)));

            var result = _service.GetById(idVehiculo);

            result.IsFailure.Should().BeTrue();
            _mockCacheLru.Verify(r => r.Obtener(idVehiculo), Times.Once);
            _mockRepository.Verify(r => r.BuscarId(idVehiculo), Times.Once);
            _mockCacheLru.Verify(c => c.Agregar(idVehiculo, It.IsAny<Cita>()), Times.Never);
        }

        [Test]
        public void GetAll_PaginacionFueraDeRango_ListaVacia()
        {
            var lista = new List<Cita>()
            {
                new Cita(91, FechaMat, FechaInsp, "1111AAA", "Exito", "...", 1000, Motor.Gasolina, "123z", false, Now,
                    Now),
                new Cita(92, FechaMat, FechaInsp, "2222BBB", "Error", "...", 1500, Motor.Diesel, "456x", false, Now,
                    Now),
                new Cita(93, FechaMat, FechaInsp, "3333CCC", "NoLlega", "...", 1200, Motor.Gasolina, "789y", false, Now,
                    Now)
            };
            _mockRepository.Setup(r => r.GetAll()).Returns(lista);

            var result = _service.GetAll(3, 1);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }
    }
}