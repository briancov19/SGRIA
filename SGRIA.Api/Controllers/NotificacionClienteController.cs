using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/notificaciones-cliente")]
public class NotificacionesClienteController : ControllerBase
{
    private readonly NotificacionClienteService _service;

    public NotificacionesClienteController(NotificacionClienteService service) 
        => _service = service;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("activas")]
    public async Task<IActionResult> GetActivas([FromQuery] int minutosCorte = 15, CancellationToken ct = default)
    {
        return Ok(await _service.GetActivasAsync(minutosCorte, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NotificacionClienteCreateDto dto, CancellationToken ct)
    {
        try
        {
            var created = await _service.AddAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:int}/atender")]
    public async Task<IActionResult> MarcarComoAtendida(int id, CancellationToken ct)
    {
        var updated = await _service.MarcarComoAtendidaAsync(id, ct);
        
        if (updated is null)
            return NotFound(new { error = "Notificación no encontrada o ya atendida" });

        return Ok(updated);
    }
}