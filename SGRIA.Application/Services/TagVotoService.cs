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
    private readonly AnonDeviceService _deviceService;
    private readonly ISesionParticipanteRepository _participanteRepo;
    private readonly RateLimitService _rateLimitService;

    public TagVotoService(
        ITagVotoRepository votoRepo,
        ISesionMesaRepository sesionRepo,
        IItemMenuRepository itemRepo,
        ITagRapidoRepository tagRepo,
        SesionPublicaService sesionPublicaService,
        AnonDeviceService deviceService,
        ISesionParticipanteRepository participanteRepo,
        RateLimitService rateLimitService)
    {
        _votoRepo = votoRepo;
        _sesionRepo = sesionRepo;
        _itemRepo = itemRepo;
        _tagRepo = tagRepo;
        _sesionPublicaService = sesionPublicaService;
        _deviceService = deviceService;
        _participanteRepo = participanteRepo;
        _rateLimitService = rateLimitService;
    }

    /// <summary>
    /// Crea o actualiza un voto de tag usando el token público de la sesión.
    /// Requiere X-Client-Id y participante con actividad reciente (protección QR + rate limiting).
    /// </summary>
    public async Task<TagVotoDto> CrearOActualizarVotoPorTokenAsync(
        string sesPublicToken,
        int itemMenuId,
        string clientId,
        TagVotoCreateDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("X-Client-Id es requerido.");

        var sesion = await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesPublicToken, ct);

        var device = await _deviceService.GetOrCreateDeviceAsync(clientId, ct);
        var participante = await _participanteRepo.GetActivoBySesionAndDeviceHashAsync(
            sesion.Id,
            device.DeviceHash,
            ct);

        if (participante == null)
            throw new InvalidOperationException("Debes escanear el QR de la mesa para unirte a la sesión antes de votar tags.");

        var minutosDesdeActividad = (DateTime.UtcNow - participante.UltimaActividad).TotalMinutes;
        if (minutosDesdeActividad > 10)
            throw new InvalidOperationException("Sesión no válida o expirada. Por favor, escanea el QR nuevamente.");

        await _rateLimitService.ValidarLimiteTagVotosAsync(participante.Id, ct);

        var item = await _itemRepo.GetByIdAsync(itemMenuId, ct);
        if (item == null)
            throw new ArgumentException("Item de menú no encontrado.");

        if (item.RestauranteId != sesion.Mesa.RestauranteId)
            throw new InvalidOperationException("El item de menú no pertenece al restaurante de esta sesión.");

        var tag = await _tagRepo.GetByIdAsync(dto.TagId, ct);
        if (tag == null)
            throw new ArgumentException("Tag no encontrado.");

        if (!tag.Activo)
            throw new InvalidOperationException("El tag no está activo.");

        if (dto.Valor != 1 && dto.Valor != -1)
            throw new ArgumentException("El valor debe ser +1 o -1");

        participante.UltimaActividad = DateTime.UtcNow;
        await _participanteRepo.UpdateAsync(participante, ct);

        var voto = new VotoTagItemMenu
        {
            SesionMesaId = sesion.Id,
            ItemMenuId = itemMenuId,
            TagRapidoId = dto.TagId,
            Valor = dto.Valor
        };

        var votoGuardado = await _votoRepo.CreateOrUpdateAsync(voto, ct);
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
