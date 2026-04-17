namespace ITV.Models;

/// <summary>
/// Define los datos de un Vehículo
/// </summary>
public record Vehiculo
{
    public int Id { get; init; }
    public string Matricula { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public double Cilindrada { get; init; }
    public Motor Motor { get; init; }
    public string DniDueño { get; init; } = string.Empty;
    public bool IsDeleted { get; init; } 

    public Vehiculo(){}
    /// <summary>
    /// Constructor para usarlo con el mapper
    /// </summary>
    public Vehiculo(
        int id,
        string matricula,
        string marca,
        string modelo,
        double cilindrada,
        Motor motor,
        string dniDueño,
        bool isDeleted
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
    /// Constructor para instanciar
    /// </summary>
    public Vehiculo(
        string matricula,
        string marca,
        string modelo,
        double cilindrada,
        Motor motor,
        string dniDueño
        )
    {
        Matricula = matricula;
        Marca = marca;
        Modelo = modelo;
        Cilindrada = cilindrada;
        Motor = motor;
        DniDueño = dniDueño;
        IsDeleted = false;
    }
}