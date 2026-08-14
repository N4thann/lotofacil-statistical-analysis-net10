using ClosedXML.Excel;
using FluentValidation;
using FluentValidation.Results;
using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.Contests;
using Lotofacil.Application.Features.Contests.DTO;
using Lotofacil.Domain.Entities;
using Lotofacil.Infra.Data.Context;
using Lotofacil.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Lotofacil.Web.Features.Contests
{
    public class ContestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IContestManagementService _managementService;
        private readonly IValidator<ContestViewModel> _validator;
        private readonly IContestService _contestService;
        private readonly IMemoryCache _cache;

        private const string CacheContestKey = "CacheContests";
        public ContestController(ApplicationDbContext context, 
            IContestManagementService managementService,
            IValidator<ContestViewModel> validator,
            IContestService contestService,
            IMemoryCache cache)
        {
            _context = context;
            _managementService = managementService;
            _validator = validator;
            _contestService = contestService;
            _cache = cache;
        }

        public async Task<IActionResult> List(string sortOrder)
        {
            try
            {
                var contests = await _contestService.GetContestsOrderedAsync(sortOrder);

                // Passar o estado da ordenação para a View
                ViewData["SortOrder"] = sortOrder == "DateAsc" ? "DateDesc" : "DateAsc";

                return contests.Any()
                    ? View(contests)
                    : View("Error", new ErrorViewModel(
                    "Nenhum registro encontrado na tabela Contests.", null, 2) // Código que representa ErrorType.NoRecords
                    );
            }
            catch (Exception ex) 
            {
                return View("Error", new ErrorViewModel("Erro ao acessar os dados do banco de dados da tabela Contests.",
                    ex.Message, 1));
            }            
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContestViewModel contestVM)
        {
            contestVM.IsBaseContest = false;
            ValidationResult result = await _validator.ValidateAsync(contestVM);

            if (!result.IsValid)
            {
                result.AddToModelState(this.ModelState);

                return View("Create", contestVM);
            }
            //Dentro desse serviço é transformado em um objeto do tipo Contest para salvar no banco
            _contestService.Create(contestVM);
            TempData["notice"] = "Concurso Criado com Sucesso!";
            return RedirectToAction("List", "Contest");
        }

        public IActionResult ReadExcel()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["error"] = "Nenhum arquivo foi enviado.";
                return RedirectToAction("List", "Contest");
            }

            var contests = new List<ContestViewModel>();
            int savedContestsCount = 0;
            var rowErrors = new List<string>();

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First();

                // Ler as linhas do Excel
                foreach (var row in worksheet.RowsUsed().Skip(1)) // Pula a linha de cabeçalhos
                {
                    try
                    {
                        Log.Information("Iniciando serviço de Input de concursos através de um arquivo excel");
                        // Recuperar o valor da célula de data como texto
                        var dateCell = row.Cell(2).GetValue<string>();
                        DateTime date;

                        // Tentar converter o texto para DateTime
                        if (!DateTime.TryParse(dateCell, out date))
                        {
                            rowErrors.Add($"Erro ao processar a data no concurso {row.Cell(1).GetValue<string>()}. Valor inválido: {dateCell}.");
                            continue; // Pula esta linha
                        }

                        var contestVM = new ContestViewModel
                        {
                            Name = row.Cell(1).GetValue<string>(), // Nome do Concurso
                            Data = date, // Data do Concurso convertida
                            Numbers = string.Join("", Enumerable.Range(3, 15) // Colunas de bolas 1 a 15
                                .Select(col => row.Cell(col).GetValue<int>().ToString("D2"))) // Formata como "01", "02", etc.
                        };
                        contestVM.IsBaseContest = false;

                        // Valida o objeto ContestViewModel
                        ValidationResult result = await _validator.ValidateAsync(contestVM);

                        if (!result.IsValid)
                        {
                            rowErrors.Add($"Erro na validação do concurso {contestVM.Name}.");
                            continue; // Pula este concurso
                        }

                        contests.Add(contestVM);
                    }
                    catch (Exception ex)
                    {
                        rowErrors.Add($"Erro ao processar uma linha: {ex.Message}");
                        continue;
                    }
                }

                foreach (var contest in contests)
                {
                     _contestService.Create(contest);
                    savedContestsCount++;
                }
                _cache.Remove(CacheContestKey);

                TempData["notice"] = "Importação concluída com sucesso!";
                if (rowErrors.Any())
                {
                    TempData["error"] = $"{rowErrors.Count} linha(s) com problema, ignoradas: {string.Join(" | ", rowErrors)}";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Erro ao importar o arquivo: {ex.Message}";
            }

            Log.Information("Total de concursos salvos: {SavedContestsCount}", savedContestsCount);

            return RedirectToAction("List", "Contest");
        }

        [HttpPost]
        [Route("/List/AnalisarConcursos")]
        public async Task<IActionResult> AnalisarConcursos([FromBody] ContestModalRequestDTO request)
        {
            if (request == null || request.Contests is null || !request.Contests.Any())
                return BadRequest(new { sucess = false, message = "É necessário selecionar pelo menos 1 concurso" });

            var response = await _contestService.AnalisarConcursos(request);

            if (response is null)
                return BadRequest(new { sucess = false, message = "Erro ao analisar os concursos!" });
            else
                return Ok(new{ sucesso = true , response});
        }
    }
}
