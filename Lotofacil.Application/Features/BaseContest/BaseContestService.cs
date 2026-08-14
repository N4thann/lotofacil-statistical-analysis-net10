using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.ContestActivityLogs;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Lotofacil.Application.Features.BaseContests
{
    public class BaseContestService : IBaseContestService
    {
        private readonly IRepository<BaseContest> _repository;
        private readonly IContestManagementService _contestMS;
        private readonly IBaseContestRepository _repositoryBC;
        private readonly IContestActivityLogService _activityLS;
        private readonly ILogger<BaseContestService> _logger;
        public BaseContestService(IRepository<BaseContest> repository, 
            IContestManagementService contestMS,
            IBaseContestRepository repositoryBC,
            IContestActivityLogService activityLS,
            ILogger<BaseContestService> logger)
        {
            _repository = repository;
            _contestMS = contestMS;
            _repositoryBC = repositoryBC;
            _activityLS = activityLS;
            _logger = logger;
        }

        public async Task<IEnumerable<BaseContest>> GetAllBaseContestAsync()
        {
            return await _repository.GetAllAsync();
        }

        public void Create(ContestViewModel contestVM)
        {
            var formattedName = $"Concurso {contestVM.Name}";   
            
            _logger.LogDebug("ContestName: {Name} , String de nome do Concurso formatada: {FormattedName}", contestVM.Name, formattedName);

            var baseContest = new BaseContest(formattedName, _contestMS.SetDataHour(contestVM.Data), _contestMS.FormatNumbersToSave(contestVM.Numbers));
            try
            {
                _repository.SaveAdd(baseContest);
            }
            catch (Exception ex) 
            {
                _logger.LogError("Erro ao criar o Concurso Base {Name}, Mensagem de erro: {message}, StackTrace: {StackTrace}", contestVM.Name, ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task EditBaseContestAsync(ContestViewModel contestVM)
        {
            _logger.LogDebug("Iniciando edição do Concurso Base {ConcursoBaseId}", contestVM.Id);

            var baseContest = await _repository.GetByIdAsync(contestVM.Id);
            if (baseContest == null)
            {
                _logger.LogWarning("Tentativa de editar concurso base inexistente {ConcursoBaseId}", contestVM.Id);
                throw new KeyNotFoundException($"Concurso base com ID {contestVM.Id} não encontrado.");
            }

            _logger.LogDebug("Atualizando propriedades do concurso base ID {ConcursoBaseId}. Nome anterior: {OldName}, Novo nome: {NewName}",
                contestVM.Id, baseContest.Name, contestVM.Name);

            baseContest.Name = contestVM.Name;
            baseContest.Data = _contestMS.SetDataHour(contestVM.Data);
            baseContest.Numbers = _contestMS.FormatNumbersToSave(contestVM.Numbers);
            try
            {
                await _repository.SaveUpdateAsync(baseContest);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao atualizar o Concurso Base de Id: {Id}, Mensagem de erro: {message}, StackTrace: {StackTrace}", contestVM.Id, ex.Message, ex.StackTrace);
                throw;
            }
        }

        //Esse método serve para recuperar uma BaseContest para ser mostrado na tela, de forma que respeite as dependencias da camada Presentation
        public async Task<ContestViewModel> ShowOnScreen(int id)
        {

            _logger.LogDebug("Iniciando recuperação do Concurso");

            var baseContest = await _repository.GetByIdAsync(id);      

            if (baseContest == null)
            {
                _logger.LogWarning("Concurso Base com ID {id} não foi encontrado", id);
                throw new KeyNotFoundException($"Concurso com ID {id} não encontrado.");
            }

            return new ContestViewModel
            {
                Id = baseContest.Id,
                Name = baseContest.Name,
                Data = baseContest.Data,
                Numbers = baseContest.Numbers
            };
        }

        public async Task<IEnumerable<BaseContest>> GetAllWithContestsAbove11Async()
        {
            _logger.LogDebug("Recuperando todos os BaseContests com concursos acima de 11.");
            try
            {
                var result = await _repositoryBC.GetAllWithContestsAbove11Async();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao tentar recuperar todos os BaseContests com concursos acima de 11, Mensagem de erro: {message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }        
        }

        public async Task DeleteByIdAsync(int id)
        {
            _logger.LogDebug("Iniciando exclusão do Concurso Base {Id}.", id);
            var baseContest = await _repository.GetByIdAsync(id);

            if (baseContest == null)
            {
                _logger.LogWarning("Tentativa de exclusão falhou. Concurso com ID {Id} não encontrado.", id);
                throw new KeyNotFoundException($"Concurso com ID {id} não encontrado.");
            }

            _logger.LogDebug("Deletando todas as referências de log associadas ao concurso {Name}.", baseContest.Name);
            try
            {
                await _activityLS.DeleteAllReferencesOfLogByBaseContest(baseContest.Name);
                await _repository.SaveDeleteAsync(id);
            }
            catch (Exception ex) 
            {
                _logger.LogError("Erro ao tentar deletar todas as referencias de {Name} ou deletar o Concurso Base de ID: {Id}, " +
                    "Mensagem de erro: {message}, StackTrace: {StackTrace}", baseContest.Name, id, ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IQueryable<BaseContest> GetQueryableBaseContests()
        {
            _logger.LogDebug("Retornando consulta IQueryable para BaseContests.");
            return _repository.GetAllQueryable();
        }

        public async Task<List<BaseContestSummaryViewModel>> GetFilteredBaseContestsAsync(string? name, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            _logger.LogDebug("Filtrando BaseContests com os seguintes parâmetros - Nome: {Name}, Data Inicial: {StartDate}, Data Final: {EndDate}, Página: {Page}, Tamanho da Página: {PageSize}",
                name, startDate, endDate, pageNumber, pageSize);
            try
            {
                var query = GetQueryableBaseContests();

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(log => log.Name.Contains(name));

                if (startDate.HasValue)
                    query = query.Where(log => log.Data >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(log => log.Data <= endDate.Value);

                // Projeta direto para o resumo em vez de Include(ContestsAbove11): a coleção completa nunca
                // é necessária aqui (só a contagem), então o Count() vira um COUNT correlacionado no SQL em
                // vez de trazer todas as linhas filhas da página inteira.
                var result = await query
                    .OrderBy(log => log.Data)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(bc => new BaseContestSummaryViewModel
                    {
                        Name = bc.Name,
                        Data = bc.Data,
                        Numbers = bc.Numbers,
                        Hit11 = bc.Hit11,
                        Hit12 = bc.Hit12,
                        Hit13 = bc.Hit13,
                        Hit14 = bc.Hit14,
                        Hit15 = bc.Hit15,
                        TopTenNumbers = bc.TopTenNumbers,
                        ContestsAbove11Count = bc.ContestsAbove11.Count()
                    })
                    .ToListAsync();

                _logger.LogDebug("Consulta de BaseContests filtrada retornou {Count} resultados.", result.Count);

                return result;
            }
            catch (Exception ex) 
            {
                _logger.LogError("Erro ao tentar filtrar um Concurso Base com os parametros  Nome: {Name}, Data Inicial: {StartDate}, Data Final: {EndDate}, Página: {Page}, Tamanho da Página: {PageSize} " +
                    "Mensagem de erro: {message}, StackTrace: {StackTrace}", name, startDate, endDate, pageNumber, pageSize, ex.Message, ex.StackTrace);
                throw;
            }           
        }

        public async Task<int> GetTotalCountAsync(string? name, DateTime? startDate, DateTime? endDate)
        {
            _logger.LogDebug("Contando o total de BaseContests com os filtros - Nome: {Name}, Data Inicial: {StartDate}, Data Final: {EndDate}", name, startDate, endDate);
            try
            {
                var query = GetQueryableBaseContests();

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(log => log.Name.Contains(name));

                if (startDate.HasValue)
                    query = query.Where(log => log.Data >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(log => log.Data <= endDate.Value);

                var count = await query.CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao tentar contar todos os Concurso Base com os parametros  Nome: {Name}, Data Inicial: {StartDate}, Data Final: {EndDate} " +
                    "Mensagem de erro: {message}, StackTrace: {StackTrace}", name, startDate, endDate, ex.Message, ex.StackTrace);
                throw;
            }         
        }
    }
}
