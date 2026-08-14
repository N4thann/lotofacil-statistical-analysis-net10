using Lotofacil.Domain.Entities;
using Lotofacil.Infra.Data.Repositories;
using Lotofacil.Tests.DataBuilder;
using Lotofacil.Tests.TestSupport;
using Shouldly;

namespace Lotofacil.Tests.Repositories
{
    public class BaseContestRepositoryTests
    {
        [Fact(DisplayName = "SUCESSO - Deve carregar o concurso base com os concursos relacionados em uma única consulta")]
        public async Task GetByIdAsync_WhenExists_ShouldReturnWithContestsAbove11Loaded()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var contestA = ContestDataBuilder.Create().Build();
            var contestB = ContestDataBuilder.Create().Build();
            var baseContest = BaseContestDataBuilder.Create()
                .WithContestsAbove11(new List<Contest> { contestA, contestB })
                .Build();
            context.BaseContests.Add(baseContest);
            await context.SaveChangesAsync();

            var sut = new BaseContestRepository(context);

            // Act
            var result = await sut.GetByIdAsync(baseContest.Id);

            // Assert
            result.ShouldNotBeNull();
            result.ContestsAbove11.Count.ShouldBe(2);
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar null quando o concurso base não existe")]
        public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var seeded = BaseContestDataBuilder.Create().Build();
            context.BaseContests.Add(seeded);
            await context.SaveChangesAsync();

            var sut = new BaseContestRepository(context);

            // Act
            var result = await sut.GetByIdAsync(seeded.Id + 1);

            // Assert
            result.ShouldBeNull();
        }
    }
}
