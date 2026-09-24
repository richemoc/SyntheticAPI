using System.ComponentModel.DataAnnotations;

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
    [property: Required, StringLength(200, MinimumLength = 1)] string Name,
    [property: Required, StringLength(2000)] string Description,
    [property: Range(typeof(decimal), "0.01", "1000000")] decimal Price,
    [property: Range(0, int.MaxValue)] int Stock,
    [property: Required, StringLength(100)] string Category
);

public record UpdateProductDto(
    [property: StringLength(200, MinimumLength = 1)] string? Name,
    [property: StringLength(2000)] string? Description,
    [property: Range(typeof(decimal), "0.01", "1000000")] decimal? Price,
    [property: Range(0, int.MaxValue)] int? Stock,
    [property: StringLength(100)] string? Category
);
