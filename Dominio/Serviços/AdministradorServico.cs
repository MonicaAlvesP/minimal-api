using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Infraestrutura.Db;
using minimal_api.Infraestrutura.Interfaces;

namespace minimal_api.Dominio.Serviços
{
  public class AdministradorServico(DbContexto contexto) : IAdministradorServico
  {
    private readonly DbContexto _contexto = contexto;

    public Administrador BuscaPorId(int id)
    {
      return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault()!;
    }

    public Administrador Incluir(Administrador administrador)
    {
      _contexto.Administradores.Add(administrador);
      _contexto.SaveChanges();

      return administrador;
    }

    public Administrador Login(LoginDTO loginDTO)
    {
      var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
      if (adm == null)
      {
        throw new Exception("Administrador não encontrado.");
      }
      return adm;
    }

    public List<Administrador> Todos(int? pagina)
    {
      var query = _contexto.Administradores.AsQueryable();

      int itensPorPagina = 10;

      if (pagina != null)
        query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

      return query.ToList();
    }
  }
}