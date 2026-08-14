using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.Statistics;
using Lotofacil.Domain.Entities;
using System.Text;

namespace Lotofacil.Application.Common
{
    /// <summary>
    /// Service responsible for contest management operations.
    /// </summary>
    public interface IContestManagementService
    {
        /// <summary>
        /// Sets the hour of the provided DateTime to 20:00 (8 PM).
        /// </summary>
        /// <param name="data">The DateTime to be adjusted.</param>
        /// <returns>A DateTime instance with the time set to 20:00.</returns>
        DateTime SetDataHour(DateTime data);

        /// <summary>
        /// Converts a formatted string of numbers separated by hyphens into a list of integers.
        /// </summary>
        /// <param name="input">The formatted string (e.g., "01-02-03").</param>
        /// <returns>A list of integers extracted from the string.</returns>
        /// <exception cref="FormatException">Thrown when the input string contains non-numeric values.</exception>
        List<int> ConvertFormattedStringToList(string input);

        /// <summary>
        /// Formats a string of digits into a hyphen-separated string with pairs of digits.
        /// </summary>
        /// <param name="input">The input string of digits (e.g., "010203").</param>
        /// <returns>A formatted string (e.g., "01-02-03").</returns>
        /// <exception cref="ArgumentException">Thrown when the input string length is odd or empty.</exception>
        string FormatNumbersToSave(string input);

        /// <summary>
        /// Calculates the number of matching elements between two integer lists.
        /// </summary>
        /// <param name="list1">The first list of integers.</param>
        /// <param name="list2">The second list of integers.</param>
        /// <returns>The count of matching integers.</returns>
        int CalculateIntersection(List<int> list1, List<int> list2);

        /// <summary>
        /// Generates an Excel file containing contest activity logs.
        /// </summary>
        /// <param name="data">The collection of contest activity logs to include in the file.</param>
        /// <returns>A memory stream containing the generated Excel file.</returns>
        MemoryStream GenerateExcelForContestActivityLog(IEnumerable<ContestActivityLog> data);

        /// <summary>
        /// Generates an Excel file containing base contest information.
        /// </summary>
        /// <param name="data">The collection of base contest summaries to include in the file.</param>
        /// <returns>A memory stream containing the generated Excel file.</returns>
        MemoryStream GenerateExcelForBaseContest(IEnumerable<BaseContestSummaryViewModel> data);
        /// <summary>
        /// Retorna os dois concursos base com melhor eficiência nas comparações.
        /// A eficiência é calculada com base nos acertos de cada concurso.
        /// </summary>
        /// <param name="baseContests">Lista de concursos base contendo suas listas relacionadas.</param>
        /// <returns>Uma lista contendo os dois concursos base mais eficientes.</returns>
        List<TopContestViewModel> TopTwoContests(IEnumerable<BaseContest> baseContests);
        /// <summary>
        /// Realiza análises sobre os concursos base e retorna um ViewModel com as informações.
        /// Inclui o primeiro e último concurso, total de concursos e agrupamento por ano.
        /// </summary>
        /// <param name="baseContests">Lista de concursos base contendo suas listas relacionadas.</param>
        /// <returns>Um objeto <see cref="Dash3ViewModel"/> preenchido com os dados analisados.</returns>
        Task<Dash3ViewModel> Dash3Analysis(IEnumerable<BaseContest> baseContests);
        /// <summary>
        /// Retorna um objeto paginado contendo os resumos de concursos base filtrados por nome e período.
        /// </summary>
        /// <param name="baseContests">Lista de resumos de concursos base filtrados.</param>
        /// <param name="totalCount">Número total de registros que atendem aos critérios de filtragem.</param>
        /// <param name="name">Nome do concurso para filtro (opcional).</param>
        /// <param name="startDate">Data inicial para filtro (opcional).</param>
        /// <param name="endDate">Data final para filtro (opcional).</param>
        /// <param name="page">Número da página atual.</param>
        /// <param name="pageSize">Quantidade de registros por página.</param>
        /// <returns>Um objeto <see cref="PagedResultViewModel{BaseContestSummaryViewModel}"/> contendo os dados paginados.</returns>
        PagedResultViewModel<BaseContestSummaryViewModel> PagedResultDash2(List<BaseContestSummaryViewModel> baseContests, int totalCount, string? name, DateTime? startDate, DateTime? endDate, int page, int pageSize);
    }   
}
/*
 Não adicione modificadores como static, private, ou protected em métodos de interface, 
exceto em casos especiais  para métodos estáticos com implementação (C# 8.0 ou superior).*/
