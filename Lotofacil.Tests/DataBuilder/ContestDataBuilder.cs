using Bogus;
using Lotofacil.Domain.Entities;

namespace Lotofacil.Tests.DataBuilder
{
    public class ContestDataBuilder
    {
        private readonly Contest _instance;

        public ContestDataBuilder()
        {
            var faker = new Faker<Contest>("pt_BR")
                .CustomInstantiator(f => new Contest(
                    $"Concurso {f.Random.Number(1, 9999)}",
                    f.Date.Past(2),
                    BaseContestDataBuilder.RandomNumbersString(f)));

            _instance = faker.Generate();
        }

        public static ContestDataBuilder Create() => new();
        public Contest Build() => _instance;
        public static implicit operator Contest(ContestDataBuilder builder) => builder.Build();

        public static List<Contest> AsList(int count)
        {
            var list = new List<Contest>();
            for (int i = 0; i < count; i++) list.Add(Create().Build());
            return list;
        }

        public ContestDataBuilder WithId(int id)
        {
            typeof(Contest).GetProperty(nameof(Contest.Id))?.SetValue(_instance, id);
            return this;
        }

        public ContestDataBuilder WithName(string name)
        {
            _instance.Name = name;
            return this;
        }

        public ContestDataBuilder WithData(DateTime data)
        {
            _instance.Data = data;
            return this;
        }

        public ContestDataBuilder WithNumbers(string numbers)
        {
            _instance.Numbers = numbers;
            return this;
        }

        public ContestDataBuilder WithLastProcessedMainJob(DateTime? lastProcessed)
        {
            _instance.LastProcessedMainJob = lastProcessed;
            return this;
        }

        public ContestDataBuilder WithBaseContests(List<BaseContest> baseContests)
        {
            _instance.BaseContests = baseContests;
            return this;
        }
    }
}
