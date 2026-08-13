namespace Lotofacil.Application.Common.ViewsModel
{
    /// <summary>
    /// ViewModel utilizada para representar um concurso (Contest ou BaseContest).
    /// </summary>
    public class ContestViewModel
    {
        /// <summary>
        /// Obtém ou define o identificador único do concurso.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtém ou define o nome do concurso.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Obtém ou define a data do concurso.
        /// </summary>
        public DateTime Data { get; set; }

        /// <summary>
        /// Obtém ou define a sequência de números sorteados no concurso.
        /// </summary>
        public string Numbers { get; set; }

        /// <summary>
        /// Obtém ou define um valor que indica se o concurso pertence à tabela BaseContest.
        /// </summary>
        public bool IsBaseContest { get; set; }
    }
}

/* O Id deve ser incluído quando necessário para operações como edição, exclusão ou identificação única no banco.
   Se não for necessário na interface do usuário, pode ser omitido para evitar exposição desnecessária de dados. */
