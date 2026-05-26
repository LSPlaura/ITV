using FluentAssertions;
using ITV.Dto;
using ITV.Entity;
using ITV.Mappers;
using ITV.Models;

namespace ITV.Test.Mapper;

[TestFixture]
public class CitaMapperTest
{
    [TestFixture]
    public class CasosValidos
    {
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-2);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(10);
        private static readonly Cita CitaValida = new(
            FechaMat, 
            FechaInsp, 
            "1234BBB", 
            "Seat", 
            "Ibiza", 
            1200.0, 
            Motor.Gasolina, 
            "12345678Z"
        ) { Id = 1, IsDeleted = false };

        [Test]
        public void ToDto_CitaValida_ConvierteMaintieneDatos()
        {
            var dto = CitaValida.ToDto();

            dto.Should().NotBeNull();
            dto.Id.Should().Be(CitaValida.Id);
            dto.Matricula.Should().Be(CitaValida.Matricula);
            dto.Marca.Should().Be(CitaValida.Marca);
            dto.Modelo.Should().Be(CitaValida.Modelo);
            dto.Cilindrada.Should().Be(CitaValida.Cilindrada);
            dto.Motor.Should().Be((int)CitaValida.Motor);
            dto.DniDueño.Should().Be(CitaValida.DniDueño);
            dto.IsDeleted.Should().Be(0);
        }

        [Test]
        public void ToDto_FechasEnFormatoISO()
        {
            var dto = CitaValida.ToDto();

            dto.FechaMatriculacion.Should().Match("*-*-*T*:*:*");
            dto.FechaInspeccion.Should().Match("*-*-*T*:*:*");
            dto.CreatedAt.Should().Match("*-*-*T*:*:*");
            dto.UpdatedAt.Should().Match("*-*-*T*:*:*");
        }

        [Test]
        public void ToDto_CitaBorrada_ConvierteFlagAUno()
        {
            var citaBorrada = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z") 
            { 
                Id = 1, 
                IsDeleted = true 
            };

            var dto = citaBorrada.ToDto();

            dto.IsDeleted.Should().Be(1);
        }

        [Test]
        public void ToModel_CitaDtoValida_ConvierteMaintieneDatos()
        {
            var dto = CitaValida.ToDto();
            var cita = dto.ToModel();

            cita.Should().NotBeNull();
            cita.Id.Should().Be(CitaValida.Id);
            cita.Matricula.Should().Be(CitaValida.Matricula);
            cita.Marca.Should().Be(CitaValida.Marca);
            cita.Modelo.Should().Be(CitaValida.Modelo);
            cita.Cilindrada.Should().Be(CitaValida.Cilindrada);
            cita.Motor.Should().Be(CitaValida.Motor);
            cita.DniDueño.Should().Be(CitaValida.DniDueño);
            cita.IsDeleted.Should().BeFalse();
        }

        [Test]
        public void ToEntity_CitaValida_ConvierteMaintieneDatos()
        {
            var entity = CitaValida.ToEntity();

            entity.Should().NotBeNull();
            entity.Id.Should().Be(CitaValida.Id);
            entity.Matricula.Should().Be(CitaValida.Matricula);
            entity.Marca.Should().Be(CitaValida.Marca);
            entity.Modelo.Should().Be(CitaValida.Modelo);
            entity.Cilindrada.Should().Be(CitaValida.Cilindrada);
            entity.Motor.Should().Be((int)CitaValida.Motor);
            entity.DniDueño.Should().Be(CitaValida.DniDueño);
        }

        [Test]
        public void ToModel_CitaEntityValida_ConvierteMaintieneDatos()
        {
            var entity = CitaValida.ToEntity();
            var cita = entity.ToModel();

            cita.Should().NotBeNull();
            cita.Id.Should().Be(CitaValida.Id);
            cita.Matricula.Should().Be(CitaValida.Matricula);
            cita.Marca.Should().Be(CitaValida.Marca);
            cita.Modelo.Should().Be(CitaValida.Modelo);
            cita.Cilindrada.Should().Be(CitaValida.Cilindrada);
            cita.Motor.Should().Be(CitaValida.Motor);
            cita.DniDueño.Should().Be(CitaValida.DniDueño);
        }

        [Test]
        public void ToModel_ListaCitaEntity_ConvierteListaMaintieneDatos()
        {
            var cita1 = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, Motor.Gasolina, "12345678Z") { Id = 1 };
            var cita2 = new Cita(FechaMat, FechaInsp, "9876DDD", "Toyota", "Corolla", 1500.0, Motor.Diesel, "87654321A") { Id = 2 };

            var entities = new List<CitaEntity> { cita1.ToEntity(), cita2.ToEntity() };
            var citas = entities.ToModel().ToList();

