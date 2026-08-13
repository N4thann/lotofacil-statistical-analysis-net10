namespace Lotofacil.Application.Features.Statistics
{
    /// <summary>
    /// ViewModel utilizada para exibir análises na tela do Dash3.
    /// </summary>
    public class Dash3ViewModel
    {
        /// <summary>
        /// Obtém ou define o número total de concursos cadastrados.
        /// </summary>
        public int TotalContests { get; set; }

        /// <summary>
        /// Obtém ou define o número total de concursos base cadastrados.
        /// </summary>
        public int TotalBaseContests { get; set; }

        /// <summary>
        /// Obtém ou define a distribuição de concursos por ano.
        /// </summary>
        public Dictionary<string, int> Years { get; set; }

        /// <summary>
        /// Obtém ou define o nome do último concurso adicionado à base.
        /// </summary>
        public string LastContest { get; set; }

        /// <summary>
        /// Obtém ou define o nome do primeiro concurso adicionado à base.
        /// </summary>
        public string FirstContest { get; set; }
    }
}

