using System.Xml.Serialization;

namespace ITV.Dto;

/// <summary>
///Objeto de transferencia de datos para Vehículo
/// </summary>
[XmlRoot("Concesionario")]
[XmlType("Cita")]
public record CitaDto
{
    [XmlAttribute("id")]
    public int Id { get; init; }
    
    [XmlElement("fecha_matriculacion")]
    public string FechaMatriculacion { get; set; } = string.Empty;
    
    [XmlElement("fecha_inspeccion")]
    public string FechaInspeccion { get; init; } = string.Empty;
    
    [XmlElement("matricula")]
    public string Matricula { get; init; } = string.Empty;

    [XmlElement("marca")]
    public string Marca { get; init; } = string.Empty;

    [XmlElement("modelo")]
    public string Modelo { get; init; } = string.Empty;

    [XmlElement("cilindrada")]
    public double Cilindrada { get; init; }

    [XmlElement("motor")]
    public int Motor { get; init; }

    [XmlElement("dni_dueno")]
    public string DniDueño { get; init; } = string.Empty;

    [XmlElement("isDeleted")]
    public int IsDeleted { get; init; }

    [XmlElement("created_at")]
    public string CreatedAt { get; init; } = string.Empty;
    
    [XmlElement("updated_at")]
    public string UpdatedAt { get; init; } = string.Empty;
    
    
    /// <summary>
    /// Constructor para usarlo con el mapper
    /// </summary>
    public CitaDto(
        int id,
        string fechaMatriculacion,
        string fechaInsepccion,
        string matricula,
        string marca,
        string modelo,
        double cilindrada,
        int motor,
        string dniDueño,
        int isDeleted, 
        string createdAt, 
        string updatedAt
        )
    {
        Id = id;
        FechaMatriculacion = fechaMatriculacion;
        FechaInspeccion = fechaInsepccion;
        Matricula = matricula;
        Marca = marca;
        Modelo = modelo;
        Cilindrada = cilindrada;
        Motor = motor;
        DniDueño = dniDueño;
        IsDeleted = isDeleted;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public CitaDto(){}
}