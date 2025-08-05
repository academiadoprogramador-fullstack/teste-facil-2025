using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.Dominio.ModuloQuestao;
using TesteFacil.Dominio.ModuloTeste;

namespace TesteFacil.Testes.Unidade.ModuloTeste;

[TestClass]
[TestCategory("Testes de Unidade de Teste")]
public sealed class TesteDomainTests
{
    private Teste? teste;

    private readonly Disciplina disciplinaMatematica;
    private readonly Materia materiaQuatroOperacoes;

    public TesteDomainTests()
    {
        disciplinaMatematica = new Disciplina("Matemática");

        materiaQuatroOperacoes = new Materia(
            "Quatro Operações",
            SerieMateria.SegundaSerie,
            disciplinaMatematica
            );

        materiaQuatroOperacoes.AdicionarQuestoes([
            new Questao("Quanto é 2 + 2?", materiaQuatroOperacoes),
            new Questao("Quanto é 53 + 38?", materiaQuatroOperacoes),
            new Questao("Quanto é 985 + 15?", materiaQuatroOperacoes),
            new Questao("Quanto é 9 / 3?", materiaQuatroOperacoes),
            new Questao("Quanto é 30 * 15?", materiaQuatroOperacoes)
        ]);

        disciplinaMatematica.AdicionarMateria(materiaQuatroOperacoes);
    }

    [TestMethod]
    public void Deve_AdicionarQuestao_AoTeste_Corretamente()
    {
        // Arrange
        teste = new Teste(
            titulo: "Teste de Matemática",
            recuperacao: false,
            quantidadeQuestoes: 5,
            serieMateria: SerieMateria.SegundaSerie,
            disciplina: disciplinaMatematica,
            materia: materiaQuatroOperacoes
        );

        var questao = new Questao("Quanto é 2 + 2?", materiaQuatroOperacoes);

        // Act
        teste.AdicionarQuestao(questao);

        // Assert
        Assert.IsTrue(questao.UtilizadaEmTeste);
    }

    [TestMethod]
    public void Deve_RemoverQuestao_DoTeste_Corretamente()
    {
        // Arrange
        teste = new Teste(
            titulo: "Teste de Matemática",
            recuperacao: false,
            quantidadeQuestoes: 5,
            serieMateria: SerieMateria.SegundaSerie,
            disciplina: disciplinaMatematica,
            materia: materiaQuatroOperacoes
        );

        var questao = new Questao("Quanto é 2 + 2?", materiaQuatroOperacoes);

        teste.AdicionarQuestao(questao);

        // Act
        teste.RemoverQuestao(questao);

        // Assert
        Assert.IsFalse(questao.UtilizadaEmTeste);
    }

    [TestMethod]
    public void Deve_RemoverQuestoes_DoTeste_Corretamente()
    {
        // Arrange
        teste = new Teste(
            titulo: "Teste de Matemática",
            recuperacao: false,
            quantidadeQuestoes: 5,
            serieMateria: SerieMateria.SegundaSerie,
            disciplina: disciplinaMatematica,
            materia: materiaQuatroOperacoes
        );

        teste.SortearQuestoes();

        // Act
        teste.RemoverQuestoesAtuais();

        // Assert
        var quantidadeQuestoesAtuais = teste.Questoes.Count;

        Assert.AreEqual(0, quantidadeQuestoesAtuais);
    }

    [TestMethod]
    public void Deve_SortearQuestoes_DoTeste_ComMateria()
    {
        // Arrange
        teste = new Teste(
            titulo: "Teste de Matemática",
            recuperacao: false,
            quantidadeQuestoes: 5,
            serieMateria: SerieMateria.SegundaSerie,
            disciplina: disciplinaMatematica,
            materia: materiaQuatroOperacoes
        );

        // Act
        var questoesSorteadas = teste.SortearQuestoes();

        // Assert
        CollectionAssert.AreNotEqual(materiaQuatroOperacoes.Questoes, questoesSorteadas);
        CollectionAssert.IsSubsetOf(questoesSorteadas, materiaQuatroOperacoes.Questoes);
    }

    [TestMethod]
    public void Deve_SortearQuestoes_DoTeste_DeRecuperacao()
    {
        // Arrange
        var materiaFracao = new Materia("Frações", SerieMateria.SegundaSerie, disciplinaMatematica);

        materiaFracao.AdicionarQuestoes([
            new Questao("Qual é a fração que representa a metade de uma pizza?", materiaFracao),
            new Questao("Qual fração representa três partes de um total de quatro partes iguais?", materiaFracao),
            new Questao("Qual é o resultado da soma: 1/4 + 1/4?", materiaFracao),
            new Questao("Qual é a fração equivalente a 2/4?", materiaFracao),
            new Questao("Se você tem uma barra de chocolate dividida em 8 pedaços e come 3, qual fração representa o que você comeu?", materiaFracao)
        ]);

        disciplinaMatematica.AdicionarMateria(materiaFracao);

        teste = new Teste(
            titulo: "Teste de Matemática",
            recuperacao: true,
            quantidadeQuestoes: 5,
            serieMateria: SerieMateria.SegundaSerie,
            disciplina: disciplinaMatematica,
            materia: null
        );

        // Act
        var questoesSorteadas = teste.SortearQuestoes();

        // Assert
        List<Questao> todasQuestoesEsperadas = [.. materiaQuatroOperacoes.Questoes,.. materiaFracao.Questoes];

        Assert.IsNotNull(questoesSorteadas);
        Assert.AreEqual(5, questoesSorteadas.Count);
        CollectionAssert.IsSubsetOf(questoesSorteadas, todasQuestoesEsperadas);
    }
}