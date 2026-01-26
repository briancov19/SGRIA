using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/restaurantes")]
public class RestaurantesController : ControllerBase
{
    private readonly RestauranteService _restauranteService;
    private readonly EstadisticasService _estadisticasService;
    private readonly TagRapidoService _tagService;

    public RestaurantesController(
        RestauranteService restauranteService,
        EstadisticasService estadisticasService,
        TagRapidoService tagService)
    {
        _restauranteService = restauranteService;
        _estadisticasService = estadisticasService;
        _tagService = tagService;
    }

    // ABM de Restaurantes
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _restauranteService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _restauranteService.GetByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RestauranteCreateDto dto, CancellationToken ct)
    {
        try
        {
            var created = await _restauranteService.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RestauranteUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var updated = await _restauranteService.UpdateAsync(id, dto, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _restauranteService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    // Estadísticas
    /// <summary>
    /// Obtiene el ranking de platos más pedidos en un período (1d, 7d, 30d, 90d)
    /// </summary>
    [HttpGet("{id}/ranking")]
    public async Task<IActionResult> GetRanking(
        [FromRoute] int id,
        [FromQuery] string periodo = "7d",
        CancellationToken ct = default)
    {
        try
        {
            var ranking = await _estadisticasService.GetRankingAsync(id, periodo, ct);
            return Ok(ranking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene "lo que se está pidiendo ahora" en los últimos X minutos
    /// </summary>
    [HttpGet("{id}/trending")]
    public async Task<IActionResult> GetTrending(
        [FromRoute] int id,
        [FromQuery] int min = 30,
        CancellationToken ct = default)
    {
        if (min <= 0 || min > 1440)
        {
            return BadRequest(new { error = "El parámetro 'min' debe estar entre 1 y 1440" });
        }

        var trending = await _estadisticasService.GetTrendingAsync(id, min, ct);
        return Ok(trending);
    }

    /// <summary>
    /// Obtiene el ranking de platos más recomendados (por promedio de rating) en los últimos X días
    /// </summary>
    [HttpGet("{id}/recomendados")]
    public async Task<IActionResult> GetRecomendados(
        [FromRoute] int id,
        [FromQuery] int dias = 30,
        CancellationToken ct = default)
    {
        if (dias <= 0 || dias > 365)
        {
            return BadRequest(new { error = "El parámetro 'dias' debe estar entre 1 y 365" });
        }

        var recomendados = await _estadisticasService.GetRecomendadosAsync(id, dias, ct);
        return Ok(recomendados);
    }

    /// <summary>
    /// Obtiene la lista de tags rápidos activos (globales o del restaurante)
    /// </summary>
    [HttpGet("{id}/tags")]
    public async Task<IActionResult> GetTags(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        // Por ahora devolvemos todos los tags activos (globales)
        // En el futuro se pueden filtrar por restaurante si es necesario
        var tags = await _tagService.GetAllAsync(soloActivos: true, ct);
        return Ok(tags);
    }
}
