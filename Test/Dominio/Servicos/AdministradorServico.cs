using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Serviços;

namespace Test.Dominio.Servicos
{
  [TestClass]
  public class AdministradorServicoTest
  {
    [TestMethod]
    public void TestarPersistenciaAdministrador()
    {
      // Arrange
      var administradorServico = new AdministradorServico();
      var administrador = new Administrador
      {
        Id = 1,
        Email = "teste@teste.com",
        Senha = "1234567",
        Perfil = "Adm",
      };

      // Act
      administradorServico.BuscaPorId(administrador);
      var administradorPersistido = administradorServico.BuscaPorId(1);

      // Assert
      Assert.AreEqual(1, administradorPersistido.Id);
      Assert.AreEqual("teste@teste.com", administradorPersistido.Email);
      Assert.AreEqual("1234567", administradorPersistido.Senha);
      Assert.AreEqual("Adm", administradorPersistido.Perfil);
    }
  }
}