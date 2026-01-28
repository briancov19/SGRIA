using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class TagVotoService
{
    private readonly ITagVotoRepository _votoRepo;
    private readonly ISesionMesaRepository _sesionRepo;
    private readonly IItemMenuRepository _itemRepo;
    private readonly ITagRapidoRepository _tagRepo;
    private readonly SesionPublicaService _sesionPublicaService;

    public TagVotoService(
        ITagVotoRepository votoRepo,
        ISesionMesaRepository sesionRepo,
        IItemMenuRepository itemRepo,
        ITagRapidoRepository tagRepo,
        SesionPublicaService sesionPublicaService)
    {
        _votoRepo = votoRepo;
        _sesionRepo = sesionRepo;
        _itemRepo = itemRepo;
        _tagRepo = tagRepo;
        _sesionPublicaService = sesionPublicaService;
    }

    /// <summary>
    /// Crea o actualiza un voto de tag usando el token público de la sesión.
    /// </summary>
    public async Task<TagVotoDto> CrearOActualizarVotoPorTokenAsync(
        string sesPublicToken,
        int itemMenuId,
        TagVotoCreateDto dto,
        CancellationToken ct)
    {
        // Validar sesión activa y no expirada usando token público
        var sesion = await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesPublicToken, ct);

        // Validar item de menú
        var item = await _itemRepo.GetByIdAsync(itemMenuId, ct);
        if (item == null)
            throw new ArgumentException($"Item de menú no encontrado: {itemMenuId}");

        // Validar que item pertenece al restaurante de la sesión
        if (item.RestauranteId != sesion.Mesa.RestauranteId)
            throw new InvalidOperationException(
                $"El item de menú no pertenece al restaurante de esta sesión");

        // Validar tag
        var tag = await _tagRepo.GetByIdAsync(dto.TagId, ct);
        if (tag == null)
            throw new ArgumentException($"Tag no encontrado: {dto.TagId}");

        if (!tag.Activo)
            throw new InvalidOperationException("El tag no está activo");

        // Validar valor
        if (dto.Valor != 1 && dto.Valor != -1)
            throw new ArgumentException("El valor debe ser +1 o -1");

        // Crear o actualizar voto (upsert)
        var voto = new VotoTagItemMenu
        {
            SesionMesaId = sesion.Id,
            ItemMenuId = itemMenuId,
            TagRapidoId = dto.TagId,
            Valor = dto.Valor
        };

        var votoGuardado = await _votoRepo.CreateOrUpdateAsync(voto, ct);
        
        // Actualizar última actividad de la sesión (touch)
        await _sesionPublicaService.TouchSessionAsync(sesion.Id, ct);

        return new TagVotoDto(
            votoGuardado.Id,
            votoGuardado.TagRapidoId,
            votoGuardado.TagRapido.Nombre,
            votoGuardado.Valor,
            votoGuardado.FechaHora
        );
    }
}
