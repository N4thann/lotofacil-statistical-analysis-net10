using Lotofacil.Domain.Interfaces;

namespace Lotofacil.Application.Common
{
    public class InitializationDbService : IInitializationDbService
    {
        private readonly IDataInitializer _dataInitializer;

        public InitializationDbService(IDataInitializer dataInitializer) => _dataInitializer = dataInitializer;

        public void Initialize()
        {
            _dataInitializer.Seed();
        }
    }
}
