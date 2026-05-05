namespace ITV.Models;

/// <summary>
/// Define los datos de un Vehículo
/// </summary>
public record Cita
{
    public int Id { get; init; }
    public DateTime FechaMatriculacion { get; init; }
    public DateTime? FechaInspeccion { get; init; } = null;
    public string Matricula { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public double Cilindrada { get; init; }
    public Motor Motor { get; init; }
    public string DniDueño { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime UpdatedAt { get; init; } = DateTime.Now;
    public bool IsDeleted { get; init; } 

    public Cita(){}
    /// <summary>
    /// Constructor para usarlo con el mapper
    /// </summary>
    public Cita(
        int id,
        DateTime fechaMatriculacion,
        DateTime? fechaInspeccion,
        string matricula,
        string marca,
        string modelo,
        double cilindrada,
        Motor motor,
        string dniDueño,
        bool isDeleted, 
        DateTime createdAt, 
        DateTime updatedAt
        )
    {
        Id = id;
        FechaMatriculacion = fechaMatriculacion;
        FechaInspeccion = fechaInspeccion;
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
    /// Constructor para instanciar
    /// </summary>
    public Cita(
        DateTime? fechaInspeccion,
        string matricula,
        string marca,
        string modelo,
        double cilindrada,
        Motor motor,
        string dniDueño
        )
    {
        FechaInspeccion = fechaInspeccion;
        Matricula = matricula;
        Marca = marca;
        Modelo = modelo;
        Cilindrada = cilindrada;
        Motor = motor;
        DniDueño = dniDueño;
    }
}