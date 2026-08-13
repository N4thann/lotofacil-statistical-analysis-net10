using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotofacil.Application.Features.Contests.DTO
{
    /// <summary>
    /// DTO de response para a tela List de Concursos.
    /// Fornece diversas análises estatísticas após a seleção de múltiplos concursos.
    /// </summary>
    /// <param name="ContestsName">Lista contendo os números dos concursos selecionados.</param>
    /// <param name="EvenNumbersAveragePercentage">Porcentagem média de números pares nos concursos selecionados.</param>
    /// <param name="OddNumbersAveragePercentage">Porcentagem média de números ímpares nos concursos selecionados.</param>
    /// <param name="Top5MostFrequentNumbers">Os 5 números mais frequentes nos concursos selecionados.</param>
    /// <param name="Top5LeastFrequentNumbers">Os 5 números menos frequentes nos concursos selecionados.</param>
    /// <param name="MultiplesOfThreeAveragePercentage">Porcentagem média de números múltiplos de 3 nos concursos selecionados.</param>
    public record ContestModalResponseDTO(
        List<string> ContestsName,
        double EvenNumbersAveragePercentage,
        double OddNumbersAveragePercentage,
        List<int> Top5MostFrequentNumbers,
        List<int> Top5LeastFrequentNumbers,
        double MultiplesOfThreeAveragePercentage);
}
