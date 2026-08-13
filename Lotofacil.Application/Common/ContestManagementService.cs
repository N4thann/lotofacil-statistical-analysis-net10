using ClosedXML.Excel;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.Statistics;
using Lotofacil.Domain.Common;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Serilog;
using System.Text;

namespace Lotofacil.Application.Common
{
    /// <summary>
    /// Service for managing contests and providing common utility methods for contest-related entities.
    /// </summary>
    public class ContestManagementService : IContestManagementService
    {
        private readonly IRepository<Contest> _repository;

        public ContestManagementService(IRepository<Contest> repository) => _repository = repository;

        public DateTime SetDataHour(DateTime data)
        {
            return data.Date.AddHours(20);
        }

        public List<int> ConvertFormattedStringToList(string input)
        {
            List<int> numbersList = new List<int>();
            string[] pairs = input.Split('-');

            foreach (string pair in pairs)
            {
                if (int.TryParse(pair, out int number))
                {
                    numbersList.Add(number);
                }
                else
                {
                    throw new FormatException("A string formatada contém valores não numéricos.");
                }
            }

            return numbersList;
        }


        public string FormatNumbersToSave(string input)
        {
            if (input.Length % 2 != 0 || input.Length == 0)
            {
                throw new ArgumentException("A string de entrada deve ter um número par de caracteres e não pode estar vazia.");
            }

            StringBuilder sb = new StringBuilder();//Mais perfomático na situação em que precisamos iterar concatenando strings

            for (int i = 0; i < input.Length; i += 2)
            {
                sb.Append(input.Substring(i, 2));
                if (i < input.Length - 2)
                {
                    sb.Append("-");
                }
            }
            return sb.ToString();
        }

        public int CalculateIntersection(List<int> list1, List<int> list2)
        {
            return list1.Intersect(list2).Count();
        }

        public MemoryStream GenerateExcelForContestActivityLog(IEnumerable<ContestActivityLog> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Log dos Concursos");

            // Definir headers personalizados e sua ordem
            var headers = new List<(string Header, Func<ContestActivityLog, object?> ValueSelector)>
                {
                    ("Concurso", log => log.Name),
                    ("Números", log => log.Numbers),
                    ("Data de Realização", log => log.Data),
                    ("Concurso Base", log => log.BaseContestName),
                    ("Números do Concurso Base", log => log.BaseContestNumbers)
                };

            // Criar cabeçalhos na planilha
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i].Header;
            }

            // Estilizar cabeçalhos
            var headerRange = worksheet.Range(1, 1, 1, headers.Count);
            headerRange.Style.Font.FontColor = XLColor.White; // Texto branco
            headerRange.Style.Fill.BackgroundColor = XLColor.Black; // Fundo preto
            headerRange.Style.Font.Bold = true; // Negrito
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;//Centralizar os valores

            // Adicionar registros dinamicamente
            int row = 2;
            foreach (var i in data)
            {
                for (int col = 0; col < headers.Count; col++)
                {
                    var value = headers[col].ValueSelector(i); // Obter valor usando o seletor
                    var cell = worksheet.Cell(row, col + 1);
                    cell.Value = value?.ToString() ?? string.Empty;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;//Centralizar os valores
                }
                row++;
            }

            // Ajustar largura das colunas
            worksheet.Columns().AdjustToContents();

