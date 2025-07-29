using System.Data;
using Dapper;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloMateria;

namespace TesteFacil.Infraestrutura.OrmDapper.ModuloDisciplina
{
    public class RepositorioDisciplinaComDapper : IRepositorioDisciplina
    {
        private readonly IDbConnection dbConnection;

        public RepositorioDisciplinaComDapper(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        #region SQL queries
        private const string sqlInserir =
            @"INSERT INTO [TBDISCIPLINA]
                ([ID], [NOME])
              VALUES
                (@ID, @NOME);";

        private const string sqlEditar =
            @"UPDATE [TBDISCIPLINA]
                SET [NOME] = @NOME
              WHERE [ID] = @ID;";

        private const string sqlExcluir =
            @"DELETE FROM [TBDISCIPLINA]
              WHERE [ID] = @ID;";

        private const string sqlSelecionarTodos =
            @"SELECT [ID], [NOME]
              FROM [TBDISCIPLINA];";

        private const string sqlSelecionarPorId =
            @"SELECT [ID], [NOME]
              FROM [TBDISCIPLINA]
              WHERE [ID] = @ID;";

        private const string sqlSelecionarMateriasDaDisciplina =
            @"SELECT [ID], [NOME], [SERIE], [DISCIPLINAID]
              FROM [TBMATERIA]
              WHERE [DISCIPLINAID] = @DISCIPLINAID;";
        #endregion

        public void Cadastrar(Disciplina novaDisciplina)
        {
            if (novaDisciplina.Id == Guid.Empty)
                novaDisciplina.Id = Guid.NewGuid();

            dbConnection.Execute(sqlInserir, new
            {
                ID = novaDisciplina.Id,
                NOME = novaDisciplina.Nome
            });
        }

        public bool Editar(Guid idRegistro, Disciplina registroEditado)
        {
            var linhasAfetadas = dbConnection.Execute(sqlEditar, new
            {
                ID = idRegistro,
                NOME = registroEditado.Nome
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

        public List<Disciplina> SelecionarRegistros()
        {
            var disciplinas = dbConnection.Query<Disciplina>(sqlSelecionarTodos).ToList();

            foreach (var disciplina in disciplinas)
            {
                disciplina.Materias = SelecionarMateriasDaDisciplina(disciplina.Id);
            }

            return disciplinas;
        }

        public Disciplina? SelecionarRegistroPorId(Guid idRegistro)
        {
            var disciplina = dbConnection.QueryFirstOrDefault<Disciplina>(sqlSelecionarPorId, new { ID = idRegistro });

            if (disciplina == null)
                return null;

            disciplina.Materias = SelecionarMateriasDaDisciplina(disciplina.Id);

            return disciplina;
        }

        private List<Materia> SelecionarMateriasDaDisciplina(Guid disciplinaId)
        {
            return dbConnection.Query<Materia>(sqlSelecionarMateriasDaDisciplina, new { DISCIPLINAID = disciplinaId }).ToList();
        }
    }
}
