using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Microsoft.AspNetCore.Mvc;
using Lotofacil.Application.Features.BaseContests;

namespace Lotofacil.Web.Features.Home
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBaseContestService _baseContestService;
        private readonly IContestManagementService _contestMS;

        public HomeController(IBaseContestService baseContestService,
            ILogger<HomeController> logger, IContestManagementService contestMS
            )
        {
            _logger = logger;
            _baseContestService = baseContestService;
            _contestMS = contestMS;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var baseContestList = await _baseContestService.GetAllBaseContestAsync();

                if (!baseContestList.Any())
                {
                    return View("Error", new ErrorViewModel(
                        "Nenhum registro encontrado na tabela Contests.", null, 2));
                }

                var orderedBaseContestList = baseContestList
                    .OrderByDescending(x => (x.Hit11 * 1) + (x.Hit12 * 2) + (x.Hit13 * 3) + (x.Hit14 * 4) + (x.Hit15 * 5))
                    .ToList();

                return View(orderedBaseContestList);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(
                    "Erro ao acessar os dados do banco de dados da tabela Contests.",
                    ex.Message, 1));
            }
        }

        public async Task<IActionResult> Dash()
        {
            try
            { 
                var baseContest = await _baseContestService.GetAllWithContestsAbove11Async();

                if (!baseContest.Any())
                {
                    return View("Error", new ErrorViewModel(
                        "Nenhum registro encontrado na tabela Contests.", null, 2)); // Código para ErrorType.NoRecords
                }

                var viewModel =  _contestMS.TopTwoContests(baseContest);

                return View(viewModel);
            }

            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel("Erro ao calcular os 2 concursos base com mais acertos.",
                    ex.Message, 4));
            }
        }

        public async Task<IActionResult> Dash2(string? name, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = pageSize < 1 ? 10 : pageSize;

            var baseContests = await _baseContestService.GetFilteredBaseContestsAsync(name, startDate, endDate, page, pageSize);
            var totalCount = await _baseContestService.GetTotalCountAsync(name, startDate, endDate); // Implementação no serviço para pegar o total de registros

            var model = _contestMS.PagedResultDash2(baseContests,totalCount,name,startDate,endDate,page,pageSize);

            return View(model);
        }

        public async Task<IActionResult> ExportToExcel(string? name, DateTime? startDate, DateTime? endDate)
        {
            var baseContests = await _baseContestService.GetFilteredBaseContestsAsync(name, startDate, endDate, 1, int.MaxValue); // Pega todos os registros

            var stream = _contestMS.GenerateExcelForBaseContest(baseContests);

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaseContest_{DateTime.Now:dd-MM-yyyy}.xlsx");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Dash3()
        {       
            var baseContests = await _baseContestService.GetAllBaseContestAsync();

            if (!baseContests.Any())
            {
                return View("Error", new ErrorViewModel(
                    "Nenhum registro encontrado na tabela Contests.", null, 2));
            }

            var dash3 = await _contestMS.Dash3Analysis(baseContests);

            return View(dash3);
        }

        /// <summary>
        /// Exibe a tela de erro genérica. Destino de <c>app.UseExceptionHandler("/Home/Error")</c>
        /// (<c>Program.cs</c>) para qualquer exceção não tratada em produção.
        /// </summary>
        public IActionResult Error()
        {
            return View("Error", new ErrorViewModel("Ocorreu um erro inesperado.", null, 1));
        }
    }
}