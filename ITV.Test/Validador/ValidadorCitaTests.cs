using ITV.Models;
using ITV.Validador;

namespace ITV.Test.Validador;

[TestFixture]
public class ValidadorCitaTests
{
    
    [SetUp]
    public void SetUp()
    {
        _validador = new ValidadorCita();
    }
    
    [TestFixture]
    public class CasosValidos()
    {
        private ValidadorCita _validador = null!;
    
        [SetUp]
        public void SetUp()
        {
            _validador = new ValidadorCita();
        }
        
        [Test]
        public void Validar_VehiculoCorrecto_SinErrores()
        {
            var vehiculo = new Cita("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");

            var result = _validador.Validar(vehiculo);
            
            Assert.That(result.IsSuccess, Is.True);
        }
    }
    
    public class CasosInalidos()
    {
        private ValidadorCita _validador = null!;
    
        [SetUp]
        public void SetUp()
        {
            _validador = new ValidadorCita();
        }
        
        [TestCase(null)]
        [TestCase("")]
        [TestCase("        ")]
        [TestCase("BBB1111")]
        [TestCase("BB11111")]
        [TestCase("BBBB111")]
        public void Validar_MatriculaIncorrecta_Errores(string? matricula)
        {
            var vehiculo = new Cita(matricula, "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");

            var result = _validador.Validar(vehiculo);
            
            Assert.That(result.IsFailure, Is.True);
        }
        
        [TestCase(null)]
        [TestCase("")]
        [TestCase("        ")]
        [TestCase("aa")]
        [TestCase("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public void Validar_MarcaIncorrecta_Errores(string? marca)
        {
            var vehiculo = new Cita("1111BBB", marca, "ElMejor", 3.3, Motor.Diesel, "12345678Z");

            var result = _validador.Validar(vehiculo);
            
            Assert.That(result.IsFailure, Is.True);
        }
        
        [TestCase(-1.1)]
        public void Validar_CilindradaIncorrecta_Errores(double cilindrada)
        {
            var vehiculo = new Cita("1111BBB", "Toyota", "ElMejor", cilindrada, Motor.Diesel, "12345678Z");

            var result = _validador.Validar(vehiculo);
            
            Assert.That(result.IsFailure, Is.True);
        }
        
        [TestCase(-1)]
        [TestCase(4)]
        public void Validar_MotorIncorrecta_Errores(int motor)
        {
            var vehiculo = new Cita("1111BBB", "Toyota", "ElMejor", 3.3, (Motor)motor, "12345678Z");

            var result = _validador.Validar(vehiculo);
            
            Assert.That(result.IsFailure, Is.True);
        }
        
        [TestCase(null)]
        [TestCase("")]
        [TestCase("         ")]
        [TestCase("1234567Z")]
        [TestCase("12345678A")]
        [TestCase("1234D45678")]
        [TestCase("1234456789")]
        public void Validar_MotorIncorrecta_Errores(string? dni)
        {
            var vehiculo = new Cita("1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, dni);

            var result = _validador.Validar(vehiculo);
            
            Assert.That(result.IsFailure, Is.True);
        }
    }
}