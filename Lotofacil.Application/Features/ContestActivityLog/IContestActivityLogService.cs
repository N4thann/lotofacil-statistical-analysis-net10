using Lotofacil.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotofacil.Application.Features.ContestActivityLogs
{
    public interface IContestActivityLogService
    {
        IQueryable<ContestActivityLog> GetQueryableContestActivityLogs();
        Task<List<ContestActivityLog>> GetFilteredContestActivityLogsAsync(string? name, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync(string? name, DateTime? startDate, DateTime? endDate);
        Task DeleteAllReferencesOfLogByBaseContest(string baseContestName);
    }
}
