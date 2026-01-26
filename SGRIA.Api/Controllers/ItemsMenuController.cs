using Microsoft.AspNetCore.Mvc;
using SGRIA.Application.DTOs;
using SGRIA.Application.Services;

namespace SGRIA.Api.Controllers;

[ApiController]
[Route("api/restaurantes/{restauranteId}/items-menu")]
public class ItemsMenuController : ControllerBase
{
    private readonly ItemMenuService _service;
    private readonly ItemSocialService _socialService;

    public ItemsMenuController(ItemMenuService service, ItemSocialService socialService)
    {
        _service = service;
        _socialService = socialService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromRoute] int restauranteId,
        [FromQuery] bool soloActivos = true,
        CancellationToken ct = default)
        => Ok(await _service.GetByRestauranteIdAsync(restauranteId, soloActivos, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromRoute] int restauranteId,
        [FromBody] ItemMenuCreateDto dto,
        CancellationToken ct)
    {
        try
        {
            // Asegurar que el restauranteId del body coincida con el de la ruta
            var createDto = dto with { RestauranteId = restauranteId };
            var created = await _service.CreateAsync(createDto, ct);
            return CreatedAtAction(nameof(GetById), new { restauranteId, id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ItemMenuUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto, ct);
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
        var deleted = await _service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Obtiene estadísticas sociales detalladas de un item de menú
    /// </summary>
    [HttpGet("{id:int}/social")]
    public async Task<IActionResult> GetSocial(
        [FromRoute] int id,
        [FromQuery] int min = 30,
        [FromQuery] int dias = 30,
        [FromQuery] string periodo = "7d",
        CancellationToken ct = default)
    {
        try
        {
            if (min <= 0 || min > 1440)
                return BadRequest(new { error = "El parámetro 'min' debe estar entre 1 y 1440" });

            if (dias <= 0 || dias > 365)
                return BadRequest(new { error = "El parámetro 'dias' debe estar entre 1 y 365" });

            var social = await _socialService.GetItemSocialAsync(id, min, dias, periodo, ct);
            return social is null ? NotFound() : Ok(social);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
