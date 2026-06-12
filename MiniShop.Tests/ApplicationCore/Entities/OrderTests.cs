using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.Events;

namespace MiniShop.Tests.ApplicationCore.Entities;

public class OrderTests
{
    [Fact]
    public void NewOrder_ShouldStartAsDraft()
    {
        var order = new Order("Diego");
        
        Assert.Equal(OrderStatus.Draft, order.Status);
    }
    
    [Fact]
    public void AddItem_ShouldAddProductToOrder()
    {
        var order = new Order("Diego");
        
        order.AddItem(
            productId: 1,
            productName: "Pilsen Callao",
            unitPrice: 8.5m,
            quantity: 2
            );
        
        Assert.Single(order.Items);
        Assert.Equal(17m, order.GetTotal());
    }
    
    [Fact]
    public void AddItem_WhenProductAlreadyExists_ShouldIncreaseQuantity()
    {
        var order = new Order("Diego");
        
        order.AddItem(1, "Pilsen Callao", 8.5m, 2);
        order.AddItem(1, "Pilsen Callao", 8.5m, 3);
        
        var item = Assert.Single(order.Items);
        
        Assert.Equal(5, item.Quantity);
        Assert.Equal(42.5m, order.GetTotal());
    }
    
    [Fact]
    public void Place_WhenOrderHasItems_ShouldChangeStatusToPlaced()
    {
        var order = new Order("Diego");
        
        order.AddItem(1, "Pilsen Callao", 8.5m, 2);
        
        order.Place();
        
        Assert.Equal(OrderStatus.Placed, order.Status);
    }
    
    [Fact]
    public void Place_WhenOrderIsEmpty_ShouldThrowException()
    {
        var order = new Order("Diego"); 
        
        var exception = Assert.Throws<InvalidOperationException>(() => order.Place());
        
        Assert.Equal("Cannot place an empty order.", exception.Message);
    }
    
    [Fact]
    public void MarkAsPaid_WhenOrderIsDraft_ShouldThrowException()
    {
        var order = new Order("Diego");
        
        var exception = Assert.Throws<InvalidOperationException>(() => order.MarkAsPaid());
        
        Assert.Equal("Only placed orders can be paid.", exception.Message);
    }
    
    [Fact]
    public void Cancel_WhenOrderIsPaid_ShouldThrowException()
    {
        var order = new Order("Diego");
        order.AddItem(1, "Pilsen Callao", 8.5m, 2);
        order.Place();
        order.MarkAsPaid();
        
        var exception = Assert.Throws<InvalidOperationException>(() => order.Cancel());
        
        Assert.Equal("Paid orders cannot be cancelled.", exception.Message);
    }
    
    [Fact]
    public void Place_ShouldAddOrderPlacedEvent()
    {
        var order = new Order("Diego");
        order.AddItem(1, "Pilsen Callao", 8.5m, 2);

        order.Place();

        var domainEvent = Assert.Single(order.DomainEvents);
        var orderPlacedEvent = Assert.IsType<OrderPlacedEvent>(domainEvent);

        Assert.Equal("Diego", orderPlacedEvent.CustomerName);
        Assert.Equal(17m, orderPlacedEvent.Total);
    }

    [Fact]
    public void MarkAsPaid_ShouldAddOrderPaidEvent()
    {
        var order = new Order("Diego");
        order.AddItem(1, "Pilsen Callao", 8.5m, 2);
        order.Place();
        order.ClearDomainEvents();

        order.MarkAsPaid();

        var domainEvent = Assert.Single(order.DomainEvents);
        var orderPaidEvent = Assert.IsType<OrderPaidEvent>(domainEvent);

        Assert.Equal("Diego", orderPaidEvent.CustomerName);
        Assert.Equal(17m, orderPaidEvent.Total);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var order = new Order("Diego");
        order.AddItem(1, "Pilsen Callao", 8.5m, 2);
        order.Place();

        order.ClearDomainEvents();

        Assert.Empty(order.DomainEvents);
    }
}