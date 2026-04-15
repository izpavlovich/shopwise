namespace ShopWise.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = "pending";
    public double TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
