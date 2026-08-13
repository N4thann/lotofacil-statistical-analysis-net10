using Bogus;
using Lotofacil.Domain.Entities;

namespace Lotofacil.Tests.DataBuilder
{
    public class ContestActivityLogDataBuilder
    {
        private readonly ContestActivityLog _instance;

        public ContestActivityLogDataBuilder()
        {
            var faker = new Faker<ContestActivityLog>("pt_BR")
                .CustomInstantiator(f => new ContestActivityLog(
                    $"Concurso {f.Random.Number(1, 9999)}",
                    BaseContestDataBuilder.RandomNumbersString(f),
                    f.Date.Past(2),
                    $"Concurso Base {f.Random.Number(1, 9999)}",
                    BaseContestDataBuilder.RandomNumbersString(f),
                    f.Random.Number(11, 15)));

            _instance = faker.Generate();
        }

        public static ContestActivityLogDataBuilder Create() => new();
        public ContestActivityLog Build() => _instance;
        public static implicit operator ContestActivityLog(ContestActivityLogDataBuilder builder) => builder.Build();

        public static List<ContestActivityLog> AsList(int count)
        {
            var list = new List<ContestActivityLog>();
            for (int i = 0; i < count; i++) list.Add(Create().Build());
            return list;
        }

        public ContestActivityLogDataBuilder WithId(int id)
        {
            typeof(ContestActivityLog).GetProperty(nameof(ContestActivityLog.Id))?.SetValue(_instance, id);
            return this;
        }

        public ContestActivityLogDataBuilder WithName(string name)
        {
            _instance.Name = name;
            return this;
        }

        public ContestActivityLogDataBuilder WithNumbers(string numbers)
        {
            _instance.Numbers = numbers;
            return this;
        }

        public ContestActivityLogDataBuilder WithData(DateTime data)
        {
            _instance.Data = data;
            return this;
        }

        public ContestActivityLogDataBuilder WithBaseContestName(string baseContestName)
        {
            typeof(ContestActivityLog).GetProperty(nameof(ContestActivityLog.BaseContestName))?.SetValue(_instance, baseContestName);
            return this;
        }

        public ContestActivityLogDataBuilder WithBaseContestNumbers(string baseContestNumbers)
        {
            typeof(ContestActivityLog).GetProperty(nameof(ContestActivityLog.BaseContestNumbers))?.SetValue(_instance, baseContestNumbers);
            return this;
        }

        public ContestActivityLogDataBuilder WithCountHits(int countHits)
        {
            typeof(ContestActivityLog).GetProperty(nameof(ContestActivityLog.CountHits))?.SetValue(_instance, countHits);
            return this;
        }
    }
}
