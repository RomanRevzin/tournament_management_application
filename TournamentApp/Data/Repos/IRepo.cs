using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace TournamentApp.Data.Repos
{
    public interface IRepo<TObj,TKey> where TObj : class 
    {
        Task<TObj?> ReadAsync(TKey id);
        Task<TObj?> ReadAsync(TKey FirstId, TKey secondId);
        //Task<TObj?> EagerReadAsync(TKey id,string);
        Task<IList<TObj>> ReadAllAsync(); 
        Task<IList<TObj>> ReadAllAsync(Expression<Func<TObj, bool>> filter);
        Task<IList<TObj>> EagerReadAllAsync(Expression<Func<TObj, bool>> filter, string rel);
        Task<(IList<TObj>, int)> ReadAllFilterAsync(int skip, int take);
        void Add(TObj entity);
        void Remove(TObj entity);
        void Update(TObj entity);
        Task<bool> IsExist(Expression<Func<TObj, bool>> filter);   
    }

    public class Repo<TObj,TKey> : IRepo<TObj,TKey> where TObj : class
    {
        private readonly TournamentAppDbContext _context;

        public Repo(TournamentAppDbContext context)
        {
            _context = context;
        }

        public async Task<TObj?> ReadAsync(TKey id)
        {
            return await _context.Set<TObj>().FindAsync(id);
        }
        public async Task<TObj?> ReadAsync(TKey FirstId, TKey secondId)
        {
            return await _context.Set<TObj>().FindAsync(FirstId, secondId);
        }
        //public async Task<TObj?> EagerReadAsync(TKey id)
        //{
        //    return await _context.Set<TObj>().FindAsync(id);
        //}
        public async Task<IList<TObj>> ReadAllAsync()
        {
            return await _context.Set<TObj>().ToListAsync();
        }
        public async Task<IList<TObj>> ReadAllAsync(Expression<Func<TObj, bool>> filter)
        {
            return await _context.Set<TObj>().Where(filter).ToListAsync();
        }
        public async Task<IList<TObj>> EagerReadAllAsync(Expression<Func<TObj, bool>> filter, string rel)
        {
            return await _context.Set<TObj>().Include(rel).Where(filter).ToListAsync();
        }
        public async Task<(IList<TObj>, int)> ReadAllFilterAsync(int skip, int take)
        {
            var all = _context.Set<TObj>();
            var relevant = await all.Skip(skip).Take(take).ToListAsync();
            var total = all.Count();

            (List<TObj>, int) result = (relevant, total);

            return result;
        }     
        public void Add(TObj entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Set<TObj>().Add(entity);
        }
        public void Remove(TObj entity)
        {
            _context.Set<TObj>().Remove(entity);
        }
        public void Update(TObj entity)
        {
            _context.Update(entity);
        }
        public async Task<bool> IsExist(Expression<Func<TObj, bool>> filter)
        {
            return await _context.Set<TObj>().AnyAsync(filter);
        }
    }

}
