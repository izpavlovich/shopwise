using Microsoft.EntityFrameworkCore;
using ShopWise.Api.Data;
using ShopWise.Api.DTOs;
using ShopWise.Api.Models;

namespace ShopWise.Api.Services;

public class ProductService
{
    private readonly AppDbContext _db;
    private readonly ImageGenerationService _images;
    private readonly IWebHostEnvironment _env;

    public ProductService(AppDbContext db, ImageGenerationService images, IWebHostEnvironment env)
    {
        _db = db;
        _images = images;
        _env = env;
    }

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

    public async Task<ProductResponse?> GenerateImageAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (product is null) return null;

        var prompt =
            $"Studio product photograph of {product.Name}. {product.Description}. " +
            "Centered on a clean white background, soft shadows, e-commerce catalog style, no text, no watermark.";

        var image = await _images.GenerateAsync(prompt, ct);

        var ext = image.MimeType.Contains("jpeg") ? "jpg" : "png";
        var fileName = $"{product.Id}.{ext}";
        var dir = Path.Combine(_env.ContentRootPath, "wwwroot", "images");
        Directory.CreateDirectory(dir);
        await File.WriteAllBytesAsync(Path.Combine(dir, fileName), image.Bytes, ct);

        product.ImageUrl = $"/images/{fileName}";
        await _db.SaveChangesAsync(ct);

        return new ProductResponse(product.Id, product.Name, product.Description, product.Price,
            product.Stock, product.ImageUrl, product.CategoryId, product.Category?.Name ?? "Unknown");
    }
}
