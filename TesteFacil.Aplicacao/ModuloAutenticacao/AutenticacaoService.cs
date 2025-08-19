using FluentResults;
using Microsoft.AspNetCore.Identity;
using TesteFacil.Aplicacao.Compartilhado;
using TesteFacil.Dominio.ModuloAutenticacao;

namespace TesteFacil.Aplicacao.ModuloAutenticacao;

public class AutenticacaoService
{
    private readonly UserManager<Usuario> userManager;
    private readonly SignInManager<Usuario> signInManager;

    public AutenticacaoService(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager
    )
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public async Task<Result> Cadastrar(Usuario usuario, string senha)
    {
        var usuarioResult = await userManager.CreateAsync(usuario, senha);

        if (!usuarioResult.Succeeded)
        {
            var erros = usuarioResult
                .Errors
                .Select(failure => failure.Description)
                .ToList();

            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        var resultadoLogin = await signInManager.PasswordSignInAsync(
            usuario.Email,
            senha,
            false,
            false
        );

        if (!resultadoLogin.Succeeded)
            return Result.Fail("Login ou senha incorretos.");

        return Result.Ok();
    }

    public async Task<Result> Autenticar(string email, string senha)
    {
        var resultadoLogin = await signInManager.PasswordSignInAsync(
            email,
            senha,
            false,
            false
        );

        if (!resultadoLogin.Succeeded)
            return Result.Fail("Login ou senha incorretos.");
        
        return Result.Ok();
    }

    public async Task<Result> Logout()
    {
        await signInManager.SignOutAsync();

        return Result.Ok();
    }
}
