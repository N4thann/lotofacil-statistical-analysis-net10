using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotofacil.Domain.Common
{
    /// <summary>
    /// Representa a contagem de ocorrências de um número específico dentro do intervalo de 1 a 25.
    /// </summary>
    public class NumberOccurencesModel
    {
        /// <summary>
        /// Obtém ou define o número analisado.
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// Obtém ou define a quantidade de vezes que o número ocorreu.
        /// </summary>
        public int Occurences { get; set; }

    }
}
