using System.Globalization;
using Lotofacil.Domain.Entities;

namespace Lotofacil.Infra.Data.Initialization.Seed
{
    /// <summary>
    /// Leitor do CSV de histórico de concursos da Lotofácil, no formato
    /// <c>Concurso,Data,n1,n2,...,n15</c> com header na primeira linha.
    /// </summary>
    public static class ContestCsvReader
    {
        /// <summary>
        /// Lê o arquivo CSV informado e retorna os <see cref="Contest"/> correspondentes a cada linha
        /// de dados (a primeira linha, de header, é sempre ignorada).
        /// </summary>
        /// <param name="filePath">Caminho absoluto do arquivo CSV.</param>
        /// <returns>Lista de <see cref="Contest"/> na mesma ordem em que aparecem no arquivo.</returns>
        public static List<Contest> Read(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var result = new List<Contest>(Math.Max(0, lines.Length - 1));

            for (int i = 1; i < lines.Length; i++) // linha 0 é o header
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                var parts = lines[i].Split(',');
                var contestNumber = int.Parse(parts[0]);
                var date = DateTime.ParseExact(parts[1], "dd/MM/yyyy", CultureInfo.InvariantCulture).AddHours(20);
                var numbers = parts.Skip(2).Select(int.Parse);
                var formattedNumbers = string.Join("-", numbers.Select(n => n.ToString("D2")));

                result.Add(new Contest($"Concurso {contestNumber}", date, formattedNumbers));
            }

            return result;
        }
    }
}
