using Lotofacil.Application.Common;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.Contests;
using Lotofacil.Application.Features.Contests.DTO;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Tests.DataBuilder;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Shouldly;

namespace Lotofacil.Tests.Features.Contests
{
    public class ContestServiceTests : IDisposable
    {
        private readonly IRepository<Lotofacil.Domain.Entities.Contest> _repositoryMock;
        private readonly IContestManagementService _contestMSMock;
        private readonly MemoryCache _cache;
        private readonly ContestService _sut;

        public ContestServiceTests()
        {
            _repositoryMock = Substitute.For<IRepository<Lotofacil.Domain.Entities.Contest>>();
            _contestMSMock = Substitute.For<IContestManagementService>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _sut = new ContestService(_repositoryMock, _contestMSMock, _cache);
        }

        public void Dispose() => _cache.Dispose();

        [Fact(DisplayName = "SUCESSO - Deve ordenar os concursos por data decrescente quando o sortOrder é DateDesc")]
        public async Task GetContestsOrderedAsync_WithDateDesc_ShouldOrderDescending()
        {
            // Arrange
            var older = ContestDataBuilder.Create().WithData(new DateTime(2026, 1, 1)).Build();
            var newer = ContestDataBuilder.Create().WithData(new DateTime(2026, 6, 1)).Build();
            _repositoryMock.GetAllAsync().Returns(new List<Lotofacil.Domain.Entities.Contest> { older, newer });

            // Act
            var result = (await _sut.GetContestsOrderedAsync("DateDesc")).ToList();

            // Assert
            result[0].ShouldBe(newer);
            result[1].ShouldBe(older);
        }

        [Fact(DisplayName = "SUCESSO - Deve ordenar os concursos por data crescente quando o sortOrder é DateAsc")]
        public async Task GetContestsOrderedAsync_WithDateAsc_ShouldOrderAscending()
        {
            // Arrange
            var older = ContestDataBuilder.Create().WithData(new DateTime(2026, 1, 1)).Build();
            var newer = ContestDataBuilder.Create().WithData(new DateTime(2026, 6, 1)).Build();
            _repositoryMock.GetAllAsync().Returns(new List<Lotofacil.Domain.Entities.Contest> { newer, older });

            // Act
            var result = (await _sut.GetContestsOrderedAsync("DateAsc")).ToList();

            // Assert
            result[0].ShouldBe(older);
            result[1].ShouldBe(newer);
        }

        [Fact(DisplayName = "SUCESSO - Deve ordenar por data decrescente quando o sortOrder não é reconhecido")]
        public async Task GetContestsOrderedAsync_WithUnknownSortOrder_ShouldDefaultToDateDescending()
        {
            // Arrange
            var older = ContestDataBuilder.Create().WithData(new DateTime(2026, 1, 1)).Build();
            var newer = ContestDataBuilder.Create().WithData(new DateTime(2026, 6, 1)).Build();
            _repositoryMock.GetAllAsync().Returns(new List<Lotofacil.Domain.Entities.Contest> { older, newer });

            // Act
            var result = (await _sut.GetContestsOrderedAsync("qualquer-coisa")).ToList();

            // Assert
            result[0].ShouldBe(newer);
            result[1].ShouldBe(older);
        }

