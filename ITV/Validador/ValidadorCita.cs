using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Error.Vehiculos;
using ITV.Models;
using Serilog;

namespace ITV.Validador;
public class ValidadorCita : IValidate<Cita>
{
    private readonly ILogger _logger = Log.ForContext<ValidadorCita>();

    // Regex estáticos y compilados para máximo rendimiento
    private static readonly Regex MatriculaRegex = new(@"^[0-9]{4}[B-DF-HJ-NPR-TV-Z]{3}$");
    private static readonly Regex MarcaModeloRegex = new(@"^[A-Za-zÑñ ]{3,100}$");
    private static readonly Regex DniRegex = new(@"^[0-9]{8}[A-Za-z]{1}$");

    public Result<bool, DomainError> Validar(Cita item)
    {
        if (string.IsNullOrWhiteSpace(item.Matricula) || !MatriculaRegex.IsMatch(item.Matricula))
            return Fail(new CitaError.ValidationError.ValidationMatricula(item.Matricula), "Matrícula no válida");

        if (item.FechaMatriculacion > DateTime.Today)
            return Fail(new CitaError.ValidationError.ValidationMatricula(item.Matricula), "Fecha de matriculación fuera de rango (no puede ser mayor que la fecha actual");
        
        if (item.FechaInspeccion <= DateTime.Today || item.FechaInspeccion >= DateTime.Today.AddDays(30))
            return Fail(new CitaError.ValidationError.ValidationMatricula(item.Matricula), "Fecha de inspección fuera de rango (1-30 días)");
        
        if (!EsTextoValido(item.Marca))
            return Fail(new CitaError.ValidationError.ValidationMarca(item.Marca), "Marca no válida");

        if (!EsTextoValido(item.Modelo))
            return Fail(new CitaError.ValidationError.ValidationModelo(item.Modelo), "Modelo no válido");
        
        if (item.Cilindrada < 0)
            return Fail(new CitaError.ValidationError.ValidationCilindrica(item.Cilindrada), "Cilindrada negativa");

        if (!Enum.IsDefined(typeof(Motor), item.Motor))
            return Fail(new CitaError.ValidationError.ValidationMotor(item.Motor), "Motor no válido");
        
        if (!ValidarDni(item.DniDueño))
            return Fail(new CitaError.ValidationError.ValidationDni(item.DniDueño), "DNI no válido");

        return Result.Success<bool, DomainError>(true);
    }
    
    private bool EsTextoValido(string texto) => 
        !string.IsNullOrWhiteSpace(texto) && MarcaModeloRegex.IsMatch(texto) && !texto.Contains("  ");
    
    private Result<bool, DomainError> Fail(DomainError error, string motivo)
    {
        _logger.Warning("Validación fallida: {Motivo}. Error: {Error}", motivo, error.GetType().Name);
        return Result.Failure<bool, DomainError>(error);
    }

    public static bool ValidarDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni) || !DniRegex.IsMatch(dni)) return false;

        const string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
        // Usamos Span o Substring de forma eficiente
        if (int.TryParse(dni.AsSpan(0, dni.Length - 1), out var num))
        {
            return char.ToUpper(dni[^1]) == letras[num % 23];
        }
        return false;
    }
}