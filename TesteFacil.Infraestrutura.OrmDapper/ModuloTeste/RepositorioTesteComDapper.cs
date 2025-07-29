using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using TesteFacil.Dominio.ModuloTeste;
using TesteFacil.Dominio.ModuloQuestao;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.Dominio.ModuloDisciplina;

namespace TesteFacil.Infraestrutura.OrmDapper.ModuloTeste
{
    public class RepositorioTesteComDapper : IRepositorioTeste
    {
        private readonly IDbConnection dbConnection;

        public RepositorioTesteComDapper(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        #region SQL queries
        private const string sqlInserir =
            @"INSERT INTO [TBTESTE]
                ([ID], [TITULO], [DATAGERACAO], [RECUPERACAO], [DISCIPLINAID], [MATERIAID])
              VALUES
                (@ID, @TITULO, @DATAGERACAO, @RECUPERACAO, @DISCIPLINAID, @MATERIAID);";

        private const string sqlEditar =
            @"UPDATE [TBTESTE]
                SET [TITULO] = @TITULO,
                    [DATAGERACAO] = @DATAGERACAO,
                    [RECUPERACAO] = @RECUPERACAO,
                    [DISCIPLINAID] = @DISCIPLINAID,
                    [MATERIAID] = @MATERIAID
              WHERE [ID] = @ID;";

        private const string sqlExcluir =
            @"DELETE FROM [TBTESTE]
              WHERE [ID] = @ID;";

        private const string sqlSelecionarTodos =
            @"SELECT t.[ID], t.[TITULO], t.[DATAGERACAO], t.[RECUPERACAO], t.[DISCIPLINAID], t.[MATERIAID],
                     d.[ID] AS DisciplinaId, d.[NOME],
                     m.[ID] AS MateriaId, m.[NOME], m.[SERIE], m.[DISCIPLINAID] 
              FROM [TBTESTE] t
              LEFT JOIN [TBDISCIPLINA] d ON t.[DISCIPLINAID] = d.[ID]
              LEFT JOIN [TBMATERIA] m ON t.[MATERIAID] = m.[ID];";

        private const string sqlSelecionarPorId =
            @"SELECT t.[ID], t.[TITULO], t.[DATAGERACAO], t.[RECUPERACAO], t.[DISCIPLINAID], t.[MATERIAID],
                     d.[ID] AS DisciplinaId, d.[NOME],
                     m.[ID] AS MateriaId, m.[NOME], m.[SERIE], m.[DISCIPLINAID] 
              FROM [TBTESTE] t
              LEFT JOIN [TBDISCIPLINA] d ON t.[DISCIPLINAID] = d.[ID]
              LEFT JOIN [TBMATERIA] m ON t.[MATERIAID] = m.[ID]
              WHERE t.[ID] = @ID;";

        private const string sqlSelecionarQuestoesDoTeste =
            @"SELECT q.[ID], q.[ENUNCIADO], q.[UTILIZADAEMTESTE], q.[MATERIAID],
                     m.[ID] AS MateriaId, m.[NOME], m.[SERIE], m.[DISCIPLINAID] 
              FROM [TBQUESTAO] q
              INNER JOIN [TBTESTE_QUESTAO] tq ON tq.[QUESTAOID] = q.[ID]
              INNER JOIN [TBMATERIA] m ON q.[MATERIAID] = m.[ID]
              WHERE tq.[TESTEID] = @TESTEID;";

        private const string sqlSelecionarAlternativasDaQuestao =
            @"SELECT [ID], [LETRA], [RESPOSTA], [CORRETA], [QUESTAOID]
              FROM [TBALTERNATIVA]
              WHERE [QUESTAOID] = @QUESTAOID;";
        #endregion

        public void Cadastrar(Teste novoTeste)
        {
            if (novoTeste.Id == Guid.Empty)
                novoTeste.Id = Guid.NewGuid();

            dbConnection.Execute(sqlInserir, new
            {
                ID = novoTeste.Id,
                TITULO = novoTeste.Titulo,
                DATAGERACAO = novoTeste.DataGeracao,
                RECUPERACAO = novoTeste.Recuperacao,
                DISCIPLINAID = novoTeste.Disciplina?.Id,
                MATERIAID = novoTeste.Materia?.Id
            });

            // Relaciona questões ao teste
            if (novoTeste.Questoes != null)
            {
                foreach (var questao in novoTeste.Questoes)
                {
                    dbConnection.Execute(
                        @"INSERT INTO [TBTESTE_QUESTAO] ([TESTEID], [QUESTAOID]) VALUES (@TESTEID, @QUESTAOID);",
                        new { TESTEID = novoTeste.Id, QUESTAOID = questao.Id }
                    );
                }
            }
        }

        public bool Editar(Guid idRegistro, Teste registroEditado)
        {
            var linhasAfetadas = dbConnection.Execute(sqlEditar, new
            {
                ID = idRegistro,
                TITULO = registroEditado.Titulo,
                DATAGERACAO = registroEditado.DataGeracao,
                RECUPERACAO = registroEditado.Recuperacao,
                DISCIPLINAID = registroEditado.Disciplina?.Id,
                MATERIAID = registroEditado.Materia?.Id
            });

            // Atualiza relação de questões
            dbConnection.Execute("DELETE FROM [TBTESTE_QUESTAO] WHERE [TESTEID] = @TESTEID", new { TESTEID = idRegistro });
            if (registroEditado.Questoes != null)
            {
                foreach (var questao in registroEditado.Questoes)
                {
                    dbConnection.Execute(
                        @"INSERT INTO [TBTESTE_QUESTAO] ([TESTEID], [QUESTAOID]) VALUES (@TESTEID, @QUESTAOID);",
                        new { TESTEID = idRegistro, QUESTAOID = questao.Id }
                    );
                }
            }

            return linhasAfetadas > 0;
        }

        public bool Excluir(Guid idRegistro)
        {
            dbConnection.Execute("DELETE FROM [TBTESTE_QUESTAO] WHERE [TESTEID] = @TESTEID", new { TESTEID = idRegistro });
            var linhasAfetadas = dbConnection.Execute(sqlExcluir, new { ID = idRegistro });
            return linhasAfetadas > 0;
        }

        public List<Teste> SelecionarRegistros()
        {
            var testes = dbConnection.Query<Teste, Disciplina, Materia, Teste>(
                sqlSelecionarTodos,
                (teste, disciplina, materia) =>
                {
                    teste.Disciplina = disciplina;
                    teste.Materia = materia;
                    return teste;
                },
                splitOn: "DisciplinaId,MateriaId"
            ).ToList();

            foreach (var teste in testes)
            {
                teste.Questoes = SelecionarQuestoesDoTeste(teste.Id);
            }

            return testes;
        }

        public Teste? SelecionarRegistroPorId(Guid idRegistro)
        {
            var teste = dbConnection.Query<Teste, Disciplina, Materia, Teste>(
                sqlSelecionarPorId,
                (t, d, m) =>
                {
                    t.Disciplina = d;
                    t.Materia = m;
                    return t;
                },
                new { ID = idRegistro },
                splitOn: "DisciplinaId,MateriaId"
            ).FirstOrDefault();

            if (teste != null)
                teste.Questoes = SelecionarQuestoesDoTeste(teste.Id);

            return teste;
        }

        private List<Questao> SelecionarQuestoesDoTeste(Guid testeId)
        {
            var questoes = dbConnection.Query<Questao, Materia, Questao>(
                sqlSelecionarQuestoesDoTeste,
                (q, m) =>
                {
                    q.Materia = m;
                    return q;
                },
                new { TESTEID = testeId },
                splitOn: "MateriaId"
            ).ToList();

            foreach (var questao in questoes)
            {
                questao.Alternativas = SelecionarAlternativasDaQuestao(questao.Id);
            }

            return questoes;
        }

        private List<Alternativa> SelecionarAlternativasDaQuestao(Guid questaoId)
        {
            return dbConnection.Query<Alternativa>(
                sqlSelecionarAlternativasDaQuestao,
                new { QUESTAOID = questaoId }
            ).ToList();
        }
    }
}
