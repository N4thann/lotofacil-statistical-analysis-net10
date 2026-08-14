using Lotofacil.Application.Common.ViewsModel;
using Microsoft.AspNetCore.Mvc;
using Lotofacil.Application.Features.BaseContests;
using FluentValidation;
using FluentValidation.Results;
using Lotofacil.Web.Extensions;

namespace Lotofacil.Web.Features.BaseContests
{
    public class BaseContestController : Controller
    {
        private readonly IBaseContestService _baseContestService;
        private readonly IValidator<ContestViewModel> _validator;

        public BaseContestController(IBaseContestService baseContestService,
            IValidator<ContestViewModel> validator)
        {
            _baseContestService = baseContestService;
            _validator = validator;
        }

        [HttpGet()]
        public async Task<IActionResult> List()
        {
            try
            {
               var baseContestList = await _baseContestService.GetAllWithContestsAbove11Async();

               return baseContestList.Any()
                    ? View(baseContestList)
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
        public async Task<IActionResult> Create(ContestViewModel baseContestVM)
        {
            baseContestVM.IsBaseContest = true;
            ValidationResult result = await _validator.ValidateAsync(baseContestVM);

            if(!result.IsValid)
            {
                result.AddToModelState(this.ModelState);

                return View("Create", baseContestVM);
            }

            _baseContestService.Create(baseContestVM);
            TempData["notice"] = "Concurso Base Criado com Sucesso!";
            return RedirectToAction("List", "BaseContest");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("Error", new ErrorViewModel(
                    "Id inválido ou não recebido.", null, 4) // Código que representa ErrorType.NotFound
                    );
            }

            var baseContest = await _baseContestService.GetAllWithContestsAbove11Async();
            var bcontest = baseContest.FirstOrDefault(x => x.Id == id);

            if (bcontest == null)
            {
                return View("Error", new ErrorViewModel(
                    $"Concurso Base com '{id}'não encontrado.", null, 4) // Código que representa ErrorType.NotFound
                    );
            }
            return View(bcontest);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var contestViewModel = await _baseContestService.ShowOnScreen(id);
                return View(contestViewModel);
            }
            catch (KeyNotFoundException ex)
            {
                return View("Error", new ErrorViewModel(ex.Message, null, 4)); // Código que representa ErrorType.NotFound
            }
        }

        [HttpPost(), ActionName("Delete")]//Quando for chamado esse método tem que chamar por "delete" graças ao ActionName
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _baseContestService.DeleteByIdAsync(id);
                TempData["notice"] = "Concurso Base deletado com Sucesso!";
                return RedirectToAction("List", "BaseContest");
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel("Erro ao deletar o resgistro.",
                    ex.Message, 1));
            }          
        }
    }
}
