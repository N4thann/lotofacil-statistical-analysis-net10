using Bogus;
using Lotofacil.Domain.Entities;

namespace Lotofacil.Tests.DataBuilder
{
    public class BaseContestDataBuilder
    {
        private readonly BaseContest _instance;

        public BaseContestDataBuilder()
        {
            var faker = new Faker<BaseContest>("pt_BR")
                .CustomInstantiator(f => new BaseContest(
                    $"Concurso {f.Random.Number(1, 9999)}",
                    f.Date.Past(2),
                    RandomNumbersString(f)));

            _instance = faker.Generate();
        }

        public static BaseContestDataBuilder Create() => new();
        public BaseContest Build() => _instance;
        public static implicit operator BaseContest(BaseContestDataBuilder builder) => builder.Build();

        public static List<BaseContest> AsList(int count)
        {
            var list = new List<BaseContest>();
            for (int i = 0; i < count; i++) list.Add(Create().Build());
            return list;
        }

        /// <summary>
        /// Gera uma string de 15 números únicos entre 1 e 25, formatada como "01-02-...-15".
        /// Compartilhado pelos três Data Builders para manter os números sempre válidos.
        /// </summary>
        public static string RandomNumbersString(Faker f)
        {
            var pool = Enumerable.Range(1, 25).ToList();
            var chosen = new List<int>();
            for (int i = 0; i < 15; i++)
            {
                var index = f.Random.Number(0, pool.Count - 1);
                chosen.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return string.Join("-", chosen.OrderBy(n => n).Select(n => n.ToString("D2")));
        }

        public BaseContestDataBuilder WithId(int id)
        {
            typeof(BaseContest).GetProperty(nameof(BaseContest.Id))?.SetValue(_instance, id);
            return this;
        }

        public BaseContestDataBuilder WithName(string name)
        {
            _instance.Name = name;
            return this;
        }

        public BaseContestDataBuilder WithData(DateTime data)
        {
            _instance.Data = data;
            return this;
        }

        public BaseContestDataBuilder WithNumbers(string numbers)
        {
            _instance.Numbers = numbers;
            return this;
        }

        public BaseContestDataBuilder WithCreatedAt(DateTime createdAt)
        {
            typeof(BaseContest).GetProperty(nameof(BaseContest.CreatedAt))?.SetValue(_instance, createdAt);
            return this;
        }

        /// <summary>
        /// Chama AddHit11()..AddHit15() a quantidade de vezes pedida em cada parâmetro,
        /// deixando os contadores da entidade no valor esperado pelo teste.
        /// </summary>
        public BaseContestDataBuilder WithHits(int hit11 = 0, int hit12 = 0, int hit13 = 0, int hit14 = 0, int hit15 = 0)
        {
            for (int i = 0; i < hit11; i++) _instance.AddHit11();
            for (int i = 0; i < hit12; i++) _instance.AddHit12();
            for (int i = 0; i < hit13; i++) _instance.AddHit13();
            for (int i = 0; i < hit14; i++) _instance.AddHit14();
            for (int i = 0; i < hit15; i++) _instance.AddHit15();
            return this;
        }

        public BaseContestDataBuilder WithTopTenNumbers(string topTen)
        {
            _instance.AddTopTenNumbers(topTen);
            return this;
        }

        public BaseContestDataBuilder WithTotalProcessed(int? totalProcessed)
        {
            _instance.TotalProcessed = totalProcessed;
            return this;
        }

        public BaseContestDataBuilder WithContestsAbove11(List<Contest> contests)
        {
            _instance.ContestsAbove11 = contests;
            return this;
        }
    }
}
