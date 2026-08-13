namespace Lotofacil.Application.Common.ViewsModel
{
    /// <summary>
    /// Representa um resultado paginado de uma entidade genérica.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade que será paginada.</typeparam>
    public class PagedResultViewModel<T> where T : class
    {
        /// <summary>
        /// Obtém ou define a lista de registros retornados na página atual.
        /// </summary>
        public List<T> Datas { get; set; }

        /// <summary>
        /// Obtém ou define a página atual do resultado paginado.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Obtém ou define o número total de páginas disponíveis.
        /// </summary>
        public int TotalPages { get; set; }

        // Filtros

        /// <summary>
        /// Obtém ou define um filtro pelo nome.
        /// </summary>
        public string? NameFilter { get; set; }

        /// <summary>
        /// Obtém ou define um filtro pela data de início.
        /// </summary>
        public DateTime? StartDateFilter { get; set; }

        /// <summary>
        /// Obtém ou define um filtro pela data de término.
        /// </summary>
        public DateTime? EndDateFilter { get; set; }
    }
}

