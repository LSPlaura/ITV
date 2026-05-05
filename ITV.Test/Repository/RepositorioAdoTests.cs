using System.IO;
using FluentAssertions;
using ITV.Error.Vehiculos;
using ITV.Models;
using ITV.Repository.Ado;
using ITV.Repository.Common;
using ITV.Repository.Dapper;
using Microsoft.Data.Sqlite;

[TestFixture]
public class RepositorioAdoTests
{
    [TestFixture]
    public class CasosValidos
    {
        private IRepositorioVehiculos _repositorio = null!;
        private string _dbFolder = null!;
        private string _dbPath = null!;
        private string _connection = null!;
        
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        [SetUp]
        public void SetUp()
        {
            _dbFolder = Path.Combine(Path.GetTempPath(), "RepoTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dbFolder);

            _dbPath = Path.Combine(_dbFolder, "vehiculos.db");
            _connection = $"Data Source={_dbPath};";

            using var anchor = new SqliteConnection(_connection);
            anchor.Open();
            using var cmd = anchor.CreateCommand();
            cmd.CommandText = @"
              CREATE TABLE IF NOT EXISTS Cita(
                Id INTEGER PRIMARY KEY,
                Matricula VARCHAR(9) NOT NULL UNIQUE,
                Modelo  VARCHAR(100) NOT NULL,
                Marca VARCHAR(100) NOT NULL,
                Motor INTEGER NOT NULL,
                Cilindrada REAL CHECK (Cilindrada > 0) NOT NULL,
                DniDueno VARCHAR(9) NOT NULL,
                IsDeleted INTEGER DEFAULT 0
              );";
            cmd.ExecuteNonQuery();
            anchor.Close();

            _repositorio = new AdoRepository(_connection);
        }

        [TearDown]
        public void TearDown()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (Directory.Exists(_dbFolder))
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        Directory.Delete(_dbFolder, true);
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }

        [Test]
        public void Agregar_SinErrores()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");

            var result = _repositorio.Agregar(vehiculo);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().BeGreaterThan(0);
            result.Value.IsDeleted.Should().Be(false);
        }

        [Test]
        public void Borrar_MarcaElimnado()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var agregado = _repositorio.Agregar(vehiculo);

