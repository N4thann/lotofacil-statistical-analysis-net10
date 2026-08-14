using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Infra.Data.Context;
using Lotofacil.Infra.Data.Initialization.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lotofacil.Infra.Data.Initialization
{
    /// <summary>
    /// Popula o banco de dados com o histórico completo de concursos (a partir do CSV de resultados)
    /// e os concursos base de referência, na primeira vez que a aplicação sobe contra um banco vazio.
    /// </summary>
    public class ApplicationDbInitializer : IDataInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationDbInitializer> _logger;

        public ApplicationDbInitializer(ApplicationDbContext context, ILogger<ApplicationDbInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Aplica as migrations pendentes e, se as tabelas <c>Contest</c> e <c>BaseContest</c> estiverem
        /// vazias, popula o banco com os 4 concursos base fixos e todo o histórico do CSV de seed.
        /// Idempotente — não faz nada se já houver dados em qualquer uma das duas tabelas.
        /// </summary>
        public void Seed()
        {
            _context.Database.Migrate();

            if (!_context.BaseContests.Any() && !_context.Contests.Any())
            {
                var baseContests = new List<BaseContest>
                {
                    new BaseContest("Concurso 444", new DateTime(2009, 07, 16), "01-02-03-06-07-10-14-15-18-19-20-21-22-24-25"),
                    new BaseContest("Concurso 888", new DateTime(2013, 04, 03), "01-03-06-11-12-13-14-16-17-18-19-20-21-22-24"),
                    new BaseContest("Concurso 1501", new DateTime(2017, 04, 19), "01-06-07-08-09-10-11-13-16-17-18-21-22-24-25"),
                    new BaseContest("Concurso 2502", new DateTime(2022, 04, 22), "02-03-04-05-09-11-12-13-15-18-20-21-23-24-25")
                };

                var seedFilePath = Path.Combine(AppContext.BaseDirectory, "Initialization", "Seed", "lotofacil-resultados.csv");
                var contests = ContestCsvReader.Read(seedFilePath);

                _context.BaseContests.AddRange(baseContests);
                _context.Contests.AddRange(contests);

                _context.SaveChanges();

                _logger.LogInformation("Seed concluído: {ContestCount} concursos e {BaseContestCount} concursos base criados.",
                    contests.Count, baseContests.Count);
            }
            else
            {
                _logger.LogInformation("Seed ignorado: já existem concursos ou concursos base cadastrados.");
            }
        }
    }
}
