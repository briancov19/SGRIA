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

    public TagVotoService(
        ITagVotoRepository votoRepo,
        ISesionMesaRepository sesionRepo,
        IItemMenuRepository itemRepo,
        ITagRapidoRepository tagRepo)
    {
        _votoRepo = votoRepo;
        _sesionRepo = sesionRepo;
        _itemRepo = itemRepo;
        _tagRepo = tagRepo;
    }

    public async Task<TagVotoDto> CrearOActualizarVotoAsync(
        int sesionId,
        int itemMenuId,
        TagVotoCreateDto dto,
        CancellationToken ct)
    {
        // Validar sesión activa
        var sesion = await _sesionRepo.GetByIdAsync(sesionId, ct);
        if (sesion == null)
            throw new ArgumentException($"Sesión no encontrada: {sesionId}");

        if (sesion.FechaHoraFin.HasValue)
            throw new InvalidOperationException("La sesión ya está cerrada");

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
            SesionMesaId = sesionId,
            ItemMenuId = itemMenuId,
            TagRapidoId = dto.TagId,
            Valor = dto.Valor
        };

        var votoGuardado = await _votoRepo.CreateOrUpdateAsync(voto, ct);

        return new TagVotoDto(
            votoGuardado.Id,
            votoGuardado.TagRapidoId,
            votoGuardado.TagRapido.Nombre,
            votoGuardado.Valor,
            votoGuardado.FechaHora
        );
    }
}
