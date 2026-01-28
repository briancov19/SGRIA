using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/mesas")]
public class MesasQrController : ControllerBase
{
    private readonly MesaQrService _mesaQrService;
    private readonly SesionPublicaService _sesionPublicaService;
    private readonly FeedService _feedService;

    public MesasQrController(
        MesaQrService mesaQrService,
        SesionPublicaService sesionPublicaService,
        FeedService feedService)
    {
        _mesaQrService = mesaQrService;
        _sesionPublicaService = sesionPublicaService;
        _feedService = feedService;
    }

    /// <summary>
    /// Resuelve una mesa desde su QR token y crea o reutiliza una sesión activa.
    /// Devuelve un token público (sesPublicToken) que debe usarse en endpoints públicos posteriores.
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
            var clientId = Request.Headers["X-Client-Id"].FirstOrDefault();
            var sesion = await _sesionPublicaService.GetOrCreateSessionByQrTokenAsync(qrToken, clientId, dto, ct);
            
            // Devolver X-Client-Id si no estaba presente (generar nuevo GUID)
            if (string.IsNullOrWhiteSpace(clientId))
            {
                var nuevoClientId = Guid.NewGuid().ToString();
                Response.Headers["X-Client-Id"] = nuevoClientId;
            }
            
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

}
