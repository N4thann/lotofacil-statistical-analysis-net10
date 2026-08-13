using Lotofacil.Domain.Common;

namespace Lotofacil.Application.Features.Statistics
{
    /// <summary>
    /// Representa os dois concursos base mais eficientes com base em um cálculo ponderado de acertos acima de 11 pontos.
    /// </summary>
    public class TopContestViewModel
    {
        /// <summary>
        /// Obtém ou define o nome do concurso base.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Obtém ou define a data de realização do sorteio.
        /// </summary>
        public DateTime Data { get; set; }

        /// <summary>
        /// Obtém ou define os números sorteados no concurso base.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Obtém ou define a quantidade de concursos associados a esse concurso base.
        /// </summary>
        public int CountContests { get; set; }

        /// <summary>
        /// Obtém ou define os 10 números mais frequentes nos concursos associados.
        /// </summary>
        public string TopTenNumbers { get; set; }

        /// <summary>
        /// Obtém ou define a frequência de cada número (1 a 25) nos concursos associados.
        /// </summary>
        public List<NumberOccurencesModel> NumberOccurences { get; set; }
    }
}

