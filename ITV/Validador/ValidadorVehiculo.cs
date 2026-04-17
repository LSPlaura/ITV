using System;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Error.Vehiculos;
using ITV.Models;
using Serilog;

namespace ITV.Validador;

/// <inheritdoc/>
public class ValidadorVehiculo : IValidate<Vehiculo>
{
    private static readonly ILogger Logger = Log.ForContext<ValidadorVehiculo>();
    
    private readonly Regex _matriculaRegex = new Regex("^[0-9]{4}[B-DF-HJ-NPR-TV-Z]{3}$");
    private readonly Regex _marcaModeloRegex = new Regex("^[A-Za-zÑñ ]{3,100}$"); 
    private static Regex _dniRegex = new Regex("^[0-9]{8}[A-Za-z]{1}$");
    private readonly ILogger _logger = Log.ForContext<ValidadorVehiculo>();
    
    public Result<bool, DomainError> Validar(Vehiculo item)
    {
        if (!_matriculaRegex.IsMatch(item.Matricula))
        {
            Logger.Warning("Validación fallida: Matrícula incorrecta {Matricula}", item.Matricula);
            return Result.Failure<bool, DomainError>(
                new VehiculoError.ValidationError.ValidationMatricula(item.Matricula))
                .TapError(v => _logger.Error("El vehiculo no tiene una matricula {Matricula} valida", item.Matricula));
        }
        
        if (!_marcaModeloRegex.IsMatch(item.Marca))
        {
            Logger.Warning("Validación fallida: Marca no permitida {Marca}", item.Marca);
            return Result.Failure<bool, DomainError>(
                    new VehiculoError.ValidationError.ValidationMarca(item.Marca))
                .TapError(v => _logger.Error("El vehiculo no tiene una marca {Marca} valida", item.Marca));
        }
        
        if (!_marcaModeloRegex.IsMatch(item.Modelo))
        {
            Logger.Warning("Validación fallida: Modelo no permitido {Modelo}", item.Modelo);
            return Result.Failure<bool, DomainError>(
                    new VehiculoError.ValidationError.ValidationModelo(item.Marca))
                .TapError(v => _logger.Error("El vehiculo no tiene un modelo {Modelo} valido", item.Modelo));
        }
        
        if (item.Cilindrada < 0)
        {
            Logger.Warning("Validación fallida: Cilindrada negativa {Cilindrada}", item.Cilindrada);
            return Result.Failure<bool, DomainError>(
                    new VehiculoError.ValidationError.ValidationCilindrica(item.Cilindrada))
                .TapError(v => _logger.Error("El vehiculo no tiene una cilíndrica valida {Cilindrada} valida", item.Cilindrada));
        }
        
        if (!Enum.IsDefined(typeof(Motor), item.Motor))
        {
            Logger.Warning("Validación fallida: Tipo de motor inexistente {Motor}", item.Motor);
            return Result.Failure<bool, DomainError>(
                    new VehiculoError.ValidationError.ValidationMotor(item.Motor))
                .TapError(v => _logger.Error("El vehiculo no tiene el tipo de motor {Motor} adecuado", item.Motor));
        }
        
        if (!ValidarDni(item.DniDueño))
        {
            Logger.Warning("Validación fallida: DNI inválido o letra incorrecta {Dni}", item.DniDueño);
            return Result.Failure<bool, DomainError>(
                    new VehiculoError.ValidationError.ValidationDni(item.DniDueño))
                .TapError(v => _logger.Error("El vehiculo no tiene un DNI {Dni} valido", item.DniDueño));
        }
        return Result.Success<bool, DomainError>(true);
    }
    
    public static bool ValidarDni(string dni)
    {
        var letras = "TRWAGMYFPDXBNJZSQVHLCKE";
        var dividendo = 23;
        
        if (string.IsNullOrWhiteSpace(dni)) return false;
        
        if (_dniRegex.IsMatch(dni))
        {
            if (int.TryParse(dni.Substring(0, dni.Length - 1), out var num))
            {
                if (dni.ToUpper().Last() == letras[num % dividendo]) 
                {
                    return true;
                }
            }
        }
        return false;
    }
}