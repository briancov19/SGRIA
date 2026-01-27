using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/sesiones")]
public class SesionesController : ControllerBase
{
    private readonly SenalPedidoService _pedidoService;
    private readonly TagVotoService _tagVotoService;

    public SesionesController(SenalPedidoService pedidoService, TagVotoService tagVotoService)
    {
        _pedidoService = pedidoService;
        _tagVotoService = tagVotoService;
    }

    /// <summary>
    /// Confirma un pedido en una sesión de mesa
    /// </summary>
    [HttpPost("{sesionId}/pedidos")]
    public async Task<IActionResult> ConfirmarPedido(
        [FromRoute] int sesionId,
        [FromBody] SenalPedidoCreateDto dto,
        CancellationToken ct)
    {
        try
        {
            var clientId = Request.Headers["X-Client-Id"].FirstOrDefault();
            var pedido = await _pedidoService.ConfirmarPedidoAsync(sesionId, clientId, dto, ct);
            
            // Devolver X-Client-Id si no estaba presente
            if (string.IsNullOrWhiteSpace(clientId))
            {
                Response.Headers["X-Client-Id"] = Guid.NewGuid().ToString();
            }
            
            return CreatedAtAction(
                nameof(GetPedido),
                new { pedidoId = pedido.Id },
                pedido);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // 429 Too Many Requests para rate limiting
            if (ex.Message.Contains("Límite") || ex.Message.Contains("excedido"))
            {
                return StatusCode(429, new { error = ex.Message });
            }
            // 403/409 para sesión expirada
            if (ex.Message.Contains("expirada") || ex.Message.Contains("no válida"))
            {
                return StatusCode(409, new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("pedidos/{pedidoId}")]
    public async Task<IActionResult> GetPedido(int pedidoId, CancellationToken ct)
    {
        var pedido = await _pedidoService.GetByIdAsync(pedidoId, ct);
        return pedido == null ? NotFound() : Ok(pedido);
    }

    /// <summary>
    /// Crea o actualiza un voto de tag para un item en una sesión (upsert).
    /// Valida que la sesión esté activa y que el item pertenezca al restaurante de la sesión.
    /// </summary>
    [HttpPost("{sesionId}/items/{itemMenuId}/tags")]
    public async Task<IActionResult> CrearOActualizarTagVoto(
        [FromRoute] int sesionId,
        [FromRoute] int itemMenuId,
        [FromBody] TagVotoCreateDto dto,
        CancellationToken ct)
    {
        try
        {
            var voto = await _tagVotoService.CrearOActualizarVotoAsync(sesionId, itemMenuId, dto, ct);
            return Ok(voto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
