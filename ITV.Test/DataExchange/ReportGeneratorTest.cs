using FluentAssertions;
using ITV.Error.Export;
using ITV.Models;
using ITV.Service.Export;

namespace ITV.Test.DataExchange;

[TestFixture]
public class ReportGeneratorTest
{
    [TestFixture]
    public class CasosValidos
    {
        private IReportGenerator<Cita> _report = null!;
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        [SetUp]
        public void SetUp()
        {
            _report = new ReportGenerator();
        }
        
        [TearDown]
        public void TearDown()
        {
            _report = null!;
        }

        [Test]
        public void ExportHtml_DevuelveSuccess()
        {
            var cita = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var result = _report.ExportHtml(cita);
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeOfType<string>();
        }
        
        [Test]
        public void ExportPdf_DevuelveSuccess()
        {
            var cita = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var result = _report.ExportPdf(cita);
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeOfType<string>();
        }
    }
    
    [TestFixture]
    public class CasosInvalidos
    {
        private IReportGenerator<Cita> _report = null!;
        private static readonly DateTime FechaMat = DateTime.Today.AddYears(-1);
        private static readonly DateTime FechaInsp = DateTime.Today.AddDays(15);

        [SetUp]
        public void SetUp()
        {
            _report = new ReportGenerator();
        }
        
        [TearDown]
        public void TearDown()
        {
            _report = null!;
        }
        
        [Test]
        public void ExportHtml_DevuelveFailure()
        {
            var cita = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var result = _report.ExportHtml(cita);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ReportError.ReportHtmlError>();
        }
        
        [Test]
        public void ExportPdf_DevuelveFailure()
        {
            var cita = new Cita(FechaMat, FechaInsp, "1111BBB", "Toyota", "ElMejor", 3.3, Motor.Diesel, "12345678Z");
            var result = _report.ExportPdf(cita);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ReportError.ReportPdfError>();
        }
    }
}