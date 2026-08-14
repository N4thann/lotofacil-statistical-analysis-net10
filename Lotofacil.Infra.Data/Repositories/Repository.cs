using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using Lotofacil.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Lotofacil.Infra.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        public Repository(ApplicationDbContext context) => _context = context;

        public async Task<T> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>()
                .AsNoTracking().ToListAsync();
        public IQueryable<T> GetAllQueryable() => _context.Set<T>().AsQueryable();
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
            await _context.Set<T>().AnyAsync(predicate);

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task SaveUpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SaveDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Entidade com ID {id} não encontrada.");
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public void SaveAdd(T entity)
        {
            // Interface exige void síncrono (usado a partir de métodos não-async como
            // BaseContestService.Create/ContestService.Create). GetAwaiter().GetResult() bloqueia até a
            // gravação terminar de verdade e propaga qualquer exceção de forma síncrona para o chamador
            // (os try/catch existentes em BaseContestService.Create dependem disso) — em vez do
            // fire-and-forget anterior, que retornava antes da gravação completar e engolia exceções.
            _context.Set<T>().AddAsync(entity).AsTask().GetAwaiter().GetResult();
            _context.SaveChangesAsync().GetAwaiter().GetResult();
        }
    }
}
