using System.ComponentModel.DataAnnotations;

namespace minimal_api.Dominio.Entidades
{
  public class Veiculo
  {
    public int Id { get; set; } = default!;
    [StringLength(150)]
    [Required]
    public string Nome { get; set; } = default!;
    [StringLength(100)]
    [Required]
    public string Marca { get; set; } = default!;
    [Required]
    public int Ano { get; set; } = default!;
  }
}