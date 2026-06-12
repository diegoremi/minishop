using Microsoft.EntityFrameworkCore;
using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.Infrastructure.Data;

public class OrderRepository: EfRepository<Order>, IOrderRepository
{
    private readonly MiniShopDbContext _dbContext;
    
    public OrderRepository(MiniShopDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}