            citas.Should().HaveCount(2);
            citas[0].Id.Should().Be(cita1.Id);
            citas[1].Id.Should().Be(cita2.Id);
            citas[0].Matricula.Should().Be(cita1.Matricula);
            citas[1].Matricula.Should().Be(cita2.Matricula);
        }

        [Test]
        public void RoundTrip_CitaDtoAModel_MaintieneDatos()
        {
            var original = CitaValida;
            var dto = original.ToDto();
            var convertido = dto.ToModel();

            convertido.Id.Should().Be(original.Id);
            convertido.Matricula.Should().Be(original.Matricula);
            convertido.Marca.Should().Be(original.Marca);
            convertido.Modelo.Should().Be(original.Modelo);
            convertido.Cilindrada.Should().Be(original.Cilindrada);
            convertido.Motor.Should().Be(original.Motor);
            convertido.DniDueño.Should().Be(original.DniDueño);
        }

        [Test]
        public void RoundTrip_CitaEntityAModel_MaintieneDatos()
        {
            var original = CitaValida;
            var entity = original.ToEntity();
            var convertido = entity.ToModel();

            convertido.Id.Should().Be(original.Id);
            convertido.Matricula.Should().Be(original.Matricula);
            convertido.Marca.Should().Be(original.Marca);
            convertido.Modelo.Should().Be(original.Modelo);
            convertido.Cilindrada.Should().Be(original.Cilindrada);
            convertido.Motor.Should().Be(original.Motor);
            convertido.DniDueño.Should().Be(original.DniDueño);
        }

        [Test]
        [TestCase(Motor.Gasolina)]
        [TestCase(Motor.Diesel)]
        [TestCase(Motor.Electrico)]
        [TestCase(Motor.Hidrogeno)]
        public void ToDto_TodosLosMotores_ConvierteCorrectamente(Motor motor)
        {
            var cita = new Cita(FechaMat, FechaInsp, "1234BBB", "Seat", "Ibiza", 1200.0, motor, "12345678Z") { Id = 1 };

            var dto = cita.ToDto();

            dto.Motor.Should().Be((int)motor);
        }
    }

    [TestFixture]
    public class CasosInvalidos
    {
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-2);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(10);

        [Test]
        public void ToModel_DtoConFechaInvalida_AsignaFechaActual()
        {
            var dto = new CitaDto(
                1,
                "fecha-invalida",
                "otra-invalida",
                "1234BBB",
                "Seat",
                "Ibiza",
                1200.0,
                (int)Motor.Gasolina,
                "12345678Z",
                0,
                "fecha-creacion-invalida",
                "fecha-actualizacion-invalida"
            );

            var cita = dto.ToModel();

            cita.FechaMatriculacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
            cita.FechaInspeccion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
            cita.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
            cita.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
        }

        [Test]
        public void ToModel_DtoConMotorInvalido_AsignaGasolina()
        {
            var dto = new CitaDto(
                1,
                DateTime.Now.ToString("s"),
                DateTime.Now.ToString("s"),
                "1234BBB",
                "Seat",
                "Ibiza",
                1200.0,
                999, // Motor inválido
                "12345678Z",
                0,
                DateTime.Now.ToString("s"),
                DateTime.Now.ToString("s")
            );

            var cita = dto.ToModel();

            cita.Motor.Should().Be(Motor.Gasolina);
        }

        [Test]
        public void ToModel_CitaEntityConMotorInvalido_AsignaGasolina()
        {
            var entity = new CitaEntity(
                1,
                DateTime.Now.ToString("s"),
                DateTime.Now.ToString("s"),
                "1234BBB",
                "Seat",
                "Ibiza",
                1200.0,
                999, // Motor inválido
                "12345678Z",
                0,
                DateTime.Now.ToString("s"),
                DateTime.Now.ToString("s")
            );

            var cita = entity.ToModel();

            cita.Motor.Should().Be(Motor.Gasolina);
        }

        [Test]
        public void ToModel_ListaCitaEntityVacia_DevuelveListaVacia()
        {
            var entities = new List<CitaEntity>();

            var citas = entities.ToModel().ToList();

            citas.Should().BeEmpty();
        }

        [Test]
        public void ToDto_FlagIsDeletedEnUno_ConvierteATrue()
        {
            var dto = new CitaDto(
                1,
                DateTime.Now.ToString("s"),
                DateTime.Now.ToString("s"),
                "1234BBB",
                "Seat",
                "Ibiza",
                1200.0,
                (int)Motor.Gasolina,
                "12345678Z",
                1, // IsDeleted = 1
                DateTime.Now.ToString("s"),
                DateTime.Now.ToString("s")
            );

            var cita = dto.ToModel();

            cita.IsDeleted.Should().BeTrue();
        }
    }
}