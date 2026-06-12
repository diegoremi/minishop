using Microsoft.EntityFrameworkCore;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.Infrastructure.Data;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly MiniShopDbContext _dbContext;
    
    public EfRepository(MiniShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<T>> ListAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
       await _dbContext.Set<T>().AddAsync(entity);
       await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbContext.Set<T>().Update(entity);
        await _dbContext.SaveChangesAsync();
    }
}