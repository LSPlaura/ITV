using System.IO;
using CSharpFunctionalExtensions;
using FluentAssertions;
using ITV.Config;
using ITV.Entity;
using ITV.Error.Common;
using ITV.Error.DataBase;
using ITV.Error.Vehiculos;
using ITV.Mappers;
using ITV.Models;
using ITV.Repository.Common;
using ITV.Repository.Dapper;
using ITV.Repository.EFCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ITV.Test.Repository;

[TestFixture]
public class RepositorioEfTests
{
    private const string InMemoryConnection = "Data Source=:memory:";

    [TestFixture]
    public class CasosValidos
    {
        private IRepositorioVehiculos _repositorio = null!;
        private SqliteConnection _connection = null!;
        private AppDbContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            // Crear y abrir la conexión in-memory
            _connection = new SqliteConnection(InMemoryConnection);
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            // Crear el contexto y asegurar esquema desde el model
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            // Crear repositorio con el contexto
            _repositorio = new EfCoreRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
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
            
            [Test]
            public void Borrar_MarcaElimnado()
            {
                var vehiculo = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
                var agregado = _repositorio.Agregar(vehiculo);
    
                var result = _repositorio.Borrar(agregado.Value.Id);
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.IsDeleted.Should().Be(true);
            }
            
            [Test]
            public void Borrar_ObjetoElimnado()
            {
                var vehiculo = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
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
            public void BuscarId_vehiculo()
            { 
                var vehiculo = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
    
                var agregado = _repositorio.Agregar(vehiculo);
                
                var result = _repositorio.BuscarId(agregado.Value.Id);
              
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.Id.Should().Be(agregado.Value.Id);
            }
    
            [Test]
            public void BuscarMatricula_Vehiculo()
            { 
                var vehiculo = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
    
                var agregado = _repositorio.Agregar(vehiculo);
                
                var result = _repositorio.BuscarMatricula(agregado.Value.Matricula);
              
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.Matricula.Should().Be(agregado.Value.Matricula);
            }
            
            [Test]
            public void Actualizar_VehiculoActualizadoo()
            {
                var vehiculoAntiguo = new  Vehiculo("3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");
                var agregado = _repositorio.Agregar(vehiculoAntiguo);
                var vehiculoNuevo = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
                var result = _repositorio.Actualizar(agregado.Value.Id, vehiculoNuevo);
    
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                result.Value.Matricula.Should().Be(agregado.Value.Matricula);
                result.Value.Id.Should().Be(agregado.Value.Id);
            }
    
            [Test]
            public void BorrarRepositorio()
            {
                var vehiculo1 = new  Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
                var vehiculo2 = new  Vehiculo("2222BBB", "AAA", "ElMejor", 3.3, Motor.Gasolina, "12345678Z");
                var vehiculo3 = new  Vehiculo("3333BBB", "JKASD", "ElMejor", 3.3, Motor.Electrico, "12345678Z");
                
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
            private IRepositorioVehiculos _repositorio = null!;
            private SqliteConnection _connection = null!;
            private AppDbContext _context = null!;

            [SetUp]
            public void SetUp()
            {
                // Crear y abrir la conexión in-memory
                _connection = new SqliteConnection(InMemoryConnection);
                _connection.Open();

                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(_connection)
                    .Options;

                // Crear el contexto y asegurar esquema desde el model
                _context = new AppDbContext(options);
                _context.Database.EnsureCreated();

                // Crear repositorio con el contexto
                _repositorio = new EfCoreRepository(_context);
            }

            [TearDown]
            public void TearDown()
            {
                _context.Dispose();
                _connection.Close();
                _connection.Dispose();
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
                result2.Error.Should().BeOfType<VehiculoError.VehiculoAlredyExist.MatriculaAlreadyExists>();
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
                result4.Error.Should().BeOfType<VehiculoError.OwnerWithThreeOrMoreVehiculos>();
            }
           
            [Test]
            public void Borrar_ErrorEncontrarId()
            {
                var result = _repositorio.Borrar(20);
                result.Should().NotBeNull();
                result.IsFailure.Should().BeTrue();
                result.Error.Should().BeOfType<VehiculoError.VehiculoNotFoundId>();
            }
            
            [Test]
            public void BuscarId_Error()
            { 
                var result = _repositorio.BuscarId(0);
    
                result.Should().NotBeNull();
                result.IsFailure.Should().BeTrue();
                result.Error.Should().BeOfType<VehiculoError.VehiculoNotFoundId>();
            }
    
            [Test]
            public void BuscarMatricula_Error()
            { 
                var result = _repositorio.BuscarMatricula("1111BBB");
    
                result.Should().NotBeNull();
                result.IsFailure.Should().BeTrue();
                result.Error.Should().BeOfType<VehiculoError.VehiculoNotFoundMatricula>();
            }
    
            [Test]
            public void Actualizar_ErrorVehiculoNoEncontrado()
            {
                var id = 10;
                var vehiculoNuevo = new Vehiculo("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
                var result = _repositorio.Actualizar(id, vehiculoNuevo);
    
                result.Should().NotBeNull();
                result.IsFailure.Should().BeTrue();
                result.Error.Should().BeOfType<VehiculoError.VehiculoNotFoundId>();
            }
        } 
}