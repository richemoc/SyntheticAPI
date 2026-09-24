using Microsoft.AspNetCore.Mvc;
using SyntheticApi.DTOs;
using SyntheticApi.Models;
using SyntheticApi.Services;

namespace SyntheticApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController(IOrderService service) : ControllerBase
{
    /// <summary>Get all orders.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

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
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus status)
    {
        var updated = await service.UpdateStatusAsync(id, status);
        return updated is null ? NotFound() : Ok(updated);
    }
}
