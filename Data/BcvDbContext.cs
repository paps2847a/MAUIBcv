using Microsoft.EntityFrameworkCore;
using BcvExchangeApp.Models;
using System.IO;
using Microsoft.Maui.Storage;

namespace BcvExchangeApp.Data;

public class BcvDbContext : DbContext
{
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    public BcvDbContext()
    {
        // Constructor sin parámetros requerido por algunas herramientas
    }

    public BcvDbContext(DbContextOptions<BcvDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Ubicación ideal y segura en el almacenamiento local del dispositivo móvil/escritorio
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "bcv_rates.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Opcional: configurar restricciones o índices
        modelBuilder.Entity<ExchangeRate>()
            .HasIndex(e => e.Date)
            .IsUnique(); // Aseguramos que solo haya un registro de tasas por día valor
    }
}
