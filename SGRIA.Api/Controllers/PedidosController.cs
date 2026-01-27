using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly SenalRatingService _ratingService;

    public PedidosController(SenalRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    /// <summary>
    /// Registra o actualiza un rating para un pedido (👍=1, 😐=0, 👎=-1)
    /// </summary>
    [HttpPost("{pedidoId}/rating")]
    public async Task<IActionResult> RegistrarRating(
        [FromRoute] int pedidoId,
        [FromBody] SenalRatingCreateDto dto,
        CancellationToken ct)
    {
        try
        {
            var clientId = Request.Headers["X-Client-Id"].FirstOrDefault();
            var rating = await _ratingService.RegistrarRatingAsync(pedidoId, clientId, dto, ct);
            
            // Devolver X-Client-Id si no estaba presente
            if (string.IsNullOrWhiteSpace(clientId))
            {
                Response.Headers["X-Client-Id"] = Guid.NewGuid().ToString();
            }
            
            return Ok(rating);
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
}
