using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Lotofacil.Infra.Data.Repositories
{
    public class BaseContestRepository : IBaseContestRepository
    {
        private readonly ApplicationDbContext _context;

        public BaseContestRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<BaseContest>> GetAllWithContestsAbove11Async()
        {
            return await _context.BaseContests
                .Include(bc => bc.ContestsAbove11)
                .OrderBy(bc => bc.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();
        }
        public async Task UpdateBaseContestAsync(BaseContest baseContest)
        {
            _context.BaseContests.Update(baseContest);
            await _context.SaveChangesAsync();
        }

        public async Task<BaseContest> GetByIdAsync(int id)
        {
            return await _context.BaseContests
                .Include(bc => bc.ContestsAbove11)
                .FirstOrDefaultAsync(bc => bc.Id == id);
        }
    }
}

