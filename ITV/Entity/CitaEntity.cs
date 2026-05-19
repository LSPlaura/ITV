using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ITV.Entity;
/// <summary>
/// Objeto de transferencia de datos para las bases de datos
/// </summary>
[Table("Cita")]
[Index(nameof(Matricula), IsUnique = true)]
public class CitaEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public string FechaMatriculacion { get; set; } = string.Empty;
    
    [Required]
    public string FechaInspeccion { get; set; } = string.Empty;
    
    [Required]
    public string Matricula { get; set; } = string.Empty;

    [Required]
    public string Marca { get; set; } = string.Empty;

    [Required]
    public string Modelo { get; set; } = string.Empty;

    [Required]
    public double Cilindrada { get; set; }

    [Required]
    public int Motor { get; set; }

    [Required][MaxLength(9)] [Column("DniDueno")]
    public string DniDueño { get; set; } = string.Empty;
    
    public int IsDeleted { get; set; }
    
    public string CreatedAt { get; init; } = string.Empty;
    
    public string UpdatedAt { get; set; } = string.Empty;
    
    /// <summary>
    /// Constructor para usarlo con el mapper
    /// </summary>
    public CitaEntity(
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
    
    /// <summary>
    ///El constructor vacío que se necesita
    /// </summary>
    public CitaEntity(){}

}