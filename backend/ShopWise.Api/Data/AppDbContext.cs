using Microsoft.EntityFrameworkCore;
using ShopWise.Api.Models;

namespace ShopWise.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map to the lowercase table names created by db/init.sql
        modelBuilder.Entity<Category>().ToTable("categories");
        modelBuilder.Entity<Product>().ToTable("products");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Cart>().ToTable("carts");
        modelBuilder.Entity<CartItem>().ToTable("cart_items");
        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<OrderItem>().ToTable("order_items");

        // Column name mappings (snake_case)
        modelBuilder.Entity<Category>(e => {
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.Name).HasColumnName("name");
            e.Property(c => c.Description).HasColumnName("description");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
        });
        modelBuilder.Entity<Product>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.CategoryId).HasColumnName("category_id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.Description).HasColumnName("description");
            e.Property(p => p.Price).HasColumnName("price");
            e.Property(p => p.Stock).HasColumnName("stock");
            e.Property(p => p.ImageUrl).HasColumnName("image_url");
            e.Property(p => p.IsDeleted).HasColumnName("is_deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
        });
        modelBuilder.Entity<User>(e => {
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Email).HasColumnName("email");
            e.Property(u => u.PasswordHash).HasColumnName("password_hash");
            e.Property(u => u.FullName).HasColumnName("full_name");
            e.Property(u => u.Role).HasColumnName("role");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
        });
        modelBuilder.Entity<Cart>(e => {
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.UserId).HasColumnName("user_id");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        });
        modelBuilder.Entity<CartItem>(e => {
            e.Property(ci => ci.Id).HasColumnName("id");
            e.Property(ci => ci.CartId).HasColumnName("cart_id");
            e.Property(ci => ci.ProductId).HasColumnName("product_id");
            e.Property(ci => ci.Quantity).HasColumnName("quantity");
        });
        modelBuilder.Entity<Order>(e => {
            e.Property(o => o.Id).HasColumnName("id");
            e.Property(o => o.UserId).HasColumnName("user_id");
            e.Property(o => o.Status).HasColumnName("status");
            e.Property(o => o.TotalAmount).HasColumnName("total_amount");
            e.Property(o => o.CreatedAt).HasColumnName("created_at");
            e.Property(o => o.UpdatedAt).HasColumnName("updated_at");
        });
        modelBuilder.Entity<OrderItem>(e => {
            e.Property(oi => oi.Id).HasColumnName("id");
            e.Property(oi => oi.OrderId).HasColumnName("order_id");
            e.Property(oi => oi.ProductId).HasColumnName("product_id");
            e.Property(oi => oi.Quantity).HasColumnName("quantity");
            e.Property(oi => oi.UnitPrice).HasColumnName("unit_price");
        });

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);
    }
}
