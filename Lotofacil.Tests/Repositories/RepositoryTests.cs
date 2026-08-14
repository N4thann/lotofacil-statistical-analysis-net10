using Lotofacil.Domain.Entities;
using Lotofacil.Infra.Data.Context;
using Lotofacil.Infra.Data.Repositories;
using Lotofacil.Tests.DataBuilder;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Lotofacil.Tests.Repositories
{
    public class RepositoryTests
    {
        [Fact(DisplayName = "SUCESSO - Deve persistir a entidade antes de retornar, visível a partir de um novo DbContext")]
        public async Task SaveAdd_WhenCalled_ShouldPersistEntityBeforeReturning()
        {
            // Arrange
            // Dois DbContext separados apontando para o mesmo banco InMemory nomeado: provar que o
            // SaveChangesAsync realmente completou (visível fora do change tracker do context de escrita),
            // não só que a mesma instância de contexto "lembra" da entidade adicionada.
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            using var writeContext = new ApplicationDbContext(options);
            var sut = new Repository<ContestActivityLog>(writeContext);
            var log = ContestActivityLogDataBuilder.Create().Build();

            // Act
            sut.SaveAdd(log);

            // Assert
            using var readContext = new ApplicationDbContext(options);
            var count = await readContext.ContestActivityLogs.CountAsync();
            count.ShouldBe(1);
        }

        [Fact(DisplayName = "SUCESSO - Deve propagar a exceção de forma síncrona quando a gravação falha")]
        public void SaveAdd_WhenSaveChangesFails_ShouldPropagateExceptionSynchronously()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            context.Dispose(); // context disposed before use forces SaveChangesAsync to throw synchronously from SaveAdd
            var sut = new Repository<ContestActivityLog>(context);
            var log = ContestActivityLogDataBuilder.Create().Build();

            // Act
            var act = () => sut.SaveAdd(log);

            // Assert
            Should.Throw<ObjectDisposedException>(act);
        }
    }
}
