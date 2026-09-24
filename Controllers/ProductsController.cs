using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyntheticApi.DTOs;
using SyntheticApi.Services;

namespace SyntheticApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProductsController(IProductService service) : ControllerBase
{
    /// <summary>Get all active products, optionally filtered by category.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        Ok(await service.GetAllAsync(category, page, pageSize));

    /// <summary>Get a single product by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await service.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Create a new product.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Partially update a product.</summary>
    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Restore a soft-deleted product.</summary>
    [HttpPost("{id:int}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(int id)
    {
        var restored = await service.RestoreAsync(id);
        return restored ? NoContent() : NotFound();
    }

    /// <summary>Soft-delete a product.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
