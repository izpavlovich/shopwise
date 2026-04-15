using Microsoft.EntityFrameworkCore;
using ShopWise.Api.Data;
using ShopWise.Api.DTOs;
using ShopWise.Api.Models;

namespace ShopWise.Api.Services;

public class OrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db) => _db = db;

    public async Task<OrderResponse?> CreateFromCartAsync(int userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || !cart.Items.Any())
            return null;

        // Simulate payment gateway latency.
        // Both concurrent requests read the cart above before either commits —
        // this is the window that makes the double-submit bug reproducible.
        await Task.Delay(2000);

        double total = cart.Items.Sum(i => i.Product!.Price * i.Quantity);

        var order = new Order
        {
            UserId = userId,
            TotalAmount = total,
            Status = "pending"
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        foreach (var item in cart.Items)
        {
            _db.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product!.Price
            });

            item.Product.Stock -= item.Quantity;
        }

        cart.Items.Clear();
        await _db.SaveChangesAsync();

        return await GetByIdAsync(userId, order.Id);
    }

    public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapOrder).ToList();
    }

    public async Task<OrderResponse?> GetByIdAsync(int userId, int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order is null ? null : MapOrder(order);
    }

    private static OrderResponse MapOrder(Order o) => new(
        o.Id,
        o.Status,
        o.TotalAmount,
        o.CreatedAt,
        o.Items.Select(i => new OrderItemResponse(
            i.ProductId,
            i.Product?.Name ?? "Unknown",
            i.Quantity,
            i.UnitPrice,
            i.UnitPrice * i.Quantity
        )).ToList()
    );
}
