using Lotofacil.Application.Common;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Serilog;

namespace Lotofacil.Application.BackgroundJobs
{
    /// <summary>
    /// Responsável pelo job principal do sistema, que estabelece relações e realiza comparações 
    /// entre os concursos diários e os concursos base armazenados ao longo do tempo.
    /// O processo calcula a interseção dos números dos concursos, que são conjuntos de 15 números,
    /// e registra os resultados quando determinadas condições são atendidas.
    /// </summary>
    public class MainJobHandler
    {
        private readonly IBaseContestRepository _repositoryBC;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IContestRepository _repositoryC;
        private readonly IContestManagementService _contestMS;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MainJobHandler"/>, injetando os repositórios necessários
        /// e o serviço de gerenciamento de concursos para manipular as relações entre concursos diários e concursos base.
        /// </summary>
        /// <param name="repositoryBC">Repositório para gerenciamento dos concursos base.</param>
        /// <param name="repositoryC">Repositório para gerenciamento dos concursos diários.</param>
        /// <param name="unitOfWork">Implementa o padrão UnitOfWork gerencia um conjunto de operações em uma única transação.</param>
        /// <param name="contestMS">Serviço para manipulação de concursos, incluindo operações como interseção de números.</param>
        public MainJobHandler(
            IUnitOfWork unitOfWork,
            IBaseContestRepository repositoryBC, 
            IContestRepository repositoryC,  
            IContestManagementService contestMS)
        {
            _unitOfWork = unitOfWork;
            _repositoryBC = repositoryBC;
            _repositoryC = repositoryC;
            _contestMS = contestMS;
        }

        /// <summary>
        /// Executa a lógica principal do job, comparando concursos diários com concursos base,
        /// estabelecendo relações e registrando atividades dos concursos.
        /// Utiliza um semáforo para garantir a execução thread-safe.
        /// </summary>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        public async Task ExecuteAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                Log.Information("Iniciando execução do MainJobHandler...");

                await SaveRelationshipsAsync();

                Log.Information("Finalizando execução do MainJobHandler...");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro durante a execução do MainJobHandler");
            }
            finally
            {
                _semaphore.Release();
                Log.Debug("Recurso liberado.");
            }
        }

        /// <summary>
        /// Compara os concursos diários com os concursos base, calcula a interseção dos números,
        /// e atualiza as relações e os registros de atividades.
        /// Salva relações quando a interseção excede 10 e registra a atividade com detalhes da correspondência.
        /// </summary>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        private async Task SaveRelationshipsAsync()
        {
            var baseContestRepository = _unitOfWork.Repository<BaseContest>();
            var contestRepository = _unitOfWork.Repository<Contest>();
            var logRepository = _unitOfWork.Repository<ContestActivityLog>();

            // Foi necessário criar repositórios específicos para recuperar as listas de ambas as entidades.
            // O método do repositório genérico não possuía o include das listas e utilizava AsNoTracking,
            // impossibilitando o update direto. 
            // Assim, o método genérico é mantido apenas para listagens simples,
            // enquanto os repositórios específicos permitem atualizações e consultas mais detalhadas.
            var baseContests = await _repositoryBC.GetAllWithContestsAbove11Async();
            var contests = await _repositoryC.GetAllWithBaseContestsAsync();

            var logs = new List<ContestActivityLog>();//Para não fazer diversas chamadas assincronas dentro do loop iremos salvar na lista e ao final 
            //gravar tudo de uma vez no banco com UnitOfWork

            if (baseContests.Any() && contests.Any())
            {
                foreach (var x in baseContests)
                {
                    var numbersBC = _contestMS.ConvertFormattedStringToList(x.Numbers);

                    foreach (var y in contests)
                    {

                        if (x.Name == y.Name || x.Data.Date == y.Data.Date)
                        {
                            Log.Debug("Concurso base {BaseName} ignorado na comparação com concurso {ContestName} (mesmo concurso)",
                                x.Name, y.Name);
                            continue;
                        }

                        var numbersC = _contestMS.ConvertFormattedStringToList(y.Numbers);

                        if (y.LastProcessedMainJob == null || y.LastProcessedMainJob < x.CreatedAt)
                        {
                            int allHits = _contestMS.CalculateIntersection(numbersBC, numbersC);

                            switch (allHits)
                            {
                                case 11: x.AddHit11(); break;
                                case 12: x.AddHit12(); break;
                                case 13: x.AddHit13(); break;
                                case 14: x.AddHit14(); break;
                                case 15: x.AddHit15(); break;
                            }

                            if (allHits > 10 && !x.ContestsAbove11.Contains(y))
                            {
                                logs.Add(new ContestActivityLog(
                                    y.Name,
                                    y.Numbers,
                                    y.Data,
                                    x.Name,
                                    x.Numbers,
                                    allHits
                                ));

                                x.ContestsAbove11.Add(y);
                                y.BaseContests.Add(x);

                                Log.Debug("O concurso {Name} teve mais de 11 acertos", y.Name);
                            }
                        }
                        else
                        {
                            Log.Debug("Concurso {Name}: já está atualizado", y.Name);
                        }
                    }

                    baseContestRepository.Update(x);
                }

                // Atualização em batch dos registros de 'LastProcessed'
                foreach (var y in contests)
                {
                    y.LastProcessedMainJob = DateTime.Now;
                    contestRepository.Update(y);
                }

                if (logs.Any())
                {
                    await logRepository.AddRangeAsync(logs); //Inserindo logs de uma vez só
                }

                await _unitOfWork.CompleteAsync();//Ele reduz o número de SaveChangesAsync, já que o método CompleteAsync pode ser chamado
                //uma única vez ao final das operações. Isso melhora a performance e evita múltiplas transações desnecessárias.
            }
            else
            {
                Log.Warning("Não há registros em Concurso e/ou Concurso Base.");
            }
        }
    }
}