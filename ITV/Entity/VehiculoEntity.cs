using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ITV.Entity;
/// <summary>
/// Objeto de transferencia de datos para las bases de datos
/// </summary>
[Table("Vehiculo")]
[Index(nameof(Matricula), IsUnique = true)]
public class VehiculoEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

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

    [Required][MaxLength(9)]
    public string DniDueño { get; set; } = string.Empty;
    
    public int IsDeleted { get; set; }
    
    /// <summary>
    /// Constructor para usarlo con el mapper
    /// </summary>
    public VehiculoEntity(
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
    
    /// <summary>
    ///El constructor vacío que se necesita
    /// </summary>
    public VehiculoEntity(){}

}