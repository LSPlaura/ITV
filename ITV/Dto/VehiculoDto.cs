using System.Xml.Serialization;

namespace ITV.Dto;

/// <summary>
///Objeto de transferencia de datos para Vehículo
/// </summary>
[XmlRoot("Concesionario")]
[XmlType("Vehiculo")]
public record VehiculoDto
{
    [XmlAttribute("id")]
    public int Id { get; init; }

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

    [XmlElement("borrado")]
    public int IsDeleted { get; init; }
    
    /// <summary>
    /// Constructor para usarlo con el mapper
    /// </summary>
    public VehiculoDto(
            int id,
            string matricula,
            string marca,
            string modelo,
            double cilindrada,
            int motor,
            string dniDueño,
            int isDeleted
        )
    {
        Id = id;
        Matricula = matricula;
        Marca = marca;
        Modelo = modelo;
        Cilindrada = cilindrada;
        Motor = motor;
        DniDueño = dniDueño;
        IsDeleted = isDeleted;
    }
    
    public VehiculoDto(){}
   
}