using MiniShop.ApplicationCore.Events;

namespace MiniShop.ApplicationCore.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    public IReadOnlyCollection<OrderItem> Items => _items;
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Order()
    {
        
    }

    public Order(string customerName)
    {
        if(string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));

        CustomerName = customerName;
    }
    
    public void AddItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified.");

        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            return;
        }

        var item = new OrderItem(productId, productName, unitPrice, quantity);
        _items.Add(item);
    }
    
    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be placed.");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot place an empty order.");

        Status = OrderStatus.Placed;
        
        _domainEvents.Add(new OrderPlacedEvent(
            Id,
            CustomerName,
            GetTotal(),
            DateTime.UtcNow
        ));
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Placed)
            throw new InvalidOperationException("Only placed orders can be paid.");

        Status = OrderStatus.Paid;
        
        _domainEvents.Add(new OrderPaidEvent(
            Id,
            CustomerName,
            GetTotal(),
            DateTime.UtcNow
        ));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Paid orders cannot be cancelled.");

        Status = OrderStatus.Cancelled;
    }
    
    public decimal GetTotal()
    {
        return _items.Sum(x => x.GetSubtotal());
    }
}