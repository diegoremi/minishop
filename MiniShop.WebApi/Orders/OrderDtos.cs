using MiniShop.ApplicationCore.Entities;

namespace MiniShop.WebApi.Orders;

public record CreateOrderRequest(
        string CustomerName
    );
    
public record AddOrderItemRequest(
        int ProductId,
        int Quantity
    );

public record OrderItemDto(
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal Subtotal
    );

public record OrderDto(
        int Id,
        string CustomerName,
        OrderStatus Status,
        decimal Total,
        List<OrderItemDto> Items
    );
