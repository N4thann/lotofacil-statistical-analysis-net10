using Lotofacil.Application.Features.ContestActivityLogs;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Infra.Data.Repositories;
using Lotofacil.Tests.DataBuilder;
using Lotofacil.Tests.TestSupport;
using NSubstitute;
using Shouldly;

namespace Lotofacil.Tests.Features.ContestActivityLogs
{
    public class ContestActivityLogServiceTests
    {
        private readonly IUnitOfWork _unitOfWorkMock;

        public ContestActivityLogServiceTests()
        {
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        }

        [Fact(DisplayName = "SUCESSO - Deve excluir somente os logs cujo nome do concurso base contém o nome informado")]
        public async Task DeleteAllReferencesOfLogByBaseContest_WhenCalled_ShouldDeleteOnlyMatchingLogsByContains()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var log1 = ContestActivityLogDataBuilder.Create().WithBaseContestName("Concurso Base 1").Build();
            var log2 = ContestActivityLogDataBuilder.Create().WithBaseContestName("Concurso Base 10").Build();
            var log3 = ContestActivityLogDataBuilder.Create().WithBaseContestName("Concurso Base 2").Build();
            context.ContestActivityLogs.AddRange(log1, log2, log3);
            await context.SaveChangesAsync();

            var realRepository = new Repository<Lotofacil.Domain.Entities.ContestActivityLog>(context);
            var logRepositoryMock = Substitute.For<IRepository<Lotofacil.Domain.Entities.ContestActivityLog>>();
            _unitOfWorkMock.Repository<Lotofacil.Domain.Entities.ContestActivityLog>().Returns(logRepositoryMock);
            var sut = new ContestActivityLogService(realRepository, _unitOfWorkMock);

            // Act
            await sut.DeleteAllReferencesOfLogByBaseContest("Concurso Base 1");

            // Assert
            // "Concurso Base 10" também é excluído porque BaseContestName.Contains("Concurso Base 1") é
            // verdadeiro — comportamento real preservado (não corrigido nesta etapa, ver design doc);
            // só a filtragem migrou de memória para SQL.
            logRepositoryMock.Received(1).Delete(log1);
            logRepositoryMock.Received(1).Delete(log2);
            logRepositoryMock.DidNotReceive().Delete(log3);
            await _unitOfWorkMock.Received(1).CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Não deve excluir nenhum log quando nenhum nome de concurso base corresponde")]
        public async Task DeleteAllReferencesOfLogByBaseContest_WhenNoLogsMatch_ShouldNotDeleteAnything()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var log1 = ContestActivityLogDataBuilder.Create().WithBaseContestName("Concurso Base 2").Build();
            context.ContestActivityLogs.Add(log1);
            await context.SaveChangesAsync();

            var realRepository = new Repository<Lotofacil.Domain.Entities.ContestActivityLog>(context);
            var logRepositoryMock = Substitute.For<IRepository<Lotofacil.Domain.Entities.ContestActivityLog>>();
            _unitOfWorkMock.Repository<Lotofacil.Domain.Entities.ContestActivityLog>().Returns(logRepositoryMock);
            var sut = new ContestActivityLogService(realRepository, _unitOfWorkMock);

            // Act
            await sut.DeleteAllReferencesOfLogByBaseContest("Concurso Base 1");

            // Assert
            logRepositoryMock.DidNotReceiveWithAnyArgs().Delete(default!);
            await _unitOfWorkMock.Received(1).CompleteAsync();
        }

        [Fact(DisplayName = "SUCESSO - Deve filtrar por nome e data e paginar os resultados ordenados por data decrescente")]
        public async Task GetFilteredContestActivityLogsAsync_WhenFiltersAndPaginationAreApplied_ShouldReturnMatchingPage()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var alpha1 = ContestActivityLogDataBuilder.Create().WithName("Concurso Alpha1").WithData(new DateTime(2026, 1, 10)).Build();
            var alpha2 = ContestActivityLogDataBuilder.Create().WithName("Concurso Alpha2").WithData(new DateTime(2026, 2, 10)).Build();
            var beta = ContestActivityLogDataBuilder.Create().WithName("Concurso Beta").WithData(new DateTime(2026, 3, 10)).Build();
            context.ContestActivityLogs.AddRange(alpha1, alpha2, beta);
            await context.SaveChangesAsync();

            var realRepository = new Repository<Lotofacil.Domain.Entities.ContestActivityLog>(context);
            var sut = new ContestActivityLogService(realRepository, _unitOfWorkMock);

            // Act
            var result = await sut.GetFilteredContestActivityLogsAsync(name: "Alpha", startDate: null, endDate: null, pageNumber: 1, pageSize: 1);

            // Assert
            result.Count.ShouldBe(1);
            // ordenação decrescente por data — mais recente primeiro
            result[0].Name.ShouldBe("Concurso Alpha2");
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar o total de registros filtrados por nome")]
        public async Task GetTotalCountAsync_WhenNameFilterIsApplied_ShouldReturnMatchingCount()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var alpha1 = ContestActivityLogDataBuilder.Create().WithName("Concurso Alpha1").Build();
            var alpha2 = ContestActivityLogDataBuilder.Create().WithName("Concurso Alpha2").Build();
            var beta = ContestActivityLogDataBuilder.Create().WithName("Concurso Beta").Build();
            context.ContestActivityLogs.AddRange(alpha1, alpha2, beta);
            await context.SaveChangesAsync();

            var realRepository = new Repository<Lotofacil.Domain.Entities.ContestActivityLog>(context);
            var sut = new ContestActivityLogService(realRepository, _unitOfWorkMock);

            // Act
            var result = await sut.GetTotalCountAsync(name: "Alpha", startDate: null, endDate: null);

            // Assert
            result.ShouldBe(2);
        }
    }
}
