using MiniShop.ApplicationCore.Entities;

namespace MiniShop.ApplicationCore.Interfaces;

public interface IOrderRepository: IRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(int id);
}