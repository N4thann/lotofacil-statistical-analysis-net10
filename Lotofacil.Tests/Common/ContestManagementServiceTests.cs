using Lotofacil.Application.Common;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
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
    }
}