        [Fact(DisplayName = "SUCESSO - Não deve consultar o repositório novamente quando o cache já possui valor")]
        public async Task GetContestsOrderedAsync_WhenCacheHasValue_ShouldNotQueryRepositoryAgain()
        {
            // Arrange
            var contests = ContestDataBuilder.AsList(2);
            _repositoryMock.GetAllAsync().Returns(contests);

            // Act
            await _sut.GetContestsOrderedAsync("DateDesc");
            await _sut.GetContestsOrderedAsync("DateDesc");

            // Assert
            await _repositoryMock.Received(1).GetAllAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar lista vazia sem cachear quando o repositório não possui concursos")]
        public async Task GetContestsOrderedAsync_WhenRepositoryReturnsEmpty_ShouldReturnEmptyWithoutCaching()
        {
            // Arrange
            _repositoryMock.GetAllAsync().Returns(new List<Lotofacil.Domain.Entities.Contest>());

            // Act
            var first = await _sut.GetContestsOrderedAsync("DateDesc");
            var second = await _sut.GetContestsOrderedAsync("DateDesc");

            // Assert
            first.ShouldBeEmpty();
            second.ShouldBeEmpty();
            await _repositoryMock.Received(2).GetAllAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve formatar e salvar o novo concurso e invalidar o cache")]
        public async Task Create_WhenCalled_ShouldFormatAndSaveThenInvalidateCache()
        {
            // Arrange
            var existingContests = ContestDataBuilder.AsList(2);
            _repositoryMock.GetAllAsync().Returns(existingContests);
            await _sut.GetContestsOrderedAsync("DateDesc"); // primeiro chamada, primeia o cache
            _repositoryMock.ClearReceivedCalls();

            var contestVM = new ContestViewModel { Name = "XYZ", Data = new DateTime(2026, 6, 1, 10, 0, 0), Numbers = "010203040506070809101112131415" };
            var formattedDate = new DateTime(2026, 6, 1, 20, 0, 0);
            _contestMSMock.SetDataHour(contestVM.Data).Returns(formattedDate);
            _contestMSMock.FormatNumbersToSave(contestVM.Numbers).Returns("01-02-03-04-05-06-07-08-09-10-11-12-13-14-15");

            // Act
            _sut.Create(contestVM);
            await _sut.GetContestsOrderedAsync("DateDesc");

            // Assert
            _repositoryMock.Received(1).SaveAdd(Arg.Is<Lotofacil.Domain.Entities.Contest>(c =>
                c.Name == "Concurso XYZ" &&
                c.Data == formattedDate &&
                c.Numbers == "01-02-03-04-05-06-07-08-09-10-11-12-13-14-15"));
            // Só é chamado de novo porque Create invalidou o cache
            await _repositoryMock.Received(1).GetAllAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve calcular estatísticas de par/ímpar/múltiplos de 3 e os 5 números mais/menos frequentes")]
        public async Task AnalisarConcursos_WhenIdsAreValid_ShouldComputeStatistics()
        {
            // Arrange
            var contestA = ContestDataBuilder.Create().WithId(1).Build();
            var contestB = ContestDataBuilder.Create().WithId(2).Build();
            _repositoryMock.GetByIdAsync(1).Returns(contestA);
            _repositoryMock.GetByIdAsync(2).Returns(contestB);
            _contestMSMock.ConvertFormattedStringToList(contestA.Numbers)
                .Returns(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            _contestMSMock.ConvertFormattedStringToList(contestB.Numbers)
                .Returns(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16 });
            var request = new ContestModalRequestDTO(new List<int> { 1, 2 });

            // Act
            var result = await _sut.AnalisarConcursos(request);

            // Assert
            result.ContestsName.ShouldBe(new List<string> { contestA.Name, contestB.Name });
            result.EvenNumbersAveragePercentage.ShouldBe(50);
            result.OddNumbersAveragePercentage.ShouldBe(50);
            result.MultiplesOfThreeAveragePercentage.ShouldBe(30);
            result.Top5MostFrequentNumbers.ShouldBe(new List<int> { 1, 2, 3, 4, 5 });
            result.Top5LeastFrequentNumbers.ShouldBe(new List<int> { 17, 18, 19, 20, 21 });
        }

        [Fact(DisplayName = "SUCESSO - Deve ignorar IDs de concursos não encontrados e considerar somente os válidos")]
        public async Task AnalisarConcursos_WhenSomeIdsAreNotFound_ShouldSkipMissingContests()
        {
            // Arrange
            var contestA = ContestDataBuilder.Create().WithId(1).Build();
            _repositoryMock.GetByIdAsync(1).Returns(contestA);
            _repositoryMock.GetByIdAsync(999).Returns((Lotofacil.Domain.Entities.Contest)null!);
            _contestMSMock.ConvertFormattedStringToList(contestA.Numbers)
                .Returns(new List<int> { 1, 2, 3 });
            var request = new ContestModalRequestDTO(new List<int> { 1, 999 });

            // Act
            var result = await _sut.AnalisarConcursos(request);

            // Assert
            result.ContestsName.ShouldBe(new List<string> { contestA.Name });
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar resposta vazia quando nenhum ID informado corresponde a um concurso válido")]
        public async Task AnalisarConcursos_WhenNoValidContestsFound_ShouldReturnEmptyResponse()
        {
            // Arrange
            _repositoryMock.GetByIdAsync(998).Returns((Lotofacil.Domain.Entities.Contest)null!);
            _repositoryMock.GetByIdAsync(999).Returns((Lotofacil.Domain.Entities.Contest)null!);
            var request = new ContestModalRequestDTO(new List<int> { 998, 999 });

            // Act
            var result = await _sut.AnalisarConcursos(request);

            // Assert
            result.ContestsName.ShouldBeEmpty();
            result.EvenNumbersAveragePercentage.ShouldBe(0);
            result.OddNumbersAveragePercentage.ShouldBe(0);
            result.Top5MostFrequentNumbers.ShouldBeEmpty();
            result.Top5LeastFrequentNumbers.ShouldBeEmpty();
            result.MultiplesOfThreeAveragePercentage.ShouldBe(0);
        }
    }
}
