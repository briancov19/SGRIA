using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class SenalRatingService
{
    private readonly ISenalRatingRepository _ratingRepo;
    private readonly ISenalPedidoRepository _pedidoRepo;
    private readonly AnonDeviceService _deviceService;
    private readonly ISesionParticipanteRepository _participanteRepo;
    private readonly RateLimitService _rateLimitService;
    private readonly SesionPublicaService _sesionPublicaService;

    public SenalRatingService(
        ISenalRatingRepository ratingRepo,
        ISenalPedidoRepository pedidoRepo,
        AnonDeviceService deviceService,
        ISesionParticipanteRepository participanteRepo,
        RateLimitService rateLimitService,
        SesionPublicaService sesionPublicaService)
    {
        _ratingRepo = ratingRepo;
        _pedidoRepo = pedidoRepo;
        _deviceService = deviceService;
        _participanteRepo = participanteRepo;
        _rateLimitService = rateLimitService;
        _sesionPublicaService = sesionPublicaService;
    }

    public async Task<SenalRatingDto> RegistrarRatingAsync(
        int pedidoId,
        string? clientId,
        SenalRatingCreateDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("X-Client-Id es requerido.");

        if (dto.Puntaje < -1 || dto.Puntaje > 1)
            throw new ArgumentException("El puntaje debe ser -1, 0 o 1");

        var pedido = await _pedidoRepo.GetByIdAsync(pedidoId, ct);
        if (pedido == null)
            throw new ArgumentException("Pedido no encontrado.");

        var sesion = pedido.SesionMesa;
        if (sesion == null)
            throw new ArgumentException("Sesión no encontrada para este pedido.");

        try
        {
            await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesion.SesPublicToken, ct);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("Sesión expirada. Por favor, re-escanea el QR.");
        }

        var device = await _deviceService.GetOrCreateDeviceAsync(clientId!, ct);
        var participante = await _participanteRepo.GetActivoBySesionAndDeviceHashAsync(
            pedido.SesionMesaId, 
            device.DeviceHash, 
            ct);

        if (participante == null)
            throw new InvalidOperationException("Debes escanear el QR de la mesa para unirte a la sesión antes de calificar.");

        var minutosDesdeActividad = (DateTime.UtcNow - participante.UltimaActividad).TotalMinutes;
        if (minutosDesdeActividad > 10)
        {
            throw new InvalidOperationException(
                "Sesión no válida o expirada. Por favor, escanea el QR nuevamente.");
        }

        await _rateLimitService.ValidarLimiteRatingsAsync(participante.Id, ct);
        participante.UltimaActividad = DateTime.UtcNow;
        await _participanteRepo.UpdateAsync(participante, ct);

        await _sesionPublicaService.TouchSessionAsync(sesion.Id, ct);

        // Verificar si ya existe un rating para este pedido
        var ratingExistente = await _ratingRepo.GetByPedidoIdAsync(pedidoId, ct);

        if (ratingExistente != null)
        {
            // Actualizar rating existente
            ratingExistente.Puntaje = dto.Puntaje;
            ratingExistente.FechaHora = DateTime.UtcNow;
            var ratingActualizado = await _ratingRepo.UpdateAsync(ratingExistente, ct);

            return new SenalRatingDto(
                ratingActualizado.Id,
                ratingActualizado.SenalPedidoId,
                ratingActualizado.Puntaje,
                ratingActualizado.FechaHora
            );
        }

        // Crear nuevo rating
        var rating = new SenalRating
        {
            SenalPedidoId = pedidoId,
            Puntaje = dto.Puntaje,
            FechaHora = DateTime.UtcNow
        };

        var ratingCreado = await _ratingRepo.CreateAsync(rating, ct);

        return new SenalRatingDto(
            ratingCreado.Id,
            ratingCreado.SenalPedidoId,
            ratingCreado.Puntaje,
            ratingCreado.FechaHora
        );
    }
}
