using ITV.Error.Common;
using ITV.Models;

namespace ITV.Error.Vehiculos;

/// <summary>
/// Record para capturar errores al operar con <see cref="Cita"/>>
/// </summary>
public abstract record CitaError(string Message) : DomainError(Message)
{
    /// <summary>
    /// Errores de validación
    /// </summary>
    /// <param name="Message"></param>
    public record ValidationError(string Message) : CitaError(Message)
    {
        public sealed record ValidationMatricula(string Matricula)
            : ValidationError($"Error en la validación: Matrícula[{Matricula}] no válida");
        public sealed record ValidationMarca(string Marca)
            : ValidationError($"Error en la validación: Marca [{Marca}] no válida");
        public sealed record ValidationModelo(string Modelo)
            : ValidationError($"Error en la validación: Modelo [{Modelo}] no válido");
        public sealed record ValidationCilindrica(double Cilindrica)
            : ValidationError($"Error en la validación: Cilindrada [{Cilindrica}] no válida");
        public sealed record ValidationMotor(Motor Motor)
            : ValidationError($"Error en la validación: Motor [{Motor}] no válido");
        public sealed record ValidationDni(string Dni)
            : ValidationError($"Error en la validación: DNI [{Dni}] no valido");
        public sealed record ValidationFechaMatriculacion(DateTime Fecha)
            : ValidationError($"Error en la validación: Fecha de matriculación [{Fecha:dd/MM/yyyy}] no puede ser mayor que la fecha actual");

        public sealed record ValidationFechaInspeccion(DateTime Fecha)
            : ValidationError($"Error en la validación: Fecha de inspección [{Fecha:dd/MM/yyyy}] fuera de rango permitido (debe ser entre hoy y los próximos 30 días)");
    }
    
    /// <summary>
    /// Errores de vehiculos no encontrados en el <see cref="IRepositorioVehiculo"/>
    /// </summary>
    public sealed record CitaNotFoundMatricula(string Matricula)
        : CitaError($"Error: Cita con la matricula [{Matricula}] no encontrado");
    public sealed record CitaNotFoundId(int Id)
        : CitaError($"Error: Cita con el ID [{Id}] no encontrado");

    /// <summary>
    /// Errores de vehiculos ya incoporados en el <see cref="IRepositorioVehiculo"/>
    /// </summary>
    public record CitaAlredyExist(string Message) : CitaError(Message)
    {
        public record class IdAlreadyExists(int Id) : CitaAlredyExist($"Error: El vehiculo con el id [{Id}] ya existe");
        public record class MatriculaAlreadyExists(string matricula) : CitaAlredyExist($"Error: El vehiculo con la matrícula [{matricula}] ya existe");
    }

    /// <summary>
    /// Error que se devuelve cuando se alcanza el límite máximo de 3 citas permitidas para un mismo dni en una misma fecha.
    /// </summary>
    public sealed record OwnerWithThreeOrMoreCitas(string Dni, DateTime Fecha)
        : CitaError($"Error: El DNI {Dni} ya tiene tres citas establecidas para la fecha {Fecha}");
    
    /// <summary>
    /// Error que se devuelve cuando se alcanza el límite de una cita para un mismo vehículo en la misma fecha
    /// </summary>
    public sealed record FechaYaEstablecida(string Matricula, DateTime Fecha) 
        : CitaError($"Error: El vehiculo con la matricula {Matricula}, ya tiene una cita programada para la fecha {Fecha}");
    
}