using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.ContestActivityLogs;
using Lotofacil.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Lotofacil.Web.Features.ContestActivityLogs
{
    public class ContestActivityLogController : Controller
    {
        private readonly IContestActivityLogService _contestActivityLogService;
        private readonly IContestManagementService _contestMS;

        public ContestActivityLogController(IContestActivityLogService contestActivityLogService,
            IContestManagementService contestMS) 
        {
            _contestActivityLogService = contestActivityLogService;
            _contestMS = contestMS;
        }

        public async Task<IActionResult> Index(string? name, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = pageSize < 1 ? 10 : pageSize;

            var logs = await _contestActivityLogService.GetFilteredContestActivityLogsAsync(name, startDate, endDate, page, pageSize);
            var totalCount = await _contestActivityLogService.GetTotalCountAsync(name, startDate, endDate); // Implementação no serviço para pegar o total de registros

            var model = new PagedResultViewModel<ContestActivityLog>
            {
                Datas = logs,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                NameFilter = name,
                StartDateFilter = startDate,
                EndDateFilter = endDate
            };

            return View(model);
        }

        public async Task<IActionResult> ExportToExcel(string? name, DateTime? startDate, DateTime? endDate)
        {
            var logs = await _contestActivityLogService.GetFilteredContestActivityLogsAsync(name, startDate, endDate, 1, int.MaxValue);

            var stream = _contestMS.GenerateExcelForContestActivityLog(logs);

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ContestActivityLog_{DateTime.Now:dd-MM-yyyy}.xlsx");
        }
    }
}
