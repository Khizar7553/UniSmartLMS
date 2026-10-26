using System.Linq.Expressions;

namespace UniSmartLMS.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
       Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
        Task<IEnumerable<T>> SearchAsync(string columnName, string searchTerm);

        // Direct Stored Procedure execution
        Task<IEnumerable<T>> ExecuteProcAsync(string procName, object parameters = null);
        Task<T> ExecuteProcSingleAsync(string procName, object parameters = null);
        Task<int> ExecuteProcNonQueryAsync(string procName, object parameters = null);
    }
}