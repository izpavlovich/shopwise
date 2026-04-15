using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopWise.Api.DTOs;
using ShopWise.Api.Services;

namespace ShopWise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartService _cart;

    public CartController(CartService cart) => _cart = cart;

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cart.GetCartAsync(UserId);
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddToCartRequest req)
    {
        var cart = await _cart.AddItemAsync(UserId, req);
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var removed = await _cart.RemoveItemAsync(UserId, productId);
        return removed ? NoContent() : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cart.ClearCartAsync(UserId);
        return NoContent();
    }
}
