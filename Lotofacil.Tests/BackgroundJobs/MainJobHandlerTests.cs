using Lotofacil.Application.BackgroundJobs;
using Lotofacil.Application.Common;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Tests.DataBuilder;
using NSubstitute;
using Shouldly;

namespace Lotofacil.Tests.BackgroundJobs
{
    public class MainJobHandlerTests
    {
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IBaseContestRepository _repositoryBCMock;
        private readonly IContestRepository _repositoryCMock;
        private readonly IContestManagementService _contestMSMock;
        private readonly IRepository<BaseContest> _baseContestEntityRepoMock;
        private readonly IRepository<Contest> _contestEntityRepoMock;
        private readonly IRepository<ContestActivityLog> _logEntityRepoMock;
        private readonly MainJobHandler _sut;

        public MainJobHandlerTests()
        {
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _repositoryBCMock = Substitute.For<IBaseContestRepository>();
            _repositoryCMock = Substitute.For<IContestRepository>();
            _contestMSMock = Substitute.For<IContestManagementService>();

            _baseContestEntityRepoMock = Substitute.For<IRepository<BaseContest>>();
            _contestEntityRepoMock = Substitute.For<IRepository<Contest>>();
            _logEntityRepoMock = Substitute.For<IRepository<ContestActivityLog>>();
            _unitOfWorkMock.Repository<BaseContest>().Returns(_baseContestEntityRepoMock);
            _unitOfWorkMock.Repository<Contest>().Returns(_contestEntityRepoMock);
            _unitOfWorkMock.Repository<ContestActivityLog>().Returns(_logEntityRepoMock);

            _sut = new MainJobHandler(_unitOfWorkMock, _repositoryBCMock, _repositoryCMock, _contestMSMock);
        }

        private void SeedOnePair(out BaseContest baseContest, out Contest contest, DateTime? contestLastProcessed = null)
        {
            baseContest = BaseContestDataBuilder.Create().WithName("Concurso Base A").WithData(new DateTime(2026, 1, 1)).WithCreatedAt(new DateTime(2026, 1, 1)).Build();
            contest = ContestDataBuilder.Create().WithName("Concurso B").WithData(new DateTime(2026, 2, 1)).WithLastProcessedMainJob(contestLastProcessed).Build();
            _repositoryBCMock.GetAllWithContestsAbove11Async().Returns(new List<BaseContest> { baseContest });
            _repositoryCMock.GetAllWithBaseContestsAsync().Returns(new List<Contest> { contest });
            _contestMSMock.ConvertFormattedStringToList(Arg.Any<string>()).Returns(new List<int> { 1 });
        }

