using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;
using SGRIA.Api.Filters;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/sesiones")]
[ValidateSesPublicToken]
public class SesionesController : ControllerBase
{
    private readonly SenalPedidoService _pedidoService;
    private readonly TagVotoService _tagVotoService;
    private readonly FeedService _feedService;
    private readonly EstadisticasService _estadisticasService;
    private readonly SesionPublicaService _sesionPublicaService;

    public SesionesController(
        SenalPedidoService pedidoService, 
        TagVotoService tagVotoService,
        FeedService feedService,
        EstadisticasService estadisticasService,
        SesionPublicaService sesionPublicaService)
    {
        _pedidoService = pedidoService;
        _tagVotoService = tagVotoService;
        _feedService = feedService;
        _estadisticasService = estadisticasService;
        _sesionPublicaService = sesionPublicaService;
    }

    /// <summary>
    /// Confirma un pedido en una sesión usando el token público.
    /// Valida que la sesión esté activa y no expirada.
    /// </summary>
    /// <param name="sesPublicToken">Token público (GUID) de la sesión.</param>
    [HttpPost("{sesPublicToken}/pedidos")]
    [ProducesResponseType(typeof(SenalPedidoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ConfirmarPedidoPorToken(
        [FromRoute] string sesPublicToken,
        [FromBody] SenalPedidoCreateDto dto,
        CancellationToken ct)
    {
        try
        {
            var clientId = Request.Headers["X-Client-Id"].FirstOrDefault();
            var pedido = await _pedidoService.ConfirmarPedidoPorTokenAsync(sesPublicToken, clientId, dto, ct);
            
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
            // 404/409 para sesión expirada o no encontrada
            if (ex.Message.Contains("expirada") || ex.Message.Contains("no válida") || ex.Message.Contains("no encontrada"))
            {
                return StatusCode(409, new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Obtiene un pedido por ID.</summary>
    [HttpGet("pedidos/{pedidoId}")]
    [ProducesResponseType(typeof(SenalPedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPedido(int pedidoId, CancellationToken ct)
    {
        var pedido = await _pedidoService.GetByIdAsync(pedidoId, ct);
        return pedido == null ? NotFound() : Ok(pedido);
    }

    /// <summary>
    /// Crea o actualiza un voto de tag para un item en una sesión usando token público (upsert).
    /// Valida que la sesión esté activa y que el item pertenezca al restaurante de la sesión.
    /// </summary>
    /// <param name="sesPublicToken">Token público (GUID) de la sesión.</param>
    [HttpPost("{sesPublicToken}/items/{itemMenuId}/tags")]
    [ProducesResponseType(typeof(TagVotoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearOActualizarTagVotoPorToken(
        [FromRoute] string sesPublicToken,
        [FromRoute] int itemMenuId,
        [FromBody] TagVotoCreateDto dto,
        CancellationToken ct)
    {
        try
        {
            var voto = await _tagVotoService.CrearOActualizarVotoPorTokenAsync(sesPublicToken, itemMenuId, dto, ct);
            return Ok(voto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("expirada"))
                return StatusCode(409, new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el feed completo (trending, ranking, recomendados) desde un token público de sesión.
    /// El restaurante se obtiene automáticamente desde sesión → mesa → restaurante.
    /// </summary>
    [HttpGet("{sesPublicToken}/feed")]
    public async Task<IActionResult> GetFeed(
        [FromRoute] string sesPublicToken,
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

            var feed = await _feedService.GetFeedPorTokenAsync(sesPublicToken, min, periodo, dias, ct);
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
            if (ex.Message.Contains("expirada"))
                return StatusCode(409, new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene trending (lo que se está pidiendo ahora) desde un token público de sesión.
    /// </summary>
    /// <param name="sesPublicToken">Token público (GUID) de la sesión.</param>
    [HttpGet("{sesPublicToken}/trending")]
    [ProducesResponseType(typeof(TrendingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetTrending(
        [FromRoute] string sesPublicToken,
        [FromQuery] int min = 30,
        [FromQuery] decimal? minConfianza = null,
        CancellationToken ct = default)
    {
        try
        {
            if (min <= 0 || min > 1440)
                return BadRequest(new { error = "El parámetro 'min' debe estar entre 1 y 1440" });

            // Obtener sesión y restaurante
            var sesion = await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesPublicToken, ct);
            var restauranteId = sesion.Mesa.RestauranteId;

            var trending = await _estadisticasService.GetTrendingAsync(restauranteId, min, minConfianza, ct);
            return Ok(trending);
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("no encontrada"))
                return NotFound(new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("expirada"))
                return StatusCode(409, new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene ranking de platos más pedidos desde un token público de sesión.
    /// </summary>
    /// <param name="sesPublicToken">Token público (GUID) de la sesión.</param>
    [HttpGet("{sesPublicToken}/ranking")]
    [ProducesResponseType(typeof(RankingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetRanking(
        [FromRoute] string sesPublicToken,
        [FromQuery] string periodo = "7d",
        [FromQuery] decimal? minConfianza = null,
        CancellationToken ct = default)
    {
        try
        {
            // Obtener sesión y restaurante
            var sesion = await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesPublicToken, ct);
            var restauranteId = sesion.Mesa.RestauranteId;

            var ranking = await _estadisticasService.GetRankingAsync(restauranteId, periodo, minConfianza, ct);
            return Ok(ranking);
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("no encontrada"))
                return NotFound(new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("expirada"))
                return StatusCode(409, new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene platos más recomendados desde un token público de sesión.
    /// </summary>
    /// <param name="sesPublicToken">Token público (GUID) de la sesión.</param>
    [HttpGet("{sesPublicToken}/recomendados")]
    [ProducesResponseType(typeof(RecomendadosResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetRecomendados(
        [FromRoute] string sesPublicToken,
        [FromQuery] int dias = 30,
        [FromQuery] decimal? minConfianza = null,
        CancellationToken ct = default)
    {
        try
        {
            if (dias <= 0 || dias > 365)
                return BadRequest(new { error = "El parámetro 'dias' debe estar entre 1 y 365" });

            // Obtener sesión y restaurante
            var sesion = await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesPublicToken, ct);
            var restauranteId = sesion.Mesa.RestauranteId;

            var recomendados = await _estadisticasService.GetRecomendadosAsync(restauranteId, dias, minConfianza, ct);
            return Ok(recomendados);
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("no encontrada"))
                return NotFound(new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("expirada"))
                return StatusCode(409, new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
    }
}
