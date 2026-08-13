using Lotofacil.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Lotofacil.Tests.TestSupport
{
    /// <summary>
    /// Cria um ApplicationDbContext real, apoiado no provider InMemory do EF Core, com um nome de
    /// banco único por chamada. Usado só nos métodos que precisam de Include/ToListAsync/CountAsync
    /// reais (ver design doc da Etapa 2) — o resto do projeto usa NSubstitute normalmente.
    /// </summary>
    public static class InMemoryDbContextFactory
    {
        public static ApplicationDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
