using System;
using System.Collections.Generic;
using System.Linq;
using ITV.Dto;
using ITV.Entity;
using ITV.Models;

namespace ITV.Mappers;

public static class CitaMapper
{
    // Formato ISO 8601 estándar (yyyy-MM-ddTHH:mm:ss)
    private static readonly string _isoFormat = "s";

    /// <summary>
    /// Transforma un <see cref="Cita"/> en un <see cref="CitaDto"/>
    /// </summary>
    public static CitaDto ToDto(this Cita cita)
    {
        return new CitaDto(
            cita.Id,
            cita.FechaMatriculacion.ToString(_isoFormat),
            cita.FechaInspeccion.ToString(_isoFormat),
            cita.Matricula, 
            cita.Marca,
            cita.Modelo,
            cita.Cilindrada,
            (int)cita.Motor,
            cita.DniDueño,
            cita.IsDeleted ? 1 : 0,
            cita.CreatedAt.ToString(_isoFormat),
            cita.UpdatedAt.ToString(_isoFormat)
        );
    }

    /// <summary>
    /// Transforma un <see cref="CitaDto"/> en un <see cref="Cita"/>
    /// </summary>
    public static Cita ToModel(this CitaDto dto)
    {
        return new Cita(
            dto.Id,
            DateTime.TryParse(dto.FechaMatriculacion, out var matriculacion) ? matriculacion : DateTime.Today,
            DateTime.TryParse(dto.FechaInspeccion, out var inspeccion) ? inspeccion : DateTime.Today,
            dto.Matricula,
            dto.Marca,
            dto.Modelo,
            dto.Cilindrada,
            Enum.IsDefined(typeof(Motor), dto.Motor) ? (Motor)dto.Motor : Motor.Gasolina,
            dto.DniDueño,
            dto.IsDeleted == 1,
            DateTime.TryParse(dto.CreatedAt, out var creado) ? creado : DateTime.Now,
            DateTime.TryParse(dto.UpdatedAt, out var actualizado) ? actualizado : DateTime.Now
        );
    }
    
    /// <summary>
    /// Transforma un <see cref="Cita"/> en un <see cref="CitaEntity"/> para Dapper
    /// </summary>
    public static CitaEntity ToEntity(this Cita cita)
    {
        return new CitaEntity(
            cita.Id,
            cita.FechaMatriculacion.ToString(_isoFormat),
            cita.FechaInspeccion.ToString(_isoFormat),
            cita.Matricula,
            cita.Marca,   // CORREGIDO: Antes tenías aquí el Modelo
            cita.Modelo,  // CORREGIDO: Antes tenías aquí la Marca
            cita.Cilindrada,
            (int)cita.Motor,
            cita.DniDueño,
            cita.IsDeleted ? 1 : 0,
            cita.CreatedAt.ToString(_isoFormat),
            cita.UpdatedAt.ToString(_isoFormat)
        );
    }
    
    /// <summary>
    /// Transforma un <see cref="CitaEntity"/> de Dapper en un <see cref="Cita"/>
    /// </summary>
    public static Cita ToModel(this CitaEntity entity)
    {
        return new Cita(
            entity.Id,
            DateTime.TryParse(entity.FechaMatriculacion, out var matriculacion) ? matriculacion : DateTime.Today,
            DateTime.TryParse(entity.FechaInspeccion, out var inspeccion) ? inspeccion : DateTime.Today,
            entity.Matricula,
            entity.Marca,   // CORREGIDO: Antes tenías aquí el Modelo
            entity.Modelo,  // CORREGIDO: Antes tenías aquí la Marca
            entity.Cilindrada,
            Enum.IsDefined(typeof(Motor), entity.Motor) ? (Motor)entity.Motor : Motor.Gasolina, // CORREGIDO: Casteo seguro con fallback
            entity.DniDueño,
            entity.IsDeleted == 1,
            DateTime.TryParse(entity.CreatedAt, out var creado) ? creado : DateTime.Now,
            DateTime.TryParse(entity.UpdatedAt, out var actualizado) ? actualizado : DateTime.Now
        );
    }
    
    /// <summary>
    /// Convierte una lista de entidades a modelos de dominio.
    /// </summary>
    public static IEnumerable<Cita> ToModel(this IEnumerable<CitaEntity> entities)
    {
        return entities.Select(ToModel);
    }
}