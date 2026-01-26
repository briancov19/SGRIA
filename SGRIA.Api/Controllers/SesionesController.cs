using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/sesiones")]
public class SesionesController : ControllerBase
{
    private readonly SenalPedidoService _pedidoService;

    public SesionesController(SenalPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
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
            var pedido = await _pedidoService.ConfirmarPedidoAsync(sesionId, dto, ct);
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
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("pedidos/{pedidoId}")]
    public async Task<IActionResult> GetPedido(int pedidoId, CancellationToken ct)
    {
        var pedido = await _pedidoService.GetByIdAsync(pedidoId, ct);
        return pedido == null ? NotFound() : Ok(pedido);
    }
}
