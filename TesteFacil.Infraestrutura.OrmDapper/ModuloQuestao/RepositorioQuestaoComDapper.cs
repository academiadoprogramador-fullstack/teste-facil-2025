using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using TesteFacil.Dominio.ModuloQuestao;
using TesteFacil.Dominio.ModuloMateria;

namespace TesteFacil.Infraestrutura.OrmDapper.ModuloQuestao
{
    public class RepositorioQuestaoComDapper : IRepositorioQuestao
    {
        private readonly IDbConnection dbConnection;

        public RepositorioQuestaoComDapper(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        #region SQL queries
        private const string sqlInserir =
            @"INSERT INTO [TBQUESTAO]
                ([ID], [ENUNCIADO], [UTILIZADAEMTESTE], [MATERIAID])
              VALUES
                (@ID, @ENUNCIADO, @UTILIZADAEMTESTE, @MATERIAID);";

        private const string sqlEditar =
            @"UPDATE [TBQUESTAO]
                SET [ENUNCIADO] = @ENUNCIADO,
                    [UTILIZADAEMTESTE] = @UTILIZADAEMTESTE,
                    [MATERIAID] = @MATERIAID
              WHERE [ID] = @ID;";

        private const string sqlExcluir =
            @"DELETE FROM [TBQUESTAO]
              WHERE [ID] = @ID;";

        private const string sqlSelecionarTodos =
            @"SELECT q.[ID], q.[ENUNCIADO], q.[UTILIZADAEMTESTE], q.[MATERIAID],
                     m.[ID] AS MateriaId, m.[NOME], m.[SERIE], m.[DISCIPLINAID]
              FROM [TBQUESTAO] q
              INNER JOIN [TBMATERIA] m ON q.[MATERIAID] = m.[ID];";

        private const string sqlSelecionarPorId =
            @"SELECT q.[ID], q.[ENUNCIADO], q.[UTILIZADAEMTESTE], q.[MATERIAID],
                     m.[ID] AS MateriaId, m.[NOME], m.[SERIE], m.[DISCIPLINAID]
              FROM [TBQUESTAO] q
              INNER JOIN [TBMATERIA] m ON q.[MATERIAID] = m.[ID]
              WHERE q.[ID] = @ID;";

        private const string sqlSelecionarAlternativas =
            @"SELECT [ID], [LETRA], [RESPOSTA], [CORRETA], [QUESTAOID]
              FROM [TBALTERNATIVA]
              WHERE [QUESTAOID] = @QUESTAOID;";

        private const string sqlSelecionarQuestoesPorDisciplinaESerie =
            @"SELECT q.[ID], q.[ENUNCIADO], q.[UTILIZADAEMTESTE], q.[MATERIAID],
                     m.[ID] AS MateriaId, m.[NOME], m.[SERIE], m.[DISCIPLINAID]
              FROM [TBQUESTAO] q
              INNER JOIN [TBMATERIA] m ON q.[MATERIAID] = m.[ID]
              WHERE m.[DISCIPLINAID] = @DISCIPLINAID AND m.[SERIE] = @SERIE";

        private const string sqlSelecionarQuestoesPorMateria =
            @"SELECT q.[ID], q.[ENUNCIADO], q.[UTILIZADAEMTESTE], q.[MATERIAID],
                     m.[ID] AS MateriaId, m.[NOME], m.[SERIE], m.[DISCIPLINAID]
              FROM [TBQUESTAO] q
              INNER JOIN [TBMATERIA] m ON q.[MATERIAID] = m.[ID]
              WHERE m.[ID] = @MATERIAID";
        #endregion

        public void Cadastrar(Questao novaQuestao)
        {
            if (novaQuestao.Id == Guid.Empty)
                novaQuestao.Id = Guid.NewGuid();

            dbConnection.Execute(sqlInserir, new
            {
                ID = novaQuestao.Id,
                ENUNCIADO = novaQuestao.Enunciado,
                UTILIZADAEMTESTE = novaQuestao.UtilizadaEmTeste,
                MATERIAID = novaQuestao.Materia.Id
            });

            foreach (var alternativa in novaQuestao.Alternativas)
            {
                alternativa.Questao = novaQuestao;
                CadastrarAlternativa(alternativa);
            }
        }

        public bool Editar(Guid idRegistro, Questao registroEditado)
        {
            var linhasAfetadas = dbConnection.Execute(sqlEditar, new
            {
                ID = idRegistro,
                ENUNCIADO = registroEditado.Enunciado,
                UTILIZADAEMTESTE = registroEditado.UtilizadaEmTeste,
                MATERIAID = registroEditado.Materia.Id
            });

            // Exclui alternativas antigas e insere as novas
            dbConnection.Execute("DELETE FROM [TBALTERNATIVA] WHERE [QUESTAOID] = @QUESTAOID", new { QUESTAOID = idRegistro });
            foreach (var alternativa in registroEditado.Alternativas)
            {
                alternativa.Questao = registroEditado;
                CadastrarAlternativa(alternativa);
            }

            return linhasAfetadas > 0;
        }

        public bool Excluir(Guid idRegistro)
        {
            dbConnection.Execute("DELETE FROM [TBALTERNATIVA] WHERE [QUESTAOID] = @QUESTAOID", new { QUESTAOID = idRegistro });
            var linhasAfetadas = dbConnection.Execute(sqlExcluir, new { ID = idRegistro });
            return linhasAfetadas > 0;
        }

        public List<Questao> SelecionarRegistros()
        {
            var questoes = dbConnection.Query<Questao, Materia, Questao>(
                sqlSelecionarTodos,
                (questao, materia) =>
                {
                    materia.Id = questao.MateriaId;
                    questao.Materia = materia;
                    return questao;
                },
                splitOn: "MateriaId"
            ).ToList();

            foreach (var questao in questoes)
            {
                questao.Alternativas = SelecionarAlternativas(questao.Id);
            }

            return questoes;
        }

        public Questao? SelecionarRegistroPorId(Guid idRegistro)
        {
            var questao = dbConnection.Query<Questao, Materia, Questao>(
                sqlSelecionarPorId,
                (q, m) =>
                {
                    m.Id = q.MateriaId;
                    q.Materia = m;
                       
                    return q;
                },
                new { ID = idRegistro },
                splitOn: "MateriaId"
            ).FirstOrDefault();

            if (questao != null)
                questao.Alternativas = SelecionarAlternativas(questao.Id);

            return questao;
        }

        public List<Questao> SelecionarQuestoesPorDisciplinaESerie(Guid disciplinaId, SerieMateria serie, int quantidadeQuestoes)
        {
            var questoes = dbConnection.Query<Questao, Materia, Questao>(
                sqlSelecionarQuestoesPorDisciplinaESerie,
                (q, m) =>
                {
                    m.Id = q.MateriaId;
                    q.Materia = m;
                    return q;
                },
                new { DISCIPLINAID = disciplinaId, SERIE = (int)serie, QUANTIDADE = quantidadeQuestoes },
                splitOn: "MateriaId"
            ).ToList();

            foreach (var questao in questoes)
            {
                questao.Alternativas = SelecionarAlternativas(questao.Id);
            }

            return questoes;
        }

        public List<Questao> SelecionarQuestoesPorMateria(Guid materiaId, int quantidadeQuestoes)
        {
            var questoes = dbConnection.Query<Questao, Materia, Questao>(
                sqlSelecionarQuestoesPorMateria,
                (q, m) =>
                {
                    m.Id = q.MateriaId;
                    q.Materia = m;
                    return q;
                },
                new { MATERIAID = materiaId, QUANTIDADE = quantidadeQuestoes },
                splitOn: "MateriaId"
            ).ToList();

            foreach (var questao in questoes)
            {
                questao.Alternativas = SelecionarAlternativas(questao.Id);
            }

            return questoes;
        }

        private void CadastrarAlternativa(Alternativa alternativa)
        {
            dbConnection.Execute(
                @"INSERT INTO [TBALTERNATIVA]
                    ([ID], [LETRA], [RESPOSTA], [CORRETA], [QUESTAOID])
                  VALUES
                    (@ID, @LETRA, @RESPOSTA, @CORRETA, @QUESTAOID);",
                new
                {
                    ID = alternativa.Id == Guid.Empty ? Guid.NewGuid() : alternativa.Id,
                    LETRA = alternativa.Letra,
                    RESPOSTA = alternativa.Resposta,
                    CORRETA = alternativa.Correta,
                    QUESTAOID = alternativa.Questao.Id
                }
            );
        }

        private List<Alternativa> SelecionarAlternativas(Guid questaoId)
        {
            return dbConnection.Query<Alternativa>(
                sqlSelecionarAlternativas,
                new { QUESTAOID = questaoId }
            ).ToList();
        }
    }
}
