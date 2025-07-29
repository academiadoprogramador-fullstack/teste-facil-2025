using TesteFacil.Dominio.ModuloQuestao;
using TesteFacil.Infrastructure.AI.Gemini;

namespace TesteFacil.WebApp.DependencyInjection;

public static class GeminiChatConfig
{
    public static void AddGeminiChatConfig(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IGeradorQuestoes, GeradorQuestoesGemini>();
    }
}
