namespace ShopWise.Api.DTOs;

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    double Price,
    int Stock,
    string? ImageUrl,
    int CategoryId,
    string CategoryName
);

public record CreateProductRequest(
    string Name,
    string? Description,
    double Price,
    int Stock,
    string? ImageUrl,
    int CategoryId
);

public record UpdateProductRequest(
    string? Name,
    string? Description,
    double? Price,
    int? Stock,
    string? ImageUrl
);
