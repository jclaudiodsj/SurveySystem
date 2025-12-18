using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SurveySystem.Domain.Repositories;
using SurveySystem.Infrastructure.Data;
using SurveySystem.Infrastructure.Data.Repositories;

namespace SurveySystem.Infrastructure
{
    /// <summary>
    /// Métodos de extensão para configurar serviços da camada de infraestrutura.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adiciona os serviços de infraestrutura (DbContext e Repositórios) à coleção de serviços.
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        /// <param name="configuration">A configuração da aplicação.</param>
        /// <returns>A coleção de serviços atualizada.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configura o DbContext para usar SQL Server com LocalDB.
            // A string de conexão é lida do arquivo de configuração (appsettings.json).
            services.AddDbContext<SurveyDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            // Registra o repositório de clientes para injeção de dependência.
            // Scoped significa que uma nova instância será criada por requisição.
            services.AddScoped<ISurveyRepository, SqlServerSurveyRepository>();

            return services;
        }
    }
}