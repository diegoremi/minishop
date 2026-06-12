namespace MiniShop.ApplicationCore.Interfaces;

public interface IRepository<T> where T : class
{
    Task<List<T>> ListAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);

}