using Lotofacil.Domain.Enums;

namespace Lotofacil.Application.Common.ViewsModel
{
    /// <summary>
    /// ViewModel responsável por classificar e exibir informações sobre erros na aplicação.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="ErrorViewModel"/>.
        /// </summary>
        /// <param name="message">Obtém ou define a mensagem de erro exibida na view.</param>
        /// <param name="excepetiondetails">Obtém ou define os detalhes técnicos da exceção.</param>
        /// <param name="errorTypeCode">Código do tipo de erro para mapeamento.</param>
        public ErrorViewModel(string? message, string? excepetiondetails, int errorTypeCode)
        {
            Message = message;
            ExceptionDetails = excepetiondetails;
            ErrorType = MapErrorType(errorTypeCode);
        }

        /// <summary>
        /// Obtém a mensagem de erro exibida na view.
        /// </summary>
        public string? Message { get; private set; }

        /// <summary>
        /// Obtém os detalhes técnicos da exceção.
        /// </summary>
        public string? ExceptionDetails { get; private set; }

        /// <summary>
        /// Obtém o tipo do erro classificado de acordo com o código fornecido.
        /// </summary>
        public ErrorType ErrorType { get; private set; }

        /// <summary>
        /// Mapeia o código do erro para um tipo específico de erro.
        /// </summary>
        /// <param name="errorTypeCode">Código do tipo de erro.</param>
        /// <returns>O tipo de erro correspondente ao código informado.</returns>
        private ErrorType MapErrorType(int errorTypeCode)
        {
            return errorTypeCode switch
            {
                1 => ErrorType.Critical,
                2 => ErrorType.NoRecords,
                3 => ErrorType.NotFound,
                _ => ErrorType.Unspecified,
            };
        }
    }
}

