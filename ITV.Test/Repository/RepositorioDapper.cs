using System.Data;
using System.IO;
using FluentAssertions;
using ITV.Error.Citas;
using ITV.Models;
using ITV.Repository.Ado;
using ITV.Repository.Common;
using ITV.Repository.Dapper;
using Microsoft.Data.Sqlite;

namespace ITV.Test.Repository;

[TestFixture]
public class RepositorioDapper
{
    [TestFixture]
    public class CasosValidosconn
    {
      private IRepositorioCitas _repositorio = null!;
        private SqliteConnection _connection = null!;
       
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _repositorio = new DapperRepository(_connection);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Close();
            _connection.Dispose();
        }
            [Test]
            public void Agregar_SinErrores()
            {
                var vehiculo = new  Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
    
                var result = _repositorio.Agregar(vehiculo);
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.Id.Should().BeGreaterThan(0);
                result.Value.IsDeleted.Should().Be(false);
                
            }
            
            [Test]
            public void Borrar_MarcaElimnado()
            {
                var vehiculo = new  Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
                var agregado = _repositorio.Agregar(vehiculo);
    
                var result = _repositorio.Borrar(agregado.Value.Id);
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.IsDeleted.Should().Be(true);
            }
            
            [Test]
            public void Borrar_ObjetoElimnado()
            {
                var vehiculo = new  Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
                var agregado = _repositorio.Agregar(vehiculo);
    
                var result = _repositorio.Borrar(agregado.Value.Id, false);
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.IsDeleted.Should().Be(false);
                
                var comprobacion =  _repositorio.Borrar(agregado.Value.Id, false);
                comprobacion.Should().NotBeNull();
                comprobacion.IsFailure.Should().BeTrue();
            }
            
            [Test]
            public void Borrar_UpdatedAtActualilzaCorrectamente()
            {
                var vehiculoAntiguo =
                    new Cita(FechaMat, FechaInsp, "3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z")
                        { UpdatedAt = DateTime.Now.AddDays(-1) };
                var agregado = _repositorio.Agregar(vehiculoAntiguo);

                var result = _repositorio.Borrar(agregado.Value.Id);
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
            }
            
            [Test]
            public void BuscarId_vehiculo()
            { 
                var vehiculo = new  Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
    
                var agregado = _repositorio.Agregar(vehiculo);
                
                var result = _repositorio.BuscarId(agregado.Value.Id);
              
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.Id.Should().Be(agregado.Value.Id);
            }
    
            [Test]
            public void BuscarMatricula_Vehiculo()
            { 
                var vehiculo = new  Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
    
                var agregado = _repositorio.Agregar(vehiculo);
                
                var result = _repositorio.BuscarMatricula(agregado.Value.Matricula);
              
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.Matricula.Should().Be(agregado.Value.Matricula);
            }
            
            [Test]
            public void Actualizar_VehiculoActualizadoo()
            {
                var vehiculoAntiguo = new  Cita(FechaMat, FechaInsp, "3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");
                var agregado = _repositorio.Agregar(vehiculoAntiguo);
                var vehiculoNuevo = new  Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
                var result = _repositorio.Actualizar(agregado.Value.Id, vehiculoNuevo);
    
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.Matricula.Should().Be(vehiculoNuevo.Matricula);
                result.Value.Id.Should().Be(agregado.Value.Id);
            }
            
            [Test]
            public void Actualizar_UpdatedAtActualilzaCorrectamente()
            {
                var vehiculoAntiguo =
                    new Cita(FechaMat, FechaInsp, "3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z")
                        { UpdatedAt = DateTime.Now.AddDays(-1) };
                var agregado = _repositorio.Agregar(vehiculoAntiguo);
            
                var vehiculoNuevo = new Cita(FechaMat, FechaInsp, "1111BBB", "Tonto", "Feo", 3.3, Motor.Gasolina, "12345678Z");
                var result = _repositorio.Actualizar(agregado.Value.Id, vehiculoNuevo);

                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
            }
    
            [Test]
            public void BorrarRepositorio()
            {
                var vehiculo1 = new  Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
                var vehiculo2 = new  Cita(FechaMat, FechaInsp, "2222BBB", "AAA", "ElMejor", 3.3, Motor.Gasolina, "12345678Z");
                var vehiculo3 = new  Cita(FechaMat, FechaInsp, "3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");
                
                _repositorio.Agregar(vehiculo1);
                _repositorio.Agregar(vehiculo2);
                _repositorio.Agregar(vehiculo3);
                
                _repositorio.DeleteAll();
                _repositorio.GetAll().Any().Should().BeFalse();
            }
        }
        
        [TestFixture]
        public class CasosInvalidos()
        {
           private IRepositorioCitas _repositorio = null!;
        private SqliteConnection _connection = null!;
       
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _repositorio = new DapperRepository(_connection);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Close();
            _connection.Dispose();
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