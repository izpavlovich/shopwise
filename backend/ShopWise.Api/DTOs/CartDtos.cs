namespace ShopWise.Api.DTOs;

public record CartResponse(int CartId, List<CartItemResponse> Items, double Total);
public record CartItemResponse(int ProductId, string ProductName, double UnitPrice, int Quantity, double Subtotal);
public record AddToCartRequest(int ProductId, int Quantity);
public record UpdateCartItemRequest(int Quantity);
