using System.Security.Claims;
using TesteFacil.Dominio.ModuloAutenticacao;

namespace TesteFacil.WebApp.Identity;

public class TenantProvider(IHttpContextAccessor contextAccessor) : ITenantProvider
{
    public Guid? UsuarioId
    {
        get
        {
            var claimId = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

            if (claimId is null)
                return null;

            return Guid.Parse(claimId.Value);
        }
    }
}