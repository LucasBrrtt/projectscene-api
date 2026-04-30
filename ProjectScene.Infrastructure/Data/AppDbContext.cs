using Microsoft.EntityFrameworkCore;
using ProjectScene.Domain.Entities;

namespace ProjectScene.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Expõe a tabela de usuários para consultas e persistência via EF Core.
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automaticamente os mapeamentos encontrados na infraestrutura.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
