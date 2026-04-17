using System;
using System.Globalization;
using ITV.Dto;
using ITV.Entity;
using ITV.Models;
namespace ITV.Mappers;

public static class VehiculoMapper
{
    /// <summary>
    /// Transforma un <see cref="Vehiculo"/> en un <see cref="VehiculoDto"/> con InvariantCulture
    /// </summary>
    /// <param name="vehiculo">La instancia que se quiere transformar</param>
    /// <returns>La instancia transformada</returns>
    public static VehiculoDto ToDto(this Vehiculo vehiculo)
    {
        return new VehiculoDto(
            vehiculo.Id,
            vehiculo.Matricula, 
            vehiculo.Marca,
            vehiculo.Modelo,
            vehiculo.Cilindrada,
            (int)vehiculo.Motor,
            vehiculo.DniDueño,
            vehiculo.IsDeleted? 1 : 0
        );
    }

    /// <summary>
    /// Transforma un <see cref="VehiculoDto"/> en un <see cref="Vehiculo"/>
    /// </summary>
    /// <param name="dto">La instancia que se quiere transformar</param>
    /// <returns>La instancia transformada</returns>
    /// <exception cref="ArgumentException">Si no se puede parsear un dato es porque se introdujo un dato del tipo incorrecto</exception>
    public static Vehiculo ToModel(this VehiculoDto dto)
    {
        return new Vehiculo(
            dto.Id,
            dto.Matricula, 
            dto.Marca,
            dto.Modelo,
            dto.Cilindrada,
            Enum.IsDefined(typeof(Motor), dto.Motor) ? (Motor)dto.Motor : Motor.Gasolina,
            dto.DniDueño,
            dto.IsDeleted == 1
        );
    }
    
    /// <summary>
    /// Transforma un <see cref="Vehiculo"/> en un <see cref="VehiculoEntity"/> con InvariantCulture
    /// </summary>
    /// <param name="vehiculo">La instancia que se quiere transformar</param>
    /// <returns>La instancia transformada</returns>
    public static VehiculoEntity ToEntity(this Vehiculo vehiculo)
    {
        return new VehiculoEntity(
            vehiculo.Id,
            vehiculo.Matricula,
            vehiculo.Modelo,
            vehiculo.Marca,
            vehiculo.Cilindrada,
            (int)vehiculo.Motor,
            vehiculo.DniDueño,
            vehiculo.IsDeleted ? 1 : 0 
        );
    }
    
    /// <summary>
    /// Transforma un <see cref="Vehiculo"/> en un <see cref="VehiculoEntity"/>
    /// </summary>
    /// <param name="entity">La instancia que se quiere transformar</param>
    /// <returns>La instancia transformada</returns>
    public static Vehiculo ToModel(this VehiculoEntity entity)
    {
        return new Vehiculo(
            entity.Id,
            entity.Matricula,
            entity.Modelo,
            entity.Marca,
            entity.Cilindrada,
            (Motor)entity.Motor,
            entity.DniDueño,
            entity.IsDeleted == 1
        );
    }
    
    /// <summary>
    /// Convierte una lista de entidades a modelos de dominio.
    /// </summary>
    public static IEnumerable<Vehiculo> ToModel(this IEnumerable<VehiculoEntity> entities)
    {
        return entities.Select(ToModel).OfType<Vehiculo>();
    }
}