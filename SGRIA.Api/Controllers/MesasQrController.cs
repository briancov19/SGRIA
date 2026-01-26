using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/mesas")]
public class MesasQrController : ControllerBase
{
    private readonly MesaQrService _mesaQrService;
    private readonly SesionMesaService _sesionService;
    private readonly FeedService _feedService;

    public MesasQrController(
        MesaQrService mesaQrService,
        SesionMesaService sesionService,
        FeedService feedService)
    {
        _mesaQrService = mesaQrService;
        _sesionService = sesionService;
        _feedService = feedService;
    }

    /// <summary>
    /// Resuelve una mesa desde su QR token y crea o reutiliza una sesión activa
    /// </summary>
    /// <summary>
    /// Resuelve una mesa desde su QR token y crea o reutiliza una sesión activa.
    /// Si la sesión existente tiene más de 90 minutos sin actividad, se cierra automáticamente.
    /// </summary>
    [HttpPost("qr/{qrToken}/sesion")]
    public async Task<IActionResult> CrearOReutilizarSesion(
        [FromRoute] string qrToken,
        [FromBody] SesionMesaCreateDto? dto,
        CancellationToken ct)
    {
        try
        {
            var sesion = await _sesionService.CrearOReutilizarSesionAsync(qrToken, dto, ct);
            return Ok(sesion);
        }
        catch (ArgumentException ex)
        {
            // QR token no encontrado → 404
            if (ex.Message.Contains("no encontrada"))
                return NotFound(new { error = ex.Message });
            
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Mesa no activa → 409 Conflict
            if (ex.Message.Contains("no está activa"))
                return Conflict(new { error = ex.Message });
            
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el feed completo (trending, ranking, recomendados) para una mesa desde su QR token.
    /// Crea o reutiliza una sesión automáticamente.
    /// </summary>
    [HttpGet("qr/{qrToken}/feed")]
    public async Task<IActionResult> GetFeed(
        [FromRoute] string qrToken,
        [FromQuery] int min = 30,
        [FromQuery] string periodo = "7d",
        [FromQuery] int dias = 30,
        CancellationToken ct = default)
    {
        try
        {
            if (min <= 0 || min > 1440)
                return BadRequest(new { error = "El parámetro 'min' debe estar entre 1 y 1440" });

            if (dias <= 0 || dias > 365)
                return BadRequest(new { error = "El parámetro 'dias' debe estar entre 1 y 365" });

            var feed = await _feedService.GetFeedAsync(qrToken, min, periodo, dias, ct);
            return Ok(feed);
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("no encontrada"))
                return NotFound(new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("no está activa"))
                return Conflict(new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
    }
}
