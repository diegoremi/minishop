using Microsoft.AspNetCore.Mvc;
using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.WebApi.Orders;

[ApiController]
[Route ("api/[controller]")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        
        if(order is null)
            return NotFound();

        return Ok(ToDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request)
    {
        var order = await _orderService.CreateOrderAsync(request.CustomerName);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            ToDto(order));
    }

    [HttpPost("{id:int}/items")]
    public async Task<ActionResult<OrderDto>> AddItem(int id, AddOrderItemRequest request)
    {
        try
        {
            var order = await _orderService.AddItemAsync(
                id,
                request.ProductId,
                request.Quantity
            );

            return Ok(ToDto(order));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/place")]
    public async Task<ActionResult<OrderDto>> Place(int id)
    {
        try
        {
            var order = await _orderService.PlaceOrderAsync(id);
            return Ok(ToDto(order));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/pay")]
    public async Task<ActionResult<OrderDto>> Pay(int id)
    {
        try
        {
            var order = await _orderService.PayOrderAsync(id);
            return Ok(ToDto(order));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    private static OrderDto ToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.CustomerName,
            order.Status,
            order.GetTotal(),
            order.Items
                .Select(i => new OrderItemDto(
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice,
                    i.Quantity,
                    i.GetSubtotal()
                ))
                .ToList()
        );
    }
}