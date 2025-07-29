using System.Data;
using Dapper;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloQuestao;

namespace TesteFacil.Infraestrutura.OrmDapper.ModuloMateria
{
    public class RepositorioMateriaComDapper : IRepositorioMateria
    {
        private readonly IDbConnection dbConnection;

        public RepositorioMateriaComDapper(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        #region SQL queries
        private const string sqlInserir =
            @"INSERT INTO [TBMATERIA]
                ([ID], [NOME], [SERIE], [DISCIPLINAID])
              VALUES
                (@ID, @NOME, @SERIE, @DISCIPLINAID);";

        private const string sqlEditar =
            @"UPDATE [TBMATERIA]
                SET [NOME] = @NOME,
                    [SERIE] = @SERIE,
                    [DISCIPLINAID] = @DISCIPLINAID
              WHERE [ID] = @ID;";

        private const string sqlExcluir =
            @"DELETE FROM [TBMATERIA]
              WHERE [ID] = @ID;";

        private const string sqlSelecionarTodos =
            @"SELECT m.[ID], m.[NOME], m.[SERIE], m.[DISCIPLINAID],
                     d.[ID] AS DisciplinaId, d.[NOME]
              FROM [TBMATERIA] m
              INNER JOIN [TBDISCIPLINA] d ON m.[DISCIPLINAID] = d.[ID];";

        private const string sqlSelecionarPorId =
            @"SELECT m.[ID], m.[NOME], m.[SERIE], m.[DISCIPLINAID],
                     d.[ID] AS DisciplinaId, d.[NOME]
              FROM [TBMATERIA] m
              INNER JOIN [TBDISCIPLINA] d ON m.[DISCIPLINAID] = d.[ID]
              WHERE m.[ID] = @ID;";
        #endregion

        public void Cadastrar(Materia novaMateria)
        {
            if (novaMateria.Id == Guid.Empty)
                novaMateria.Id = Guid.NewGuid();

            dbConnection.Execute(sqlInserir, new
            {
                ID = novaMateria.Id,
                NOME = novaMateria.Nome,
                SERIE = (int)novaMateria.Serie,
                DISCIPLINAID = novaMateria.Disciplina.Id
            });
        }

        public bool Editar(Guid idRegistro, Materia registroEditado)
        {
            var linhasAfetadas = dbConnection.Execute(sqlEditar, new
            {
                ID = idRegistro,
                NOME = registroEditado.Nome,
                SERIE = (int)registroEditado.Serie,
                DISCIPLINAID = registroEditado.Disciplina.Id
            });

            return linhasAfetadas > 0;
        }

        public bool Excluir(Guid idRegistro)
        {
            var linhasAfetadas = dbConnection.Execute(sqlExcluir, new
            {
                ID = idRegistro
            });

            return linhasAfetadas > 0;
        }

        public List<Materia> SelecionarRegistros()
        {
            var materias = dbConnection.Query<Materia, Disciplina, Materia>(
                sqlSelecionarTodos,
                (materia, disciplina) =>
                {
                    disciplina.Id = materia.DisciplinaId;
                    materia.Disciplina = disciplina;
                    return materia;
                },
                splitOn: "DisciplinaId"
            ).ToList();

            return materias;
        }

        public Materia? SelecionarRegistroPorId(Guid idRegistro)
        {
            var materia = dbConnection.Query<Materia, Disciplina, Materia>(
                sqlSelecionarPorId,
                (m, d) =>
                {
                    d.Id = m.DisciplinaId;
                    m.Disciplina = d;
                    return m;
                },
                new { ID = idRegistro },
                splitOn: "DisciplinaId"
            ).FirstOrDefault();

            return materia;
        }
    }
}
