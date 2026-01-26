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

    public MesasQrController(MesaQrService mesaQrService, SesionMesaService sesionService)
    {
        _mesaQrService = mesaQrService;
        _sesionService = sesionService;
    }

    /// <summary>
    /// Resuelve una mesa desde su QR token y crea o reutiliza una sesión activa
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
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
