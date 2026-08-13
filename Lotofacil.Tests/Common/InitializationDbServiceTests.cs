using Lotofacil.Application.Common;
using Lotofacil.Domain.Interfaces;
using NSubstitute;

namespace Lotofacil.Tests.Common
{
    public class InitializationDbServiceTests
    {
        private readonly IDataInitializer _dataInitializerMock;
        private readonly InitializationDbService _sut;

        public InitializationDbServiceTests()
        {
            _dataInitializerMock = Substitute.For<IDataInitializer>();
            _sut = new InitializationDbService(_dataInitializerMock);
        }

        [Fact(DisplayName = "SUCESSO - Deve delegar a inicialização para o IDataInitializer.Seed()")]
        public void Initialize_WhenCalled_ShouldCallDataInitializerSeed()
        {
            // Arrange
            // (nada a preparar além do mock do construtor)

            // Act
            _sut.Initialize();

            // Assert
            _dataInitializerMock.Received(1).Seed();
        }
    }
}
