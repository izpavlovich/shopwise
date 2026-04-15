using Microsoft.EntityFrameworkCore;
using ShopWise.Api.Data;
using ShopWise.Api.DTOs;

namespace ShopWise.Api.Services;

public class CartService
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db) => _db = db;

    public async Task<CartResponse?> GetCartAsync(int userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null) return null;

        var items = cart.Items.Select(i => new CartItemResponse(
            i.ProductId,
            i.Product!.Name,
            i.Product.Price,
            i.Quantity,
            i.Product.Price * i.Quantity
        )).ToList();

        return new CartResponse(cart.Id, items, items.Sum(i => i.Subtotal));
    }

    public async Task<CartResponse?> AddItemAsync(int userId, AddToCartRequest req)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null) return null;

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == req.ProductId);
        if (existing is not null)
            existing.Quantity += req.Quantity;
        else
            cart.Items.Add(new Models.CartItem { CartId = cart.Id, ProductId = req.ProductId, Quantity = req.Quantity });

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<bool> RemoveItemAsync(int userId, int productId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null) return false;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return false;

        cart.Items.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null) return false;
        cart.Items.Clear();
        await _db.SaveChangesAsync();
        return true;
    }
}
