using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Infraestrutura.Db
{
  public class DbContexto : DbContext
  {
    private readonly IConfiguration _configurationAppSettings;

    public DbContexto(IConfiguration configurationAppSettings)
    {
      _configurationAppSettings = configurationAppSettings;
    }

    public DbSet<Administrador> Administradores { get; set; } = default!;
    public DbSet<Veiculo> Veiculos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Administrador>().HasData(
        new Administrador
        {
          Id = 1,
          Email = "administrador@teste.com",
          Senha = "123456",
          Perfil = "Adm"
        }
      );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      if (!optionsBuilder.IsConfigured)
      {
        var stringConexao = _configurationAppSettings.GetConnectionString("SqlServer")?.ToString();
        if (!string.IsNullOrEmpty(stringConexao))
          optionsBuilder.UseSqlServer(stringConexao);
      }
    }
  }
}