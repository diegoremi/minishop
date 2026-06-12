using MiniShop.ApplicationCore.Entities;

namespace MiniShop.ApplicationCore.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(string customerName);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<Order> AddItemAsync(int orderId, int productId, int quantity);
    Task<Order> PlaceOrderAsync(int orderId);
    Task<Order> PayOrderAsync(int orderId);
}