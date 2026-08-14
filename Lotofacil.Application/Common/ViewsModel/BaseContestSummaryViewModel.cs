namespace Lotofacil.Application.Common.ViewsModel
{
    /// <summary>
    /// Resumo somente-leitura de um <see cref="Domain.Entities.BaseContest"/> para a listagem paginada
    /// (Dashboard 2) e para a exportação em Excel — evita carregar a coleção completa de
    /// <c>ContestsAbove11</c> quando só a contagem é necessária.
    /// </summary>
    public class BaseContestSummaryViewModel
    {
        /// <summary>Nome do concurso base.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Data de realização do concurso base.</summary>
        public DateTime Data { get; set; }

        /// <summary>Números sorteados no concurso base.</summary>
        public string Numbers { get; set; } = string.Empty;

        /// <summary>Quantidade de concursos em que foram acertados exatamente 11 números.</summary>
        public int Hit11 { get; set; }

        /// <summary>Quantidade de concursos em que foram acertados exatamente 12 números.</summary>
        public int Hit12 { get; set; }

        /// <summary>Quantidade de concursos em que foram acertados exatamente 13 números.</summary>
        public int Hit13 { get; set; }

        /// <summary>Quantidade de concursos em que foram acertados exatamente 14 números.</summary>
        public int Hit14 { get; set; }

        /// <summary>Quantidade de concursos em que foram acertados exatamente 15 números.</summary>
        public int Hit15 { get; set; }

        /// <summary>Os dez números mais frequentes nos concursos relacionados.</summary>
        public string? TopTenNumbers { get; set; }

        /// <summary>Total de concursos vinculados com 11 ou mais acertos (contagem, não a coleção completa).</summary>
        public int ContestsAbove11Count { get; set; }
    }
}
