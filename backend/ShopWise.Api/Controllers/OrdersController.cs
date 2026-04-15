using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopWise.Api.Data;
using ShopWise.Api.Services;

namespace ShopWise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orders;
    private readonly AppDbContext _db;

    public OrdersController(OrderService orders, AppDbContext db)
    {
        _orders = orders;
        _db = db;
    }

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _orders.GetUserOrdersAsync(UserId);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orders.GetByIdAsync(UserId, id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder()
    {
        var order = await _orders.CreateFromCartAsync(UserId);
        return order is null ? BadRequest(new { message = "Cart is empty." }) : Ok(order);
    }

    [HttpGet("search")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> SearchOrders([FromQuery] string term)
    {
        var orders = await _db.Orders
            .FromSqlRaw($"SELECT * FROM orders WHERE status LIKE '%{term}%'")
            .ToListAsync();

        return Ok(orders);
    }
}
