using DocumentFormat.OpenXml.Office2010.Excel;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq;

namespace Lotofacil.Application.Features.ContestActivityLogs
{
    public class ContestActivityLogService : IContestActivityLogService
    {
        private readonly IRepository<ContestActivityLog> _repository;
        private readonly IUnitOfWork _unitOfWork;
        public ContestActivityLogService(IRepository<ContestActivityLog> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public IQueryable<ContestActivityLog> GetQueryableContestActivityLogs()
        {
            Log.Debug("Obtendo registros de atividade de concursos como IQueryable");
            return _repository.GetAllQueryable();
        }

        public async Task DeleteAllReferencesOfLogByBaseContest(string baseContestName)
        {
            var contestLog = Log.ForContext("BaseContestName", baseContestName);
            contestLog.Debug("Iniciando exclusão de logs relacionados ao concurso base");
            var logRepository = _unitOfWork.Repository<ContestActivityLog>();

            var logs = await _repository.GetAllAsync();
            int deletedCount = 0;

            foreach (var log in logs)
            {
                if (log.BaseContestName.Contains(baseContestName))
                {
                    contestLog.Debug("Excluindo log com ID {LogId} relacionado ao concurso base {BaseContestName}", log.Id, baseContestName);
                    logRepository.Delete(log);
                    deletedCount++;
                }
            }
            await _unitOfWork.CompleteAsync();
            contestLog.Information("Exclusão concluída: {DeletedCount} logs relacionados ao concurso base {BaseContestName} foram removidos", deletedCount, baseContestName);
        }

        public async Task<List<ContestActivityLog>> GetFilteredContestActivityLogsAsync(string? name, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            Log.Debug("Iniciando busca filtrada de logs de atividade. Filtros: Nome={Name}, DataInicial={StartDate}, DataFinal={EndDate}, Página={PageNumber}, ItensPerPage={PageSize}",
                name ?? "todos", startDate?.ToString() ?? "sem limite inicial", endDate?.ToString() ?? "sem limite final", pageNumber, pageSize);

            var query = GetQueryableContestActivityLogs();

            if (!string.IsNullOrEmpty(name))
            {
                Log.Debug("Aplicando filtro por nome: {Name}", name);
                query = query.Where(log => log.Name.Contains(name));
            }

            if (startDate.HasValue)
            {
                Log.Debug("Aplicando filtro por data inicial: {StartDate}", startDate.Value);
                query = query.Where(log => log.Data >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                Log.Debug("Aplicando filtro por data final: {EndDate}", endDate.Value);
                query = query.Where(log => log.Data <= endDate.Value);
            }

            var result = await query
                .OrderByDescending(log => log.Data)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Log.Information("Consulta filtrada concluída: {ResultCount} registros encontrados", result.Count);
            return result;
        }

        public async Task<int> GetTotalCountAsync(string? name, DateTime? startDate, DateTime? endDate)
        {
            Log.Debug("Calculando total de registros com filtros: Nome={Name}, DataInicial={StartDate}, DataFinal={EndDate}",
                name ?? "todos", startDate?.ToString() ?? "sem limite inicial", endDate?.ToString() ?? "sem limite final");
            //Segurança contra nulos: Evita exceções quando os parâmetros são nulos
            //Logging estruturado: Os valores são armazenados separadamente da mensagem, permitindo consultas e análises avançadas

            var query = GetQueryableContestActivityLogs();

            if (!string.IsNullOrEmpty(name))
            {
                Log.Debug("Aplicando filtro por nome: {Name}", name);
                query = query.Where(log => log.Name.Contains(name));
            }

            if (startDate.HasValue)
            {
                Log.Debug("Aplicando filtro por data inicial: {StartDate}", startDate.Value);
                query = query.Where(log => log.Data >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                Log.Debug("Aplicando filtro por data final: {EndDate}", endDate.Value);
                query = query.Where(log => log.Data <= endDate.Value);
            }

            var count = await query.CountAsync();
            Log.Information("Total de registros encontrados: {Count}", count);
            return count;
        }

    }
}
