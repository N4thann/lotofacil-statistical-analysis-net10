using Lotofacil.Application.Common;
using Lotofacil.Application.Features.BaseContests;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Serilog;

namespace Lotofacil.Application.BackgroundJobs
{
    /// <summary>
    /// Responsável por executar o job que analisa os 10 números mais frequentes em um concurso base, 
    /// iterando pelos concursos associados. O resultado é armazenado na propriedade 
    /// <see cref="BaseContest.TopTenNumbers"/>, podendo ser utilizado para atualizações em tempo real em dashboards.
    /// </summary>
    public class TopTenJobHandler
    {
        private readonly IBaseContestService _baseContestService;
        private readonly IContestManagementService _contestMS;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="TopTenJobHandler"/>, 
        /// injetando os serviços e o repositório necessários para gerenciar os concursos base 
        /// e suas relações com outros concursos.
        /// </summary>
        /// <param name="unitOfWork">Implementa o padrão UnitOfWork gerencia um conjunto de operações em uma única transação.</param>
        /// <param name="contestMS">Serviço que manipula operações relacionadas a concursos.</param>
        /// <param name="baseContestService">Serviço para obtenção de concursos base com concursos associados.</param>
        public TopTenJobHandler(IUnitOfWork unitOfWork,
            IContestManagementService contestMS,
            IBaseContestService baseContestService)
        {
            _unitOfWork = unitOfWork;
            _contestMS = contestMS;
            _baseContestService = baseContestService;
        }

        /// <summary>
        /// Executa o job que calcula os 10 números mais frequentes para cada concurso base.
        /// O processo percorre os concursos associados, conta a ocorrência dos números e atualiza
        /// a propriedade <see cref="BaseContest.TopTenNumbers"/>.
        /// O uso de um semáforo garante que a execução seja thread-safe, impedindo concorrência indesejada.
        /// </summary>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        public async Task ExecuteAsync()
        {
           await _semaphore.WaitAsync();
            try
            {
                Log.Information("Inciando execução do TopTenJobHandler...");
                // Obter todos os concursos base
                var baseContests = await _baseContestService.GetAllWithContestsAbove11Async();

                var baseContestRepository = _unitOfWork.Repository<BaseContest>();
               
                int numberOfUpdates = 0;

                if (baseContests != null) 
                {
                    var totalCount = baseContests.Count();
                    Log.Debug("Quantidade de concursos base: {Count}", totalCount);

                    foreach (var baseContest in baseContests)
                    {
                        var totalCountContest = baseContest.ContestsAbove11.Count();

                        if (baseContest.TotalProcessed < totalCountContest || baseContest.TotalProcessed is null)
                        {
                            if (baseContest.ContestsAbove11 != null)
                            {
                                Log.Debug("Concurso Base {Name} não tem concursos adicionados", baseContest.Name);
                                continue;
                            }
                                
                            var occurrences = Enumerable.Range(1, 25).ToDictionary(i => i, _ => 0);//Substitui um "for" tradicional para preencher o valor do dictionary

                            // Contabilizar ocorrências dos números nos concursos
                            foreach (var contest in baseContest.ContestsAbove11)
                            {
                                var numbers = _contestMS.ConvertFormattedStringToList(contest.Numbers);
                                foreach (var number in numbers)
                                {
                                    occurrences[number]++;
                                }
                            }

                            // Obter os 10 números mais frequentes, mantendo apenas as chaves (números)
                            var top10Numbers = occurrences
                                .OrderByDescending(x => x.Value) // Ordena pela frequência (maior primeiro)
                                .ThenBy(x => x.Key) // Em caso de empate, ordena pelo menor número
                                .Take(10)
                                .Select(x => x.Key) // Pega apenas os números
                                .OrderBy(x => x) // Ordena em ordem crescente
                                .Select(x => x.ToString("D2")) // Formata para "01", "02", etc.
                                .ToList();

                            //Chama um método da entidade para atribuir valor a variável TopTenNumbers
                            baseContest.AddTopTenNumbers(string.Join("-", top10Numbers));
                            baseContest.TotalProcessed = totalCountContest;

                            Log.Debug("Valor dos 10 números mais frequentes do Concurso base {Name}: {Top10Numbers}", baseContest.Name, top10Numbers);
                            baseContestRepository.Update(baseContest);//chamada sem o await, isso evita várias chamadas assíncronas dentro do loop e melhora a performance.
                            numberOfUpdates++;
                        }
                        else{
                            Log.Debug("Concurso {Name} já está atualizado", baseContest.Name);
                        }                     
                    }
                }
                else
                {
                    Log.Warning("A tabela de Concurso Base não possui registros");
                }

                if (numberOfUpdates > 0)
                {
                    await _unitOfWork.CompleteAsync();
                    Log.Information("Número de concursos base atualizados: {NumberOfUpdates}", numberOfUpdates);
                }

                Log.Information("Finalizando execução do TopTenJobHandler...");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro durante a execução do TopTenJobHandler");
            }
            finally
            {
                _semaphore.Release();
                Log.Debug("Recurso liberado.");
            }
        }
    }
}
