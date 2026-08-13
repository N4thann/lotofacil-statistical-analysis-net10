using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.BaseContests;
using Lotofacil.Application.Features.Contests;
using Lotofacil.Application.Features.ContestActivityLogs;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Infra.Data.Context;
using Lotofacil.Infra.Data.Initialization;
using Lotofacil.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Lotofacil.Infra.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
           IConfiguration configuration)
        {
            // Pegando a string de conexão do appsettings.json
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Registrando o DbContext com a string de conexão correta
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            //Configurações feitas para caso o banco de dados precise ser criado do zero
            services.AddScoped<IDataInitializer, ApplicationDbInitializer>();
            services.AddScoped<IInitializationDbService, InitializationDbService>();

            // Configuração do Hangfire no DependencyInjection
            //Faz uma limpeza nas tabelas que o Hangfire criou no banco de dados
            //Por padrão, os jobs são marcados para expiração após 24 horas.
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15), // Frequência de polling das filas
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5), // Timeout para mensagens
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5), // Tempo máximo de execução de batch
                    DisableGlobalLocks = true // Evita deadlocks
                });

            // Registrando serviços da camada de aplicação
            services.AddScoped<IBaseContestService, BaseContestService>();
            services.AddScoped<IContestService, ContestService>();
            services.AddScoped<IContestActivityLogService, ContestActivityLogService>();
            services.AddTransient<IContestManagementService, ContestManagementService>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IBaseContestRepository, BaseContestRepository>();
            services.AddScoped<IContestRepository, ContestRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Registrando serviços de validação com Fluent Validations
            services.AddTransient<IValidator<ContestViewModel>, ContestValidator>();

            // Configuração do Hangfire
            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings()
                      .UseSqlServerStorage(connectionString);
            });

            services.AddHangfireServer();

            return services;
        }

        public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));
        }
    }
}
/*Dentro do Clean Architecture, cada camada depende da camada de baixo nível apenas por meio de abstrações ou interfaces. 
 * A camada de infraestrutura (Infra.Data) contém a implementação concreta do DbContext e outras dependências de dados. 
 * A camada Infra.IoC, por sua vez, expõe essas dependências por meio da classe DependencyInjection, 
 * para que elas possam ser injetadas nas camadas superiores sem a necessidade de acoplamento direto.*/

/*Usar Singleton para um serviço que depende de DbContext (ou de repositórios que usam DbContext) geralmente causa problemas, pois o DbContext não é seguro para uso simultâneo (thread-safe). 
 * Como ele mantém o estado das entidades carregadas, compartilhar a mesma instância entre várias requisições pode levar a erros e comportamentos inesperados. 
 * O Scoped, por outro lado, cria uma instância isolada do DbContext para cada requisição, evitando esses conflitos*/
