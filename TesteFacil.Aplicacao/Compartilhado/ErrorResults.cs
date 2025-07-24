using FluentResults;

namespace TesteFacil.Aplicacao.Compartilhado;

public abstract class ResultadosErro
{
    public static Error RegistroDuplicadoErro(string mensagemErro)
    {
        return new Error("Registro duplicado")
            .CausedBy(mensagemErro)
            .WithMetadata("TipoErro", "RegistroDuplicado");
    }

    public static Error ExcecaoInternaErro(Exception ex)
    {
        return new Error("Ocorreu um erro interno do servidor")
            .CausedBy(ex)
            .WithMetadata("TipoErro", "ExcecaoInterna");
    }
}
