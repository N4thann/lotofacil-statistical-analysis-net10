using FluentValidation.Results;
using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using NSubstitute;
using Shouldly;

namespace Lotofacil.Tests.Common
{
    public class ContestValidatorTests
    {
        private readonly IRepository<Contest> _contestRepositoryMock;
        private readonly IRepository<BaseContest> _baseContestRepositoryMock;
        private readonly ContestValidator _sut;

        public ContestValidatorTests()
        {
            _contestRepositoryMock = Substitute.For<IRepository<Contest>>();
            _baseContestRepositoryMock = Substitute.For<IRepository<BaseContest>>();
            _sut = new ContestValidator(_contestRepositoryMock, _baseContestRepositoryMock);
        }

        private static ContestViewModel ValidModel(bool isBaseContest = false) => new()
        {
            Name = "1234",
            Numbers = "010203040506070809101112131415",
            Data = DateTime.Now.AddDays(-1),
            IsBaseContest = isBaseContest
        };

        [Fact(DisplayName = "SUCESSO - Deve validar com sucesso quando todos os campos são válidos e o nome não está duplicado")]
        public async Task ValidateAsync_WhenAllFieldsAreValidAndNameIsNotDuplicated_ShouldPass()
        {
            // Arrange
            var model = ValidModel(isBaseContest: false);
            _contestRepositoryMock.ExistsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Contest, bool>>>()).Returns(false);

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeTrue();
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando o nome é nulo")]
        public async Task ValidateAsync_WhenNameIsNull_ShouldFail()
        {
            // Arrange
            var model = ValidModel();
            model.Name = null!;

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Name");
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando o nome excede 5 caracteres")]
        public async Task ValidateAsync_WhenNameExceeds5Characters_ShouldFail()
        {
            // Arrange
            var model = ValidModel();
            model.Name = "123456";

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Name");
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando o nome já existe como Concurso Base")]
        public async Task ValidateAsync_WhenNameAlreadyExistsAsBaseContest_ShouldFail()
        {
            // Arrange
            var model = ValidModel(isBaseContest: true);
            _baseContestRepositoryMock.ExistsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<BaseContest, bool>>>()).Returns(true);

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Name" && e.ErrorMessage == "Esse concurso já foi cadastrado.");
            await _contestRepositoryMock.DidNotReceiveWithAnyArgs().ExistsAsync(default!);
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando o nome já existe como Concurso")]
        public async Task ValidateAsync_WhenNameAlreadyExistsAsContest_ShouldFail()
        {
            // Arrange
            var model = ValidModel(isBaseContest: false);
            _contestRepositoryMock.ExistsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Contest, bool>>>()).Returns(true);

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Name" && e.ErrorMessage == "Esse concurso já foi cadastrado.");
            await _baseContestRepositoryMock.DidNotReceiveWithAnyArgs().ExistsAsync(default!);
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando os números são nulos")]
        public async Task ValidateAsync_WhenNumbersIsNull_ShouldFail()
        {
            // Arrange
            var model = ValidModel();
            model.Numbers = null!;

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Numbers");
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando os números não têm exatamente 30 caracteres")]
        public async Task ValidateAsync_WhenNumbersLengthIsNot30_ShouldFail()
        {
            // Arrange
            var model = ValidModel();
            model.Numbers = "0102030405";

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Numbers");
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando a data é o valor padrão (default)")]
        public async Task ValidateAsync_WhenDataIsDefault_ShouldFail()
        {
            // Arrange
            var model = ValidModel();
            model.Data = default;

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Data");
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando a data está no futuro")]
        public async Task ValidateAsync_WhenDataIsInTheFuture_ShouldFail()
        {
            // Arrange
            var model = ValidModel();
            model.Data = DateTime.Now.AddDays(10);

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Data");
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando os números contêm uma dezena duplicada")]
        public async Task ValidateAsync_WhenNumbersHaveADuplicateDezena_ShouldFail()
        {
            // Arrange
            var model = ValidModel();
            model.Numbers = "010102030405060708091011121314"; // "01" repetido, 30 caracteres

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Numbers");
        }

        [Fact(DisplayName = "ERRO - Deve falhar quando uma dezena está fora da faixa 01-25")]
        public async Task ValidateAsync_WhenADezenaIsOutOfRange_ShouldFail()
        {
            // Arrange
            var model = ValidModel();
            model.Numbers = "010203040506070809101112131426"; // última dezena = 26, fora de 01-25

            // Act
            ValidationResult result = await _sut.ValidateAsync(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Numbers");
        }
    }
}
