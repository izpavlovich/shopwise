using Microsoft.EntityFrameworkCore;
using ShopWise.Api.Data;
using ShopWise.Api.DTOs;
using ShopWise.Api.Models;

namespace ShopWise.Api.Services;

public class ProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public async Task<List<ProductResponse>> GetAllAsync(string? search = null)
    {
        var query = _db.Products
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        var products = await query.ToListAsync();

        return products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Stock,
            p.ImageUrl,
            p.CategoryId,
            p.Category?.Name ?? "Unknown"
        )).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var p = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (p is null) return null;

        return new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.Stock, p.ImageUrl, p.CategoryId, p.Category?.Name ?? "Unknown");
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest req)
    {
        var product = new Product
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            Stock = req.Stock,
            ImageUrl = req.ImageUrl,
            CategoryId = req.CategoryId
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var category = await _db.Categories.FindAsync(req.CategoryId);
        return new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.Stock, product.ImageUrl, product.CategoryId, category?.Name ?? "Unknown");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return false;
        product.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
