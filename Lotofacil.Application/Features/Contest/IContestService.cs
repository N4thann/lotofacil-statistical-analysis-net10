using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Application.Features.Contests.DTO;
using Lotofacil.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotofacil.Application.Features.Contests
{
    public interface IContestService
    {
        void Create(ContestViewModel contestVM);
        Task<IEnumerable<Contest>> GetContestsOrderedAsync(string sortOrder);
        Task<ContestModalResponseDTO> AnalisarConcursos(ContestModalRequestDTO request);
    }
}
