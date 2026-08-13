using ClosedXML.Excel;
using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.Statistics;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Tests.DataBuilder;
using NSubstitute;
using Shouldly;

namespace Lotofacil.Tests.Common
{
    public class ContestManagementServiceTests
    {
        private readonly IRepository<Contest> _repositoryMock;
        private readonly ContestManagementService _sut;

        public ContestManagementServiceTests()
        {
            _repositoryMock = Substitute.For<IRepository<Contest>>();
            _sut = new ContestManagementService(_repositoryMock);
        }

        [Fact(DisplayName = "SUCESSO - Deve ajustar a hora da data para 20:00 mantendo o dia original")]
        public void SetDataHour_WhenCalled_ShouldSetTimeTo20h()
        {
            // Arrange
            var data = new DateTime(2026, 3, 15, 10, 30, 0);

            // Act
            var result = _sut.SetDataHour(data);

            // Assert
            result.Hour.ShouldBe(20);
            result.Minute.ShouldBe(0);
            result.Date.ShouldBe(data.Date);
        }

        [Fact(DisplayName = "SUCESSO - Deve converter uma string formatada em uma lista de inteiros")]
        public void ConvertFormattedStringToList_WhenInputIsValid_ShouldReturnListOfInts()
        {
            // Arrange
            var input = "01-02-03";

            // Act
            var result = _sut.ConvertFormattedStringToList(input);

            // Assert
            result.ShouldBe(new List<int> { 1, 2, 3 });
        }

        [Fact(DisplayName = "ERRO - Deve lançar FormatException quando a string contém valores não numéricos")]
        public void ConvertFormattedStringToList_WhenInputContainsNonNumeric_ShouldThrowFormatException()
        {
            // Arrange
            var input = "01-AB-03";

            // Act
            var act = () => _sut.ConvertFormattedStringToList(input);

            // Assert
            Should.Throw<FormatException>(act);
        }

        [Fact(DisplayName = "SUCESSO - Deve formatar uma string de dígitos em pares separados por hífen")]
        public void FormatNumbersToSave_WhenInputIsValid_ShouldReturnDashSeparatedPairs()
        {
            // Arrange
            var input = "010203";

            // Act
            var result = _sut.FormatNumbersToSave(input);

            // Assert
            result.ShouldBe("01-02-03");
        }

        [Fact(DisplayName = "ERRO - Deve lançar ArgumentException quando a string de entrada tem tamanho ímpar")]
        public void FormatNumbersToSave_WhenInputHasOddLength_ShouldThrowArgumentException()
        {
            // Arrange
            var input = "123";

            // Act
            var act = () => _sut.FormatNumbersToSave(input);

            // Assert
            Should.Throw<ArgumentException>(act);
        }

        [Fact(DisplayName = "ERRO - Deve lançar ArgumentException quando a string de entrada está vazia")]
        public void FormatNumbersToSave_WhenInputIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var input = "";

            // Act
            var act = () => _sut.FormatNumbersToSave(input);

            // Assert
            Should.Throw<ArgumentException>(act);
        }

