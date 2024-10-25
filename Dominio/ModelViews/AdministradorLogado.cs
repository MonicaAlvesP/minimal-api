namespace minimal_api.Dominio.ModelViews;

public record AdministradorLogado
{
  public string Token { get; init; } = default!;
  public string Email { get; init; } = default!;
  public string Perfil { get; init; } = default!;
}