using Lotofacil.Tests.DataBuilder;
using Lotofacil.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Lotofacil.Tests.DataBuilder
{
    public class DataBuildersSmokeTests
    {
        [Fact(DisplayName = "SUCESSO - BaseContestDataBuilder deve gerar uma entidade válida com 15 números únicos")]
        public void BaseContestDataBuilder_WhenCreated_ShouldProduceValidEntity()
        {
            // Arrange
            var builder = BaseContestDataBuilder.Create().WithHits(hit11: 2, hit15: 1);

            // Act
            var baseContest = builder.Build();

            // Assert
            baseContest.Name.ShouldNotBeNullOrEmpty();
            baseContest.Numbers.Split('-').Length.ShouldBe(15);
            baseContest.Numbers.Split('-').Distinct().Count().ShouldBe(15);
            baseContest.ContestsAbove11.ShouldNotBeNull();
            baseContest.Hit11.ShouldBe(2);
            baseContest.Hit15.ShouldBe(1);
        }

        [Fact(DisplayName = "SUCESSO - ContestDataBuilder deve gerar uma entidade válida com 15 números únicos")]
        public void ContestDataBuilder_WhenCreated_ShouldProduceValidEntity()
        {
            // Arrange
            var builder = ContestDataBuilder.Create();

            // Act
            var contest = builder.Build();

            // Assert
            contest.Name.ShouldNotBeNullOrEmpty();
            contest.Numbers.Split('-').Length.ShouldBe(15);
            contest.BaseContests.ShouldNotBeNull();
        }

        [Fact(DisplayName = "SUCESSO - ContestActivityLogDataBuilder deve gerar uma entidade válida com acertos entre 11 e 15")]
        public void ContestActivityLogDataBuilder_WhenCreated_ShouldProduceValidEntity()
        {
            // Arrange
            var builder = ContestActivityLogDataBuilder.Create().WithCountHits(13);

            // Act
            var log = builder.Build();

            // Assert
            log.Name.ShouldNotBeNullOrEmpty();
            log.BaseContestName.ShouldNotBeNullOrEmpty();
            log.CountHits.ShouldBe(13);
        }

        [Fact(DisplayName = "SUCESSO - InMemoryDbContextFactory deve criar um contexto funcional isolado por chamada")]
        public async Task InMemoryDbContextFactory_WhenCalledTwice_ShouldProduceIsolatedDatabases()
        {
            // Arrange
            using var context1 = InMemoryDbContextFactory.Create();
            using var context2 = InMemoryDbContextFactory.Create();

            // Act
            context1.BaseContests.Add(BaseContestDataBuilder.Create().Build());
            await context1.SaveChangesAsync();

            // Assert
            (await context1.BaseContests.CountAsync()).ShouldBe(1);
            (await context2.BaseContests.CountAsync()).ShouldBe(0);
        }
    }
}
