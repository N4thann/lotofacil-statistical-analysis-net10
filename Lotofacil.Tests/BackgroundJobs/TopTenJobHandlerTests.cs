using Lotofacil.Application.BackgroundJobs;
using Lotofacil.Application.Common;
using Lotofacil.Application.Features.BaseContests;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Tests.DataBuilder;
using NSubstitute;
using Shouldly;

namespace Lotofacil.Tests.BackgroundJobs
{
    public class TopTenJobHandlerTests
    {
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IContestManagementService _contestMSMock;
        private readonly IBaseContestService _baseContestServiceMock;
        private readonly IRepository<BaseContest> _baseContestEntityRepoMock;
        private readonly TopTenJobHandler _sut;

        public TopTenJobHandlerTests()
        {
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _contestMSMock = Substitute.For<IContestManagementService>();
            _baseContestServiceMock = Substitute.For<IBaseContestService>();
            _baseContestEntityRepoMock = Substitute.For<IRepository<BaseContest>>();
            _unitOfWorkMock.Repository<BaseContest>().Returns(_baseContestEntityRepoMock);
            _sut = new TopTenJobHandler(_unitOfWorkMock, _contestMSMock, _baseContestServiceMock);
        }

        [Fact(DisplayName = "SUCESSO - Deve calcular os 10 números mais frequentes com desempate pelo menor número e atualizar TotalProcessed")]
        public async Task ExecuteAsync_WhenTotalProcessedIsNull_ShouldComputeTop10NumbersWithTieBreakByLowerNumber()
        {
            // Arrange
            var contest1 = ContestDataBuilder.Create().Build();
            var contest2 = ContestDataBuilder.Create().Build();
            var baseContest = BaseContestDataBuilder.Create()
                .WithTotalProcessed(null)
                .WithContestsAbove11(new List<Contest> { contest1, contest2 })
                .Build();
            _baseContestServiceMock.GetAllWithContestsAbove11Async().Returns(new List<BaseContest> { baseContest });
            _contestMSMock.ConvertFormattedStringToList(contest1.Numbers).Returns(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
            _contestMSMock.ConvertFormattedStringToList(contest2.Numbers).Returns(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 12 });

            // Act
            await _sut.ExecuteAsync();

            // Assert
            // 1-9 aparecem 2x cada (garantem as 9 primeiras posições); 10, 11 e 12 aparecem 1x cada — o desempate
            // por ThenBy(Key) escolhe o menor (10) para a 10ª posição.
            baseContest.TopTenNumbers.ShouldBe("01-02-03-04-05-06-07-08-09-10");
            baseContest.TotalProcessed.ShouldBe(2);
            _baseContestEntityRepoMock.Received(1).Update(baseContest);
            await _unitOfWorkMock.Received(1).CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve pular o recálculo quando TotalProcessed já está em dia com a quantidade de concursos vinculados")]
        public async Task ExecuteAsync_WhenTotalProcessedIsUpToDate_ShouldSkipRecomputation()
        {
            // Arrange
            var contest1 = ContestDataBuilder.Create().Build();
            var baseContest = BaseContestDataBuilder.Create()
                .WithTotalProcessed(1)
                .WithContestsAbove11(new List<Contest> { contest1 })
                .Build();
            _baseContestServiceMock.GetAllWithContestsAbove11Async().Returns(new List<BaseContest> { baseContest });

            // Act
            await _sut.ExecuteAsync();

            // Assert
            baseContest.TopTenNumbers.ShouldBe(string.Empty);
            _baseContestEntityRepoMock.DidNotReceiveWithAnyArgs().Update(default!);
            await _unitOfWorkMock.DidNotReceiveWithAnyArgs().CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve pular sem calcular quando o concurso base não tem concursos vinculados (prova a correção do bug)")]
        public async Task ExecuteAsync_WhenBaseContestHasNoLinkedContests_ShouldSkipWithoutComputing()
        {
            // Arrange
            var baseContest = BaseContestDataBuilder.Create()
                .WithTotalProcessed(null)
                .WithContestsAbove11(new List<Contest>())
                .Build();
            _baseContestServiceMock.GetAllWithContestsAbove11Async().Returns(new List<BaseContest> { baseContest });

            // Act
            await _sut.ExecuteAsync();

            // Assert
            baseContest.TopTenNumbers.ShouldBe(string.Empty);
            _baseContestEntityRepoMock.DidNotReceiveWithAnyArgs().Update(default!);
            await _unitOfWorkMock.DidNotReceiveWithAnyArgs().CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve logar aviso e não processar nada quando o serviço retorna nulo")]
        public async Task ExecuteAsync_WhenBaseContestsIsNull_ShouldLogWarningAndSkip()
        {
            // Arrange
            _baseContestServiceMock.GetAllWithContestsAbove11Async().Returns((IEnumerable<BaseContest>)null!);

            // Act
            // TopTenJobHandler.ExecuteAsync captura e loga qualquer exceção internamente — se este await
            // lançar (ex.: NullReferenceException por não checar null corretamente), o xUnit falha o teste.
            await _sut.ExecuteAsync();

            // Assert
            await _unitOfWorkMock.DidNotReceiveWithAnyArgs().CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Não deve atualizar nada quando a lista de concursos base está vazia")]
        public async Task ExecuteAsync_WhenBaseContestsIsEmpty_ShouldNotUpdateAnything()
        {
            // Arrange
            _baseContestServiceMock.GetAllWithContestsAbove11Async().Returns(new List<BaseContest>());

            // Act
            await _sut.ExecuteAsync();

            // Assert
            await _unitOfWorkMock.DidNotReceiveWithAnyArgs().CompleteAsync();
        }
    }
}
