using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniShop.ApplicationCore.Entities;
using MiniShop.Infrastructure.Data;

namespace MiniShop.IntegrationTests;

public sealed class OrdersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrdersIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_AddItem_And_GetOrder_ShouldReturnOrderWithItems()
    {
        await _factory.ResetDatabaseAsync();

        var brandId = await CreateBrandAsync("Logitech");
        var productId = await CreateProductAsync("MX Master 3S", 120, brandId);
        var orderId = await CreateOrderAsync("Diego");

        var addItemResponse = await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/items",
            new
            {
                productId,
                quantity = 2
            });
        
        Assert.Equal(HttpStatusCode.OK, addItemResponse.StatusCode);
        
        var getOrderResponse = await _client.GetAsync($"/api/orders/{orderId}");
        
        Assert.Equal(HttpStatusCode.OK, getOrderResponse.StatusCode);
        
        var json = await TestJsonHelper.ReadJsonAsync(getOrderResponse);
        
        Assert.Equal(orderId, json.GetProperty("id").GetInt32());
        Assert.Equal("Diego", json.GetProperty("customerName").GetString());

        var items = json.GetProperty("items");
        
        Assert.Equal(1, items.GetArrayLength());

        var firstItem = items[0];
        Assert.Equal(productId, firstItem.GetProperty("productId").GetInt32());
        Assert.Equal(2, firstItem.GetProperty("quantity").GetInt32());
    }

    [Fact]
    public async Task PlaceOrder_ShouldChangeStatusToPlaced_AndCreateOutboxMessage()
    {
        await _factory.ResetDatabaseAsync();

        var brandId = await CreateBrandAsync("Apple");
        var productId = await CreateProductAsync("Magic Keyboard", 180, brandId);
        var orderId = await CreateOrderAsync("Lucia");

        await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/items",
            new
            {
                productId,
                quantity = 1
            });

        var placeResponse = await _client.PostAsync(
            $"/api/orders/{orderId}/place",
            content: null);

        Assert.Equal(HttpStatusCode.OK, placeResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MiniShopDbContext>();

        var order = await dbContext.Orders
            .Include(x => x.Items)
            .FirstAsync(x => x.Id == orderId);

        Assert.Equal(OrderStatus.Placed, order.Status);

        var outboxMessages = await dbContext.OutboxMessages.ToListAsync();

        Assert.Contains(
            outboxMessages,
            message => message.Type.Contains("OrderPlacedIntegrationEvent"));
    }
    
    [Fact]
    public async Task PayOrder_ShouldChangeStatusToPaid_AndCreateOrderPaidOutboxMessage()
    {
        await _factory.ResetDatabaseAsync();

        var brandId = await CreateBrandAsync("Sony");
        var productId = await CreateProductAsync("Headphones", 250, brandId);
        var orderId = await CreateOrderAsync("Diego");

        await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/items",
            new
            {
                productId,
                quantity = 1
            });

        var placeResponse = await _client.PostAsync(
            $"/api/orders/{orderId}/place",
            content: null);

        Assert.Equal(HttpStatusCode.OK, placeResponse.StatusCode);

        var payResponse = await _client.PostAsync(
            $"/api/orders/{orderId}/pay",
            content: null);

        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MiniShopDbContext>();

        var order = await dbContext.Orders
            .FirstAsync(x => x.Id == orderId);

        Assert.Equal(OrderStatus.Paid, order.Status);

        var outboxMessages = await dbContext.OutboxMessages.ToListAsync();

        Assert.Contains(
            outboxMessages,
            message => message.Type.Contains("OrderPaidIntegrationEvent"));
    }

    private async Task<int> CreateBrandAsync(string name)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/brands",
            new
            {
                name
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await TestJsonHelper.ReadIdAsync(response);
    }

    private async Task<int> CreateProductAsync(
        string name,
        decimal price,
        int brandId)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new
            {
                name,
                price,
                brandId
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await TestJsonHelper.ReadIdAsync(response);
    }

    private async Task<int> CreateOrderAsync(string customerName)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            new
            {
                customerName
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await TestJsonHelper.ReadIdAsync(response);
    }
}
