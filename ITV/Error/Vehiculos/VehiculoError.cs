using ITV.Error.Common;
using ITV.Models;

namespace ITV.Error.Vehiculos;

/// <summary>
/// Record para capturar errores al operar con <see cref="Vehiculo"/>>
/// </summary>
public abstract record VehiculoError(string Message) : DomainError(Message)
{
    /// <summary>
    /// Errores de validación
    /// </summary>
    /// <param name="Message"></param>
    public record ValidationError(string Message) : VehiculoError(Message)
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
    }
    
    /// <summary>
    /// Errores de vehiculos no encontrados en el <see cref="IRepositorioVehiculo"/>
    /// </summary>
    public sealed record VehiculoNotFoundMatricula(string Matricula)
        : VehiculoError($"Error: Vehiculo con la matricula [{Matricula}] no encontrado");
    public sealed record VehiculoNotFoundId(int Id)
        : VehiculoError($"Error: Vehiculo con el ID [{Id}] no encontrado");

    /// <summary>
    /// Errores de vehiculos ya incoporados en el <see cref="IRepositorioVehiculo"/>
    /// </summary>
    public record VehiculoAlredyExist(string Message) : VehiculoError(Message)
    {
        public record class IdAlreadyExists(int Id) : VehiculoAlredyExist($"Error: El vehiculo con el id [{Id}] ya existe");
        public record class MatriculaAlreadyExists(string matricula) : VehiculoAlredyExist($"Error: El vehiculo con la matrícula [{matricula}] ya existe");
    }
    /// <summary>
    /// Error que se devuelve cuando la mátricula del vehículo que se quiere actualizar no es la misma que la almacenada con los datos a actualizar
    /// </summary>
    public sealed record InconsistentUpdate(string Key, string MatriculaVehiculo)
        : VehiculoError($"Error: Matricula [{MatriculaVehiculo}] con los datos actualizados asociados no coincide con la mátricula del vehiculo que se quiere actualizar[{Key}] ");

    /// <summary>
    /// Error que se devuelve cuando un propietario alcanza el límite máximo de 3 vehículos permitidos.
    /// </summary>
    public sealed record OwnerWithThreeOrMoreVehiculos(string Dni)
        : VehiculoError($"Error: En el concesionario el dni [{Dni}] ya tiene asociados 3 vehiculos");
    
};