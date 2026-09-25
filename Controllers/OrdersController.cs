using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyntheticApi.DTOs;
using SyntheticApi.Models;
using SyntheticApi.Services;

namespace SyntheticApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class OrdersController(IOrderService service) : ControllerBase
{
    /// <summary>Get all orders.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        Ok(await service.GetAllAsync(page, pageSize));

    /// <summary>Get a single order by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await service.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Place a new order.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var (order, error) = await service.CreateAsync(dto);
        if (error is not null) return BadRequest(new { error });
        return CreatedAtAction(nameof(GetById), new { id = order!.Id }, order);
    }

    /// <summary>Update order status.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus status)
    {
        var updated = await service.UpdateStatusAsync(id, status);
        return updated is null ? NotFound() : Ok(updated);
    }
}
