using Microsoft.EntityFrameworkCore;

namespace ITV.Entity;

/// <summary>
/// Mapper para Efcore que establece las tablas que la base de datos usa y la conexión con la base de datos
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<VehiculoEntity> Vehiculo {get; set; } = null!;
    private readonly string _connection;
    
    public AppDbContext(string connecion)
    {
        _connection = connecion;
    }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        _connection = "";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlite(_connection);
    }
    public void EnsureCreated()
    {
        Database.EnsureCreated();
    }
}