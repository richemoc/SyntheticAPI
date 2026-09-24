namespace SyntheticApi.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category
);

public record UpdateProductDto(
    string? Name,
    string? Description,
    decimal? Price,
    int? Stock,
    string? Category,
    bool? IsActive
);