        [Fact(DisplayName = "SUCESSO - Deve criar log de atividade, vincular as entidades e incrementar Hit11 quando a interseção é 11")]
        public async Task ExecuteAsync_WhenIntersectionIs11_ShouldCreateActivityLogAndIncrementHit11()
        {
            // Arrange
            SeedOnePair(out var baseContest, out var contest);
            _contestMSMock.CalculateIntersection(Arg.Any<List<int>>(), Arg.Any<List<int>>()).Returns(11);

            // Act
            await _sut.ExecuteAsync();

            // Assert
            baseContest.Hit11.ShouldBe(1);
            baseContest.ContestsAbove11.ShouldContain(contest);
            contest.BaseContests.ShouldContain(baseContest);
            await _logEntityRepoMock.Received(1).AddRangeAsync(Arg.Is<IEnumerable<ContestActivityLog>>(logs => logs.Count() == 1 && logs.First().CountHits == 11));
            _baseContestEntityRepoMock.Received(1).Update(baseContest);
            _contestEntityRepoMock.Received(1).Update(contest);
            await _unitOfWorkMock.Received(1).CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve incrementar Hit12 quando a interseção é 12")]
        public async Task ExecuteAsync_WhenIntersectionIs12_ShouldIncrementHit12()
        {
            // Arrange
            SeedOnePair(out var baseContest, out _);
            _contestMSMock.CalculateIntersection(Arg.Any<List<int>>(), Arg.Any<List<int>>()).Returns(12);

            // Act
            await _sut.ExecuteAsync();

            // Assert
            baseContest.Hit12.ShouldBe(1);
        }

        [Fact(DisplayName = "SUCESSO - Deve incrementar Hit13 quando a interseção é 13")]
        public async Task ExecuteAsync_WhenIntersectionIs13_ShouldIncrementHit13()
        {
            // Arrange
            SeedOnePair(out var baseContest, out _);
            _contestMSMock.CalculateIntersection(Arg.Any<List<int>>(), Arg.Any<List<int>>()).Returns(13);

            // Act
            await _sut.ExecuteAsync();

            // Assert
            baseContest.Hit13.ShouldBe(1);
        }

        [Fact(DisplayName = "SUCESSO - Deve incrementar Hit14 quando a interseção é 14")]
        public async Task ExecuteAsync_WhenIntersectionIs14_ShouldIncrementHit14()
        {
            // Arrange
            SeedOnePair(out var baseContest, out _);
            _contestMSMock.CalculateIntersection(Arg.Any<List<int>>(), Arg.Any<List<int>>()).Returns(14);

            // Act
            await _sut.ExecuteAsync();

            // Assert
            baseContest.Hit14.ShouldBe(1);
        }

        [Fact(DisplayName = "SUCESSO - Deve incrementar Hit15 quando a interseção é 15")]
        public async Task ExecuteAsync_WhenIntersectionIs15_ShouldIncrementHit15()
        {
            // Arrange
            SeedOnePair(out var baseContest, out _);
            _contestMSMock.CalculateIntersection(Arg.Any<List<int>>(), Arg.Any<List<int>>()).Returns(15);

            // Act
            await _sut.ExecuteAsync();

            // Assert
            baseContest.Hit15.ShouldBe(1);
        }

        [Fact(DisplayName = "SUCESSO - Não deve criar log de atividade nem vincular entidades quando a interseção é 10 ou menos")]
        public async Task ExecuteAsync_WhenIntersectionIs10OrLess_ShouldNotCreateActivityLogOrLinkEntities()
        {
            // Arrange
            SeedOnePair(out var baseContest, out var contest);
            _contestMSMock.CalculateIntersection(Arg.Any<List<int>>(), Arg.Any<List<int>>()).Returns(10);

            // Act
            await _sut.ExecuteAsync();

            // Assert
            baseContest.Hit11.ShouldBe(0);
            baseContest.ContestsAbove11.ShouldNotContain(contest);
            await _logEntityRepoMock.DidNotReceiveWithAnyArgs().AddRangeAsync(default!);
        }

        [Fact(DisplayName = "SUCESSO - Deve pular a comparação quando o concurso base e o concurso têm o mesmo nome")]
        public async Task ExecuteAsync_WhenSameName_ShouldSkipComparison()
        {
            // Arrange
            var baseContest = BaseContestDataBuilder.Create().WithName("Concurso Igual").WithData(new DateTime(2026, 1, 1)).Build();
            var contest = ContestDataBuilder.Create().WithName("Concurso Igual").WithData(new DateTime(2026, 2, 1)).Build();
            _repositoryBCMock.GetAllWithContestsAbove11Async().Returns(new List<BaseContest> { baseContest });
            _repositoryCMock.GetAllWithBaseContestsAsync().Returns(new List<Contest> { contest });
            _contestMSMock.ConvertFormattedStringToList(Arg.Any<string>()).Returns(new List<int> { 1 });

            // Act
            await _sut.ExecuteAsync();

            // Assert
            _contestMSMock.DidNotReceiveWithAnyArgs().CalculateIntersection(default!, default!);
            baseContest.ContestsAbove11.ShouldNotContain(contest);
            await _unitOfWorkMock.Received(1).CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve pular a comparação quando o concurso base e o concurso têm a mesma data")]
        public async Task ExecuteAsync_WhenSameDate_ShouldSkipComparison()
        {
            // Arrange
            var sameDate = new DateTime(2026, 1, 1);
            var baseContest = BaseContestDataBuilder.Create().WithName("Concurso Base A").WithData(sameDate).Build();
            var contest = ContestDataBuilder.Create().WithName("Concurso B").WithData(sameDate).Build();
            _repositoryBCMock.GetAllWithContestsAbove11Async().Returns(new List<BaseContest> { baseContest });
            _repositoryCMock.GetAllWithBaseContestsAsync().Returns(new List<Contest> { contest });
            _contestMSMock.ConvertFormattedStringToList(Arg.Any<string>()).Returns(new List<int> { 1 });

            // Act
            await _sut.ExecuteAsync();

            // Assert
            _contestMSMock.DidNotReceiveWithAnyArgs().CalculateIntersection(default!, default!);
            baseContest.ContestsAbove11.ShouldNotContain(contest);
        }

        [Fact(DisplayName = "SUCESSO - Deve pular a comparação quando o concurso já foi processado após a criação do concurso base")]
        public async Task ExecuteAsync_WhenContestAlreadyProcessedAfterBaseContestCreation_ShouldSkipComparison()
        {
            // Arrange
            SeedOnePair(out _, out _, contestLastProcessed: new DateTime(2026, 1, 15)); // depois de CreatedAt (2026-01-01)

            // Act
            await _sut.ExecuteAsync();

            // Assert
            _contestMSMock.DidNotReceiveWithAnyArgs().CalculateIntersection(default!, default!);
        }

        [Fact(DisplayName = "SUCESSO - Deve capturar e logar a exceção sem propagá-la")]
        public async Task ExecuteAsync_WhenExceptionIsThrown_ShouldLogAndNotPropagate()
        {
            // Arrange
            _repositoryBCMock.GetAllWithContestsAbove11Async().Returns(Task.FromException<IEnumerable<BaseContest>>(new InvalidOperationException("falha simulada")));

            // Act
            // MainJobHandler.ExecuteAsync captura e loga qualquer exceção internamente — se este await
            // lançar, o xUnit falha o teste automaticamente, provando que a exceção NÃO escapou do método.
            await _sut.ExecuteAsync();

            // Assert
            await _unitOfWorkMock.DidNotReceiveWithAnyArgs().CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Não deve processar nada quando não há concursos base ou concursos")]
        public async Task ExecuteAsync_WhenNoBaseContestsOrNoContests_ShouldNotProcessAnything()
        {
            // Arrange
            _repositoryBCMock.GetAllWithContestsAbove11Async().Returns(new List<BaseContest>());
            _repositoryCMock.GetAllWithBaseContestsAsync().Returns(ContestDataBuilder.AsList(1));

            // Act
            await _sut.ExecuteAsync();

            // Assert
            await _unitOfWorkMock.DidNotReceiveWithAnyArgs().CompleteAsync();
        }
    }
}
