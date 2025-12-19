using Microsoft.EntityFrameworkCore;
using SurveySystem.Infrastructure.Data.Repositories;

namespace SurveySystem.Infrastructure.Data.Tests
{
    /// <summary>
    /// Fixture de teste para configurar e limpar o ambiente de teste do repositório Entity Framework.
    /// Implementa IAsyncLifetime para gerenciar o ciclo de vida assíncrono do setup e teardown.
    /// </summary>
    public class SurveySystemRepositoryTestFixture : IAsyncLifetime
    {
        /// <summary>
        /// Contexto do banco de dados para os testes.
        /// </summary>
        public SurveySystemDbContext DbContext { get; private set; }

        /// <summary>
        /// Repositórios a serem testados.
        /// </summary>
        public SqlServerSurveyRepository SurveyRepository { get; private set; }
        public SqlServerSubmissionRepository SubmissionRepository { get; private set; }

        /// <summary>
        /// Inicializa o ambiente de teste de forma assíncrona.
        /// Cria um DbContext em memória e o repositório.
        /// </summary>
        public async Task InitializeAsync()
        {
            // Configura as opções do DbContext para usar um banco de dados em memória.
            // Usa um nome de banco de dados único para cada execução de fixture para evitar conflitos.
            var options = new DbContextOptionsBuilder<SurveySystemDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Instancia o DbContext com as opções configuradas.
            DbContext = new SurveySystemDbContext(options);

            // Garante que o banco de dados em memória seja criado.
            // Para bancos de dados reais, isso seria 'MigrateAsync()'.
            await DbContext.Database.EnsureCreatedAsync();

            // Instancia os repositórios com o DbContext criado.
            SurveyRepository = new SqlServerSurveyRepository(DbContext);
            SubmissionRepository = new SqlServerSubmissionRepository(DbContext);
        }

        /// <summary>
        /// Limpa o ambiente de teste de forma assíncrona.
        /// Descarta o DbContext.
        /// </summary>
        public async Task DisposeAsync()
        {
            // Descarta o DbContext, liberando os recursos.
            await DbContext.DisposeAsync();
        }

        /// <summary>
        /// Limpa todos os dados das tabelas no banco de dados em memória.
        /// Este método deve ser chamado antes de cada teste individual que precise de um estado limpo.
        /// </summary>
        public async Task ClearDataAsync()
        {
            // Remove os dados existentes no banco de dados.
            DbContext.Surveys.RemoveRange(DbContext.Surveys);
            DbContext.Submissions.RemoveRange(DbContext.Submissions);

            // Salva as mudanças para efetivar a remoção.
            await DbContext.SaveChangesAsync();
        }
    }
}