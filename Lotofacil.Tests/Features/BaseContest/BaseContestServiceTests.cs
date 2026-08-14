using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.BaseContests;
using Lotofacil.Application.Features.ContestActivityLogs;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Infra.Data.Repositories;
using Lotofacil.Tests.DataBuilder;
using Lotofacil.Tests.TestSupport;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Lotofacil.Tests.Features.BaseContests
{
    public class BaseContestServiceTests
    {
        private readonly IRepository<BaseContest> _repositoryMock;
        private readonly IContestManagementService _contestMSMock;
        private readonly IBaseContestRepository _repositoryBCMock;
        private readonly IContestActivityLogService _activityLSMock;
        private readonly ILogger<BaseContestService> _loggerMock;
        private readonly BaseContestService _sut;

        public BaseContestServiceTests()
        {
            _repositoryMock = Substitute.For<IRepository<BaseContest>>();
            _contestMSMock = Substitute.For<IContestManagementService>();
            _repositoryBCMock = Substitute.For<IBaseContestRepository>();
            _activityLSMock = Substitute.For<IContestActivityLogService>();
            _loggerMock = Substitute.For<ILogger<BaseContestService>>();
            _sut = new BaseContestService(_repositoryMock, _contestMSMock, _repositoryBCMock, _activityLSMock, _loggerMock);
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar todos os concursos base do repositório")]
        public async Task GetAllBaseContestAsync_WhenCalled_ShouldReturnAllFromRepository()
        {
            // Arrange
            var baseContests = BaseContestDataBuilder.AsList(3);
            _repositoryMock.GetAllAsync().Returns(baseContests);

            // Act
            var result = await _sut.GetAllBaseContestAsync();

            // Assert
            result.ShouldBe(baseContests);
        }

        [Fact(DisplayName = "SUCESSO - Deve formatar nome e números e salvar o novo concurso base")]
        public void Create_WhenDataIsValid_ShouldFormatNameNumbersAndSaveEntity()
        {
            // Arrange
            var contestVM = new ContestViewModel { Name = "ABC", Data = new DateTime(2026, 1, 1, 10, 0, 0), Numbers = "010203040506070809101112131415" };
            var formattedDate = new DateTime(2026, 1, 1, 20, 0, 0);
            _contestMSMock.SetDataHour(contestVM.Data).Returns(formattedDate);
            _contestMSMock.FormatNumbersToSave(contestVM.Numbers).Returns("01-02-03-04-05-06-07-08-09-10-11-12-13-14-15");

            // Act
            _sut.Create(contestVM);

            // Assert
            _repositoryMock.Received(1).SaveAdd(Arg.Is<BaseContest>(b =>
                b.Name == "Concurso ABC" &&
                b.Data == formattedDate &&
                b.Numbers == "01-02-03-04-05-06-07-08-09-10-11-12-13-14-15"));
        }

        [Fact(DisplayName = "ERRO - Deve relançar a exceção quando o repositório falha ao salvar")]
        public void Create_WhenRepositoryThrows_ShouldRethrow()
        {
            // Arrange
            var contestVM = new ContestViewModel { Name = "ABC", Data = DateTime.Now, Numbers = "010203040506070809101112131415" };
            _contestMSMock.SetDataHour(Arg.Any<DateTime>()).Returns(DateTime.Now);
            _contestMSMock.FormatNumbersToSave(Arg.Any<string>()).Returns("01-02-03-04-05-06-07-08-09-10-11-12-13-14-15");
            _repositoryMock.When(r => r.SaveAdd(Arg.Any<BaseContest>())).Do(_ => throw new InvalidOperationException("falha de banco"));

            // Act
            var act = () => _sut.Create(contestVM);

            // Assert
            Should.Throw<InvalidOperationException>(act);
        }

        [Fact(DisplayName = "SUCESSO - Deve mapear o concurso base encontrado para o ViewModel")]
        public async Task ShowOnScreen_WhenExists_ShouldMapEntityToViewModel()
        {
            // Arrange
            var existing = BaseContestDataBuilder.Create().WithId(5).WithName("Concurso X").Build();
            _repositoryMock.GetByIdAsync(5).Returns(existing);

            // Act
            var result = await _sut.ShowOnScreen(5);

            // Assert
            result.Id.ShouldBe(existing.Id);
            result.Name.ShouldBe(existing.Name);
            result.Data.ShouldBe(existing.Data);
            result.Numbers.ShouldBe(existing.Numbers);
        }

        [Fact(DisplayName = "ERRO - Deve lançar KeyNotFoundException ao exibir um concurso base inexistente")]
        public async Task ShowOnScreen_WhenNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _repositoryMock.GetByIdAsync(999).Returns((BaseContest)null!);

            // Act
            var act = async () => await _sut.ShowOnScreen(999);

            // Assert
            await Should.ThrowAsync<KeyNotFoundException>(act);
        }

        [Fact(DisplayName = "SUCESSO - Deve delegar para o repositório específico de concursos base com concursos acima de 11")]
        public async Task GetAllWithContestsAbove11Async_WhenCalled_ShouldDelegateToBaseContestRepository()
        {
            // Arrange
            var baseContests = BaseContestDataBuilder.AsList(2);
            _repositoryBCMock.GetAllWithContestsAbove11Async().Returns(baseContests);

            // Act
            var result = await _sut.GetAllWithContestsAbove11Async();

            // Assert
            result.ShouldBe(baseContests);
        }

        [Fact(DisplayName = "SUCESSO - Deve excluir os logs de atividade relacionados antes de excluir o concurso base")]
        public async Task DeleteByIdAsync_WhenExists_ShouldDeleteActivityLogsThenEntity()
        {
            // Arrange
            var existing = BaseContestDataBuilder.Create().WithId(7).WithName("Concurso Y").Build();
            _repositoryMock.GetByIdAsync(7).Returns(existing);

            // Act
            await _sut.DeleteByIdAsync(7);

            // Assert
            await _activityLSMock.Received(1).DeleteAllReferencesOfLogByBaseContest("Concurso Y");
            await _repositoryMock.Received(1).SaveDeleteAsync(7);
        }

        [Fact(DisplayName = "ERRO - Deve lançar KeyNotFoundException ao excluir um concurso base inexistente")]
        public async Task DeleteByIdAsync_WhenNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _repositoryMock.GetByIdAsync(999).Returns((BaseContest)null!);

            // Act
            var act = async () => await _sut.DeleteByIdAsync(999);

            // Assert
            await Should.ThrowAsync<KeyNotFoundException>(act);
            await _activityLSMock.DidNotReceiveWithAnyArgs().DeleteAllReferencesOfLogByBaseContest(default!);
            await _repositoryMock.DidNotReceiveWithAnyArgs().SaveDeleteAsync(default);
        }

        [Fact(DisplayName = "SUCESSO - Deve filtrar por nome e data, paginar e projetar os campos resumidos incluindo a contagem de concursos relacionados")]
        public async Task GetFilteredBaseContestsAsync_WhenFiltersAndPaginationAreApplied_ShouldReturnMatchingPage()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var relatedContest = ContestDataBuilder.Create().Build();
            var alpha1 = BaseContestDataBuilder.Create()
                .WithName("Concurso Alpha1")
                .WithData(new DateTime(2026, 1, 10))
                .WithContestsAbove11(new List<Contest> { relatedContest })
                .Build();
            var alpha2 = BaseContestDataBuilder.Create().WithName("Concurso Alpha2").WithData(new DateTime(2026, 2, 10)).Build();
            var beta = BaseContestDataBuilder.Create().WithName("Concurso Beta").WithData(new DateTime(2026, 3, 10)).Build();
            context.BaseContests.AddRange(alpha1, alpha2, beta);
            await context.SaveChangesAsync();

            var realRepository = new Repository<BaseContest>(context);
            var sut = new BaseContestService(realRepository, _contestMSMock, _repositoryBCMock, _activityLSMock, _loggerMock);

            // Act
            var result = await sut.GetFilteredBaseContestsAsync(name: "Alpha", startDate: null, endDate: null, pageNumber: 1, pageSize: 1);

            // Assert
            result.Count.ShouldBe(1);
            result[0].Name.ShouldBe("Concurso Alpha1");
            result[0].ContestsAbove11Count.ShouldBe(1);
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar o total de registros filtrados por nome")]
        public async Task GetTotalCountAsync_WhenNameFilterIsApplied_ShouldReturnMatchingCount()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var alpha1 = BaseContestDataBuilder.Create().WithName("Concurso Alpha1").Build();
            var alpha2 = BaseContestDataBuilder.Create().WithName("Concurso Alpha2").Build();
            var beta = BaseContestDataBuilder.Create().WithName("Concurso Beta").Build();
            context.BaseContests.AddRange(alpha1, alpha2, beta);
            await context.SaveChangesAsync();

            var realRepository = new Repository<BaseContest>(context);
            var sut = new BaseContestService(realRepository, _contestMSMock, _repositoryBCMock, _activityLSMock, _loggerMock);

            // Act
            var result = await sut.GetTotalCountAsync(name: "Alpha", startDate: null, endDate: null);

            // Assert
            result.ShouldBe(2);
        }
    }
}
