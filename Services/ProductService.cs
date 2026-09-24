using Microsoft.EntityFrameworkCore;
using SyntheticApi.Data;
using SyntheticApi.DTOs;
using SyntheticApi.Models;

namespace SyntheticApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(string? category = null, int page = 1, int pageSize = 50);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);
}

public class ProductService(AppDbContext db, ILogger<ProductService> logger) : IProductService
{
    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? category = null, int page = 1, int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Products.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        return await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category.Trim()
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();
        logger.LogInformation("Product {ProductId} created", product.Id);
        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return null;

        if (dto.Name is not null) product.Name = dto.Name.Trim();
        if (dto.Description is not null) product.Description = dto.Description.Trim();
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.Category is not null) product.Category = dto.Category.Trim();

        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("Product {ProductId} updated", id);
        return ToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return false;
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogWarning("Product {ProductId} soft deleted", id);
        return true;
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return false;

        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("Product {ProductId} restored", id);
        return true;
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.IsActive, p.CreatedAt);
}