        [Fact(DisplayName = "SUCESSO - Deve calcular a quantidade de números em comum entre duas listas")]
        public void CalculateIntersection_WhenListsHaveCommonNumbers_ShouldReturnCorrectCount()
        {
            // Arrange
            var list1 = new List<int> { 1, 2, 3, 4, 5 };
            var list2 = new List<int> { 3, 4, 5, 6, 7 };

            // Act
            var result = _sut.CalculateIntersection(list1, list2);

            // Assert
            result.ShouldBe(3);
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar zero quando as listas não têm números em comum")]
        public void CalculateIntersection_WhenListsHaveNoCommonNumbers_ShouldReturnZero()
        {
            // Arrange
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = new List<int> { 4, 5, 6 };

            // Act
            var result = _sut.CalculateIntersection(list1, list2);

            // Assert
            result.ShouldBe(0);
        }

        [Fact(DisplayName = "SUCESSO - Deve gerar planilha Excel com cabeçalhos e valores corretos para logs de atividade")]
        public void GenerateExcelForContestActivityLog_WhenDataIsProvided_ShouldProduceCorrectHeadersAndValues()
        {
            // Arrange
            var log = ContestActivityLogDataBuilder.Create()
                .WithName("Concurso 100")
                .WithNumbers("01-02-03-04-05-06-07-08-09-10-11-12-13-14-15")
                .WithData(new DateTime(2026, 1, 10))
                .WithBaseContestName("Concurso Base 1")
                .WithBaseContestNumbers("01-02-03-04-05-06-07-08-09-10-11-12-13-14-16")
                .Build();
            var data = new List<ContestActivityLog> { log };

            // Act
            using var stream = _sut.GenerateExcelForContestActivityLog(data);

            // Assert
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            worksheet.Cell(1, 1).GetString().ShouldBe("Concurso");
            worksheet.Cell(1, 2).GetString().ShouldBe("Números");
            worksheet.Cell(1, 3).GetString().ShouldBe("Data de Realização");
            worksheet.Cell(1, 4).GetString().ShouldBe("Concurso Base");
            worksheet.Cell(1, 5).GetString().ShouldBe("Números do Concurso Base");

            worksheet.Cell(2, 1).GetString().ShouldBe(log.Name);
            worksheet.Cell(2, 2).GetString().ShouldBe(log.Numbers);
            worksheet.Cell(2, 3).GetString().ShouldBe(log.Data.ToString());
            worksheet.Cell(2, 4).GetString().ShouldBe(log.BaseContestName);
            worksheet.Cell(2, 5).GetString().ShouldBe(log.BaseContestNumbers);
        }

        [Fact(DisplayName = "SUCESSO - Deve gerar planilha Excel somente com cabeçalhos quando não há logs de atividade")]
        public void GenerateExcelForContestActivityLog_WhenDataIsEmpty_ShouldProduceOnlyHeaders()
        {
            // Arrange
            var data = new List<ContestActivityLog>();

            // Act
            using var stream = _sut.GenerateExcelForContestActivityLog(data);

            // Assert
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            worksheet.Cell(1, 1).GetString().ShouldBe("Concurso");
            worksheet.Cell(2, 1).GetString().ShouldBeEmpty();
        }

        [Fact(DisplayName = "SUCESSO - Deve gerar planilha Excel com cabeçalhos e valores corretos para concursos base, incluindo o cálculo de eficiência corrigido")]
        public void GenerateExcelForBaseContest_WhenDataIsProvided_ShouldProduceCorrectHeadersAndValues()
        {
            // Arrange
            var baseContest = BaseContestDataBuilder.Create()
                .WithName("Concurso Base 1")
                .WithNumbers("01-02-03-04-05-06-07-08-09-10-11-12-13-14-15")
                .WithData(new DateTime(2026, 1, 10))
                .WithHits(hit11: 1, hit12: 1, hit13: 1, hit14: 1, hit15: 1)
                .WithTopTenNumbers("01-02-03-04-05-06-07-08-09-10")
                .Build();
            var data = new List<BaseContest> { baseContest };

            // Act
            using var stream = _sut.GenerateExcelForBaseContest(data);

            // Assert
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            worksheet.Cell(1, 1).GetString().ShouldBe("Concurso Base");
            worksheet.Cell(1, 2).GetString().ShouldBe("Números");
            worksheet.Cell(1, 3).GetString().ShouldBe("Data de Realização");
            worksheet.Cell(1, 4).GetString().ShouldBe("Acertou 11");
            worksheet.Cell(1, 5).GetString().ShouldBe("Acertou 12");
            worksheet.Cell(1, 6).GetString().ShouldBe("Acertou 13");
            worksheet.Cell(1, 7).GetString().ShouldBe("Acertou 14");
            worksheet.Cell(1, 8).GetString().ShouldBe("Acertou 15");
            worksheet.Cell(1, 9).GetString().ShouldBe("Valor do Cálculo de eficiência");
            worksheet.Cell(1, 10).GetString().ShouldBe("Top 10 números mais frequentes");

            worksheet.Cell(2, 1).GetString().ShouldBe(baseContest.Name);
            worksheet.Cell(2, 4).GetString().ShouldBe("1");
            worksheet.Cell(2, 5).GetString().ShouldBe("1");
            worksheet.Cell(2, 6).GetString().ShouldBe("1");
            worksheet.Cell(2, 7).GetString().ShouldBe("1");
            worksheet.Cell(2, 8).GetString().ShouldBe("1");
            // 1 + (1*2) + (1*3) + (1*4) + (1*5) = 15 — prova a correção da fórmula (Task 1);
            // a fórmula antiga (bug) daria 1 + 2 + 3 + (4*5) = 26.
            worksheet.Cell(2, 9).GetString().ShouldBe("15");
            worksheet.Cell(2, 10).GetString().ShouldBe(baseContest.TopTenNumbers);
        }

        [Fact(DisplayName = "SUCESSO - Deve gerar planilha Excel somente com cabeçalhos quando não há concursos base")]
        public void GenerateExcelForBaseContest_WhenDataIsEmpty_ShouldProduceOnlyHeaders()
        {
            // Arrange
            var data = new List<BaseContest>();

            // Act
            using var stream = _sut.GenerateExcelForBaseContest(data);

            // Assert
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            worksheet.Cell(1, 1).GetString().ShouldBe("Concurso Base");
            worksheet.Cell(2, 1).GetString().ShouldBeEmpty();
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar os dois concursos base com maior pontuação ponderada, em ordem decrescente")]
        public void TopTwoContests_WhenMultipleBaseContests_ShouldReturnTopTwoOrderedByWeightedScoreDescending()
        {
            // Arrange
            var lowScore = BaseContestDataBuilder.Create().WithName("Baixa Pontuação").WithHits(hit11: 1).Build(); // score 1
            var midScore = BaseContestDataBuilder.Create().WithName("Média Pontuação").WithHits(hit15: 1).Build(); // score 5
            var highScore = BaseContestDataBuilder.Create().WithName("Alta Pontuação").WithHits(hit11: 1, hit12: 1, hit13: 1, hit14: 1, hit15: 1).Build(); // score 15
            var baseContests = new List<BaseContest> { lowScore, midScore, highScore };

            // Act
            var result = _sut.TopTwoContests(baseContests);

            // Assert
            result.Count.ShouldBe(2);
            result[0].Name.ShouldBe("Alta Pontuação");
            result[1].Name.ShouldBe("Média Pontuação");
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar lista vazia quando não há concursos base")]
        public void TopTwoContests_WhenNoBaseContests_ShouldReturnEmptyList()
        {
            // Arrange
            var baseContests = new List<BaseContest>();

            // Act
            var result = _sut.TopTwoContests(baseContests);

            // Assert
            result.ShouldBeEmpty();
        }

        [Fact(DisplayName = "SUCESSO - Deve calcular corretamente as ocorrências dos números e a contagem de concursos vinculados")]
        public void TopTwoContests_ShouldComputeNumberOccurrencesAndCountContestsFromLinkedContests()
        {
            // Arrange
            var contest1 = ContestDataBuilder.Create().WithNumbers("01-02-03-04-05-06-07-08-09-10-11-12-13-14-15").Build();
            var contest2 = ContestDataBuilder.Create().WithNumbers("01-02-03-04-05-06-07-08-09-10-11-12-13-14-16").Build();
            var baseContest = BaseContestDataBuilder.Create()
                .WithTopTenNumbers("01-02-03-04-05-06-07-08-09-10")
                .WithContestsAbove11(new List<Contest> { contest1, contest2 })
                .Build();

            // Act
            var result = _sut.TopTwoContests(new List<BaseContest> { baseContest });

            // Assert
            result.Count.ShouldBe(1);
            var viewModel = result[0];
            viewModel.CountContests.ShouldBe(2);
            viewModel.TopTenNumbers.ShouldBe("01-02-03-04-05-06-07-08-09-10");
            viewModel.NumberOccurences.Single(o => o.Number == 1).Occurences.ShouldBe(2);
            viewModel.NumberOccurences.Single(o => o.Number == 15).Occurences.ShouldBe(1);
            viewModel.NumberOccurences.Single(o => o.Number == 16).Occurences.ShouldBe(1);
            viewModel.NumberOccurences.Single(o => o.Number == 17).Occurences.ShouldBe(0);
        }

        [Fact(DisplayName = "SUCESSO - Deve definir o nome do primeiro e do último concurso conforme retornados pelo repositório")]
        public async Task Dash3Analysis_WhenContestsExist_ShouldSetFirstAndLastContestNames()
        {
            // Arrange
            var first = ContestDataBuilder.Create().WithName("Concurso 1").Build();
            var middle = ContestDataBuilder.Create().WithName("Concurso 2").Build();
            var last = ContestDataBuilder.Create().WithName("Concurso 3").Build();
            _repositoryMock.GetAllAsync().Returns(new List<Contest> { first, middle, last });

            // Act
            var result = await _sut.Dash3Analysis(new List<BaseContest>());

            // Assert
            result.FirstContest.ShouldBe("Concurso 1");
            result.LastContest.ShouldBe("Concurso 3");
        }

        [Fact(DisplayName = "SUCESSO - Deve agrupar os concursos por ano de realização")]
        public async Task Dash3Analysis_ShouldGroupContestsByYear()
        {
            // Arrange
            var contest2024a = ContestDataBuilder.Create().WithData(new DateTime(2024, 3, 1)).Build();
            var contest2024b = ContestDataBuilder.Create().WithData(new DateTime(2024, 8, 1)).Build();
            var contest2025 = ContestDataBuilder.Create().WithData(new DateTime(2025, 1, 1)).Build();
            _repositoryMock.GetAllAsync().Returns(new List<Contest> { contest2024a, contest2024b, contest2025 });

            // Act
            var result = await _sut.Dash3Analysis(new List<BaseContest>());

            // Assert
            result.Years["2024"].ShouldBe(2);
            result.Years["2025"].ShouldBe(1);
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar primeiro e último concurso nulos e total zero quando não há concursos")]
        public async Task Dash3Analysis_WhenNoContests_ShouldReturnNullFirstAndLastContestAndZeroTotal()
        {
            // Arrange
            _repositoryMock.GetAllAsync().Returns(new List<Contest>());

            // Act
            var result = await _sut.Dash3Analysis(new List<BaseContest>());

            // Assert
            result.FirstContest.ShouldBeNull();
            result.LastContest.ShouldBeNull();
            result.Years.ShouldBeEmpty();
            result.TotalContests.ShouldBe(0);
        }

        [Fact(DisplayName = "SUCESSO - Deve definir o total de concursos base a partir do parâmetro recebido, independente do repositório")]
        public async Task Dash3Analysis_ShouldSetTotalBaseContestsFromParameter()
        {
            // Arrange
            _repositoryMock.GetAllAsync().Returns(new List<Contest>());
            var baseContests = BaseContestDataBuilder.AsList(4);

            // Act
            var result = await _sut.Dash3Analysis(baseContests);

            // Assert
            result.TotalBaseContests.ShouldBe(4);
        }

        [Fact(DisplayName = "SUCESSO - Deve calcular o total de páginas arredondando para cima")]
        public void PagedResultDash2_WhenTotalCountIsNotMultipleOfPageSize_ShouldComputeTotalPagesUsingCeiling()
        {
            // Arrange
            var baseContests = BaseContestDataBuilder.AsList(10);

            // Act
            var result = _sut.PagedResultDash2(baseContests, totalCount: 25, name: null, startDate: null, endDate: null, page: 1, pageSize: 10);

            // Assert
            result.TotalPages.ShouldBe(3);
        }

        [Fact(DisplayName = "SUCESSO - Deve mapear filtros e página atual corretamente no ViewModel paginado")]
        public void PagedResultDash2_ShouldMapFiltersAndPageIntoViewModel()
        {
            // Arrange
            var baseContests = BaseContestDataBuilder.AsList(2);
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 12, 31);

            // Act
            var result = _sut.PagedResultDash2(baseContests, totalCount: 2, name: "Concurso", startDate: startDate, endDate: endDate, page: 2, pageSize: 10);

            // Assert
            result.Datas.ShouldBe(baseContests);
            result.CurrentPage.ShouldBe(2);
            result.NameFilter.ShouldBe("Concurso");
            result.StartDateFilter.ShouldBe(startDate);
            result.EndDateFilter.ShouldBe(endDate);
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar zero páginas quando o total de registros é zero")]
        public void PagedResultDash2_WhenTotalCountIsZero_ShouldReturnZeroTotalPages()
        {
            // Arrange
            var baseContests = new List<BaseContest>();

            // Act
            var result = _sut.PagedResultDash2(baseContests, totalCount: 0, name: null, startDate: null, endDate: null, page: 1, pageSize: 10);

            // Assert
            result.TotalPages.ShouldBe(0);
        }
    }
}
