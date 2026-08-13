using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotofacil.Application.Features.Contests.DTO
{
    /// <summary>
    /// DTO de request utilizada na tela List de Concursos.
    /// Representa os concursos selecionados pelo usuário através de um checkbox.
    /// </summary>
    /// <param name="Contests">Lista contendo os números dos concursos selecionados.</param>
    public record ContestModalRequestDTO(
        List<int> Contests      
        );
}
