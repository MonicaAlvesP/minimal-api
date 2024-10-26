using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enuns;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Serviços;
using minimal_api.Infraestrutura.Db;
using minimal_api.Infraestrutura.Interfaces;

#region Builder
var builder = WebApplication.CreateBuilder(args);
;
var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key))
  key = "123456";

builder.Services.AddAuthentication(option =>
{
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
  option.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true
  };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Insira aqui o seu Token JWT: "
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
  {
    new OpenApiSecurityScheme
    {
      Reference = new OpenApiReference
      {
        Type = ReferenceType.SecurityScheme,
        Id = "Bearer"
      }
    },
    new string[]{}
  }
});

});

builder.Services.AddDbContext<DbContexto>(option => option.UseSqlServer(
  builder.Configuration.GetConnectionString("SqlServer")
));

var app = builder.Build();
#endregion

app.UseHttpsRedirection();

#region Home
app.MapGet("/", () => Results.Json(new Home()))
.AllowAnonymous()
.WithTags("Home");
#endregion

#region Administradores

string GerarTokenTwt(Administrador administrador)
{
  if (string.IsNullOrEmpty(key)) return string.Empty;
  var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
  var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

  var claims = new List<Claim>()
  {
    new Claim("Email", administrador.Email),
    new Claim("Perfil", administrador.Perfil),
    new Claim(ClaimTypes.Role, administrador.Perfil)
  };

  var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.Now.AddDays(1),
    signingCredentials: credentials
  );

  return new JwtSecurityTokenHandler().WriteToken(token);
}


app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  var adm = administradorServico.Login(loginDTO);
  if (adm != null)
  {
    string token = GerarTokenTwt(adm);
    return Results.Ok(new AdministradorLogado
    {
      Email = adm.Email,
      Perfil = adm.Perfil,
      Token = token
    });
  }
  else
  {
    return Results.Unauthorized();
  }
})
.AllowAnonymous()
.WithTags("Administradores");



app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
  var adms = new List<AdministradorModelView>();
  var administradores = administradorServico.Todos(pagina);
  foreach (var adm in administradores)
  {
    adms.Add(new AdministradorModelView
    {
      Id = adm.Id,
      Email = adm.Email,
      Perfil = adm.Perfil
    });
  }
  return Results.Ok(adms);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.RequireAuthorization()
.WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO AdministradorDTO, IAdministradorServico administradorServico) =>
{
  var validacao = new ErrosDeValidacao
  {
    Mensagens = new List<string>()
  };

  if (string.IsNullOrEmpty(AdministradorDTO.Email))
    validacao.Mensagens.Add("O email não pode ser vazio");
  if (string.IsNullOrEmpty(AdministradorDTO.Senha))
    validacao.Mensagens.Add("A senha não pode ser vazia");
  if (AdministradorDTO.Perfil == null)
    validacao.Mensagens.Add("Perfil não pode ser vazio");
  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  var administrador = new Administrador
  {
    Email = AdministradorDTO.Email,
    Senha = AdministradorDTO.Senha,
    Perfil = AdministradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
  };

  administradorServico.Incluir(administrador);
  return Results.Created($"/administradores/{administrador.Id}", new AdministradorModelView
  {
    Id = administrador.Id,
    Email = administrador.Email,
    Perfil = administrador.Perfil
  });
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.RequireAuthorization()
.WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.BuscaPorId(id);
  if (administrador == null) return Results.NotFound();
  return Results.Ok(new AdministradorModelView
  {
    Id = administrador.Id,
    Email = administrador.Email,
    Perfil = administrador.Perfil
  });
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");
#endregion

#region Veiculos

static ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
  var validacao = new ErrosDeValidacao
  {
    Mensagens = new List<string>()
  };

  if (string.IsNullOrWhiteSpace(veiculoDTO.Nome))
    validacao.Mensagens.Add("O nome não pode ser vazio");

  if (string.IsNullOrWhiteSpace(veiculoDTO.Marca))
    validacao.Mensagens.Add("A marca não pode ficar em branco");

  if (veiculoDTO.Ano < 1950)
    validacao.Mensagens.Add("Veiculo muito antigo, aceitavel a partir de 1950");

  return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  var validacao = validaDTO(veiculoDTO);
  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  var veiculo = new Veiculo
  {
    Nome = veiculoDTO.Nome,
    Marca = veiculoDTO.Marca,
    Ano = veiculoDTO.Ano
  };
  veiculoServico.Incluir(veiculo);
  return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.RequireAuthorization()
.WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
  Results.Ok(veiculoServico.Todos(pagina))
).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(id);
  return veiculo == null ? Results.NotFound() : Results.Ok(veiculo);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.RequireAuthorization()
.WithTags("Veiculos");

app.MapPut("/Veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(id);
  if (veiculo == null)
    return Results.NotFound();

  var validacao = validaDTO(veiculoDTO);
  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  veiculo.Nome = veiculoDTO.Nome;
  veiculo.Marca = veiculoDTO.Marca;
  veiculo.Ano = veiculoDTO.Ano;
  veiculoServico.Atualizar(veiculo);
  return Results.Ok(veiculo);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veiculos");

app.MapDelete("/Veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(id);
  if (veiculo == null)
    return Results.NotFound();

  veiculoServico.Apagar(veiculo);
  return Results.NoContent();
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion