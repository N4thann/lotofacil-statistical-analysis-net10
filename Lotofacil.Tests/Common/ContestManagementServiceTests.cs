using ClosedXML.Excel;
using Lotofacil.Application.Common;
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
    }
}
