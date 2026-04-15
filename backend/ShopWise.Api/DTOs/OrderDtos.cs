namespace ShopWise.Api.DTOs;

public record OrderResponse(
    int Id,
    string Status,
    double TotalAmount,
    DateTime CreatedAt,
    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    int ProductId,
    string ProductName,
    int Quantity,
    double UnitPrice,
    double Subtotal
);

public record CreateOrderRequest(string? ShippingAddress);
