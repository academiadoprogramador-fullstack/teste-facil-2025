using Microsoft.AspNetCore.Identity;

namespace TesteFacil.Dominio.ModuloAutenticacao;

public class Usuario : IdentityUser<Guid>
{
    public Usuario()
    {
        Id = Guid.NewGuid();
        EmailConfirmed = true;
    }
}

public class InformacoesUsuario
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
}