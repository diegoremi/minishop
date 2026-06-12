using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly IRepository<Product> _products;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public OrderService(
        IOrderRepository orderRepository,
        IRepository<Product> productRepository,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _orders = orderRepository;
        _products = productRepository;
        _domainEventDispatcher = domainEventDispatcher;
    }
    
    public async Task<Order> CreateOrderAsync(string customerName)
    {
        var order = new Order(customerName);
        
        await _orders.AddAsync(order);
        
        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _orders.GetByIdWithItemsAsync(orderId);
    }

    public async Task<Order> AddItemAsync(int orderId, int productId, int quantity)
    {
        var order = await _orders.GetByIdWithItemsAsync(orderId);
        
        if(order == null)
            throw new InvalidOperationException($"Order with id {orderId} does not exist.");
        
        var product = await _products.GetByIdAsync(productId);
        
        if(product == null)
            throw new InvalidOperationException($"Product with id {productId} does not exist.");
        
        order.AddItem(
            product.Id,
            product.Name,
            product.Price,
            quantity);
        
        await _orders.UpdateAsync(order);

        return order;
    }

    public async Task<Order> PlaceOrderAsync(int orderId)
    {
        var order = await _orders.GetByIdWithItemsAsync(orderId);
        
        if(order == null)
            throw new InvalidOperationException($"Order with id {orderId} does not exist.");
        
        order.Place();
        
        await _orders.UpdateAsync(order);
        
        await _domainEventDispatcher.DispatchAndClearEventsAsync(order.DomainEvents);
        order.ClearDomainEvents();
        
        return order;
    }

    public async Task<Order> PayOrderAsync(int orderId)
    {
        var order = await _orders.GetByIdWithItemsAsync(orderId);

        if (order == null)
            throw new InvalidOperationException($"Order with id {orderId} does not exist.");
        
        order.MarkAsPaid();
        
        await _orders.UpdateAsync(order);
        
        await _domainEventDispatcher.DispatchAndClearEventsAsync(order.DomainEvents);
        order.ClearDomainEvents();
        
        return order;
    }
}