            var result = _repositorio.Borrar(agregado.Value.Id);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.IsDeleted.Should().Be(true);
        }

        [Test]
        public void Borrar_ObjetoElimnado()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var agregado = _repositorio.Agregar(vehiculo);

            var result = _repositorio.Borrar(agregado.Value.Id, false);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.IsDeleted.Should().Be(false);

            var comprobacion = _repositorio.Borrar(agregado.Value.Id, false);
            comprobacion.Should().NotBeNull();
            comprobacion.IsFailure.Should().BeTrue();
        }

        [Test]
        public void BuscarId_vehiculo()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var agregado = _repositorio.Agregar(vehiculo);

            var result = _repositorio.BuscarId(agregado.Value.Id);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(agregado.Value.Id);
        }

        [Test]
        public void BuscarMatricula_Vehiculo()
        {
            var vehiculo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var agregado = _repositorio.Agregar(vehiculo);

            var result = _repositorio.BuscarMatricula(agregado.Value.Matricula);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Matricula.Should().Be(agregado.Value.Matricula);
        }

        [Test]
        public void Actualizar_VehiculoActualizadoo()
        {
            var vehiculoAntiguo = new Cita(FechaMat, FechaInsp, "3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");
            var agregado = _repositorio.Agregar(vehiculoAntiguo);
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var result = _repositorio.Actualizar(agregado.Value.Id, vehiculoNuevo);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Matricula.Should().Be(agregado.Value.Matricula);
            result.Value.Id.Should().Be(agregado.Value.Id);
        }

        [Test]
        public void BorrarRepositorio()
        {
            var vehiculo1 = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var vehiculo2 = new Cita(FechaMat, FechaInsp, "2222BBB", "AAA", "ElMejor", 3.3, Motor.Gasolina, "12345678Z");
            var vehiculo3 = new Cita(FechaMat, FechaInsp, "3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");

            _repositorio.Agregar(vehiculo1);
            _repositorio.Agregar(vehiculo2);
            _repositorio.Agregar(vehiculo3);

            _repositorio.DeleteAll();
            _repositorio.GetAll().Any().Should().BeFalse();
        }
    }

    [TestFixture]
    public class CasosInvalidos
    {
        private IRepositorioVehiculos _repositorio = null!;
        private string _dbFolder = null!;
        private string _dbPath = null!;
        private string _connection = null!;

        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        [SetUp]
        public void SetUp()
        {
            _dbFolder = Path.Combine(Path.GetTempPath(), "RepoTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dbFolder);

            _dbPath = Path.Combine(_dbFolder, "vehiculos.db");
            _connection = $"Data Source={_dbPath};";

            using var anchor = new SqliteConnection(_connection);
            anchor.Open();
            using var cmd = anchor.CreateCommand();
            cmd.CommandText = @"
              CREATE TABLE IF NOT EXISTS Cita(
                Id INTEGER PRIMARY KEY,
                Matricula VARCHAR(9) NOT NULL UNIQUE,
                Modelo  VARCHAR(100) NOT NULL,
                Marca VARCHAR(100) NOT NULL,
                Motor INTEGER NOT NULL,
                Cilindrada REAL CHECK (Cilindrada > 0) NOT NULL,
                DniDueno VARCHAR(9) NOT NULL,
                IsDeleted INTEGER DEFAULT 0
              );";
            cmd.ExecuteNonQuery();
            anchor.Close();

            _repositorio = new DapperRepository(_connection);
        }

        [TearDown]
        public void TearDown()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (Directory.Exists(_dbFolder))
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        Directory.Delete(_dbFolder, true);
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }

        [Test]
        public void Agregar_ErrorMatricula()
        {
            var vehiculo1 = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var vehiculo2 = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");

            var result1 = _repositorio.Agregar(vehiculo1);
            var result2 = _repositorio.Agregar(vehiculo2);
            result1.Should().NotBeNull();
            result1.IsSuccess.Should().BeTrue();
            result1.Value.Id.Should().BeGreaterThan(0);
            result1.Value.IsDeleted.Should().Be(false);

            result2.Should().NotBeNull();
            result2.IsFailure.Should().BeTrue();
            result2.Error.Should().BeOfType<CitaError.CitaAlredyExist.MatriculaAlreadyExists>();
        }

        [Test]
        public void Agregar_ErrorOwnerCon3Vehiculos()
        {
            var vehiculo1 = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var vehiculo2 = new Cita(FechaMat, FechaInsp, "2222BBB", "AAA", "ElMejor", 3.3, Motor.Gasolina, "12345678Z");
            var vehiculo3 = new Cita(FechaMat, FechaInsp, "3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");
            var vehiculo4 = new Cita(FechaMat, FechaInsp, "4444BBB", "NKNN", "ElMejor", 3.3, Motor.Hidrogeno, "12345678Z");

            var result1 = _repositorio.Agregar(vehiculo1);
            var result2 = _repositorio.Agregar(vehiculo2);
            var result3 = _repositorio.Agregar(vehiculo3);
            var result4 = _repositorio.Agregar(vehiculo4);

            result1.IsSuccess.Should().BeTrue();
            result2.IsSuccess.Should().BeTrue();
            result3.IsSuccess.Should().BeTrue();

            result4.Should().NotBeNull();
            result4.IsFailure.Should().BeTrue();
            result4.Error.Should().BeOfType<CitaError.OwnerWithThreeOrMoreCitas>();
        }

        [Test]
        public void Borrar_ErrorEncontrarId()
        {
            var result = _repositorio.Borrar(20);
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<CitaError.CitaNotFoundId>();
        }

        [Test]
        public void BuscarId_Error()
        {
            var result = _repositorio.BuscarId(0);

            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<CitaError.CitaNotFoundId>();
        }

        [Test]
        public void BuscarMatricula_Error()
        {
            var result = _repositorio.BuscarMatricula("1111BBB");

            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<CitaError.CitaNotFoundMatricula>();
        }

        [Test]
        public void Actualizar_ErrorVehiculoNoEncontrado()
        {
            var id = 10;
            var vehiculoNuevo = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var result = _repositorio.Actualizar(id, vehiculoNuevo);

            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<CitaError.CitaNotFoundId>();
        }
    }
}