            // Gerar memória para retorno
            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public MemoryStream GenerateExcelForBaseContest(IEnumerable<BaseContest> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Concursos Base");

            var headers = new List<(string Header, Func<BaseContest, object?> ValueSelector)>
                {
                    ("Concurso Base", log => log.Name),
                    ("Números", log => log.Numbers),
                    ("Data de Realização", log => log.Data),
                    ("Acertou 11", log => log.Hit11),
                    ("Acertou 12", log => log.Hit12),
                    ("Acertou 13", log => log.Hit13),
                    ("Acertou 14", log => log.Hit14),
                    ("Acertou 15", log => log.Hit15),
                    ("Valor do Cálculo de eficiência", log => 
                    (log.Hit11) + (log.Hit12 * 2) + (log.Hit13 * 3) + (log.Hit14 * 4) * (log.Hit15 * 5)),
                    ("Top 10 números mais frequentes", log => log.TopTenNumbers),
                };

            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i].Header;
            }

            var headerRange = worksheet.Range(1, 1, 1, headers.Count);
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.Black; 
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var i in data)
            {
                for (int col = 0; col < headers.Count; col++)
                {
                    var value = headers[col].ValueSelector(i); // Obter valor usando o seletor
                    var cell = worksheet.Cell(row, col + 1);
                    cell.Value = value?.ToString() ?? string.Empty;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                row++;
            }

            // Ajustar largura das colunas
            worksheet.Columns().AdjustToContents();

            // Gerar memória para retorno
            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public List<TopContestViewModel> TopTwoContests(IEnumerable<BaseContest> baseContests)
        {
            Log.Debug("Iniciando cálculo dos dois melhores concursos base de {TotalContests} concursos", baseContests.Count());

            // Calcula o somatório para cada concurso
            var contestsWithSum = baseContests
                .Select(x => new
                {
                    Contest = x,
                    Sum = (x.Hit11 * 1) + (x.Hit12 * 2) + (x.Hit13 * 3) + (x.Hit14 * 4) + (x.Hit15 * 5)
                })
                .ToList();

            Log.Debug("Somatório calculado para {Count} concursos", contestsWithSum.Count);

            // Ordena pelo maior somatório e pega os dois primeiros
            var topTwoContests = contestsWithSum
                .OrderByDescending(x => x.Sum)
                .Take(2)
                .Select(x => x.Contest)
                .ToList();

            if (topTwoContests.Count == 0)
            {
                Log.Warning("Nenhum concurso base encontrado para análise de top 2");
                return new List<TopContestViewModel>();
            }

            Log.Debug("Top 2 concursos selecionados: {Contests}",
                string.Join(", ", topTwoContests.Select(c => c.Name)));

            // Lista para armazenar os ViewModels
            var viewModel = new List<TopContestViewModel>();
            foreach (var x in topTwoContests)
            {
                Log.Debug("Processando concurso {ContestName} com {ContestsCount} concursos relacionados",
                    x.Name, x.ContestsAbove11.Count());

                // Dicionário para contar as ocorrências dos números (1 a 25)
                var occurrences = new Dictionary<int, int>();
                for (int i = 1; i <= 25; i++)
                {
                    occurrences[i] = 0;
                }

                // Calcula as ocorrências
                foreach (var y in x.ContestsAbove11)
                {
                    var numbers = ConvertFormattedStringToList(y.Numbers);
                    foreach (var i in numbers)
                    {
                        if (occurrences.ContainsKey(i))
                        {
                            occurrences[i]++;
                        }
                    }
                }

                var topNumbers = occurrences.OrderByDescending(o => o.Value).Take(10)
                    .Select(o => o.Key).ToList();
                Log.Debug("Top 10 números mais frequentes para {ContestName}: {TopNumbers}",
                    x.Name, string.Join(", ", topNumbers));

                viewModel.Add(new TopContestViewModel
                {
                    Name = x.Name,
                    Data = x.Data,
                    Number = x.Numbers,
                    CountContests = x.ContestsAbove11.Count(),
                    TopTenNumbers = x.TopTenNumbers,
                    NumberOccurences = occurrences.Select(o => new NumberOccurencesModel
                    {
                        Number = o.Key,
                        Occurences = o.Value
                    }).ToList()
                });
            }

            Log.Information("Análise dos top 2 concursos concluída com sucesso: {ContestNames}",
                string.Join(", ", viewModel.Select(vm => vm.Name)));
            return viewModel;
        }

        public async Task<Dash3ViewModel> Dash3Analysis(IEnumerable<BaseContest> baseContests)
        {
            Log.Debug("Iniciando análise para dashboard 3");

            var dash3 = new Dash3ViewModel();
            var contests = await _repository.GetAllAsync();

            Log.Debug("Recuperados {ContestsCount} concursos do repositório", contests.Count());

            var lastContest = contests.LastOrDefault();
            if (lastContest != null)
            {
                Log.Debug("Último concurso identificado: {ContestName}", lastContest.Name);
                dash3.LastContest = lastContest.Name;
            }
            else
            {
                Log.Warning("Nenhum concurso encontrado para última posição");
            }

            var firstContest = contests.FirstOrDefault();
            if (firstContest != null)
            {
                Log.Debug("Primeiro concurso identificado: {ContestName}", firstContest.Name);
                dash3.FirstContest = firstContest.Name;
            }
            else
            {
                Log.Warning("Nenhum concurso encontrado para primeira posição");
            }

            dash3.Years = contests
                .GroupBy(c => c.Data.Year)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            Log.Debug("Anos processados: {YearsCount} anos com dados", dash3.Years.Count);

            dash3.TotalBaseContests = baseContests.Count();
            dash3.TotalContests = contests.Count();

            Log.Information("Análise do Dashboard 3 concluída. Total de concursos: {ContestsCount}, Total de concursos base: {BaseContestsCount}",
                dash3.TotalContests, dash3.TotalBaseContests);
            return dash3;
        }

        public PagedResultViewModel<BaseContest> PagedResultDash2(List<BaseContest> baseContests, int totalCount, string? name, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            Log.Debug("Construindo modelo paginado para Dashboard 2. Filtros: Nome={Name}, DataInicial={StartDate}, DataFinal={EndDate}, Página={Page}, TamanhoPerPage={PageSize}",
                name ?? "todos", startDate?.ToString() ?? "sem limite inicial", endDate?.ToString() ?? "sem limite final", page, pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            Log.Debug("Total de registros: {TotalCount}, Total de páginas calculadas: {TotalPages}",
                totalCount, totalPages);

            var model = new PagedResultViewModel<BaseContest>
            {
                Datas = baseContests,
                CurrentPage = page,
                TotalPages = totalPages,
                NameFilter = name,
                StartDateFilter = startDate,
                EndDateFilter = endDate
            };

            Log.Information("Modelo paginado construído com sucesso. Exibindo página {CurrentPage} de {TotalPages} com {ItemCount} itens",
                model.CurrentPage, model.TotalPages, model.Datas.Count);
            return model;
        }
    }
}
