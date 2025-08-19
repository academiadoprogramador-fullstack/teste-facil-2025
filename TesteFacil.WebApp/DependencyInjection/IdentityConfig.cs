using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using TesteFacil.Dominio.ModuloAutenticacao;
using TesteFacil.Infraestrutura.Orm.Compartilhado;
using TesteFacil.WebApp.Identity;

namespace TesteFacil.WebApp.DependencyInjection;

public static class IdentityConfig
{
    public static void AddIdentityProviderConfig(this IServiceCollection services)
    {
        services.AddScoped<ITenantProvider, TenantProvider>();

        services.AddIdentity<Usuario, Cargo>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<TesteFacilDbContext>()
        .AddDefaultTokenProviders();
    }

    public static void AddJwtAuthenticationConfig(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
          .AddCookie(options =>
          {
              options.Cookie.Name = "AspNetCore.Cookies";
              options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
              options.SlidingExpiration = true;
          });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Autenticacao/Login";
            options.AccessDeniedPath = "/Autenticacao/AcessoNegado";
        });
    }
}
