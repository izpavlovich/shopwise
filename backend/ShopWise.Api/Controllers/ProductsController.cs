using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopWise.Api.DTOs;
using ShopWise.Api.Services;

namespace ShopWise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _products;

    public ProductsController(ProductService products) => _products = products;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var results = await _products.GetAllAsync(search);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _products.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(CreateProductRequest req)
    {
        var result = await _products.CreateAsync(req);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _products.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
