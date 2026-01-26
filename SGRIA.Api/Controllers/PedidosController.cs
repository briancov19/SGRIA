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
            var rating = await _ratingService.RegistrarRatingAsync(pedidoId, dto, ct);
            return Ok(rating);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
