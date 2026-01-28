using Microsoft.Extensions.Configuration;
using SGRIA.Application.Interfaces;

namespace SGRIA.Application.Services;

/// <summary>
/// Servicio para validar límites de rate limiting por participante.
/// </summary>
public class RateLimitService
{
    private readonly ISesionParticipanteRepository _participanteRepo;
    private readonly int _maxPedidosPorVentana;
    private readonly int _maxRatingsPorVentana;
    private readonly int _maxTagVotosPorVentana;
    private readonly int _ventanaMinutos;

    public RateLimitService(
        ISesionParticipanteRepository participanteRepo,
        IConfiguration configuration)
    {
        _participanteRepo = participanteRepo;
        _maxPedidosPorVentana = int.Parse(configuration["AntiAbuse:MaxPedidosPorVentana"] ?? "10");
        _maxRatingsPorVentana = int.Parse(configuration["AntiAbuse:MaxRatingsPorVentana"] ?? "10");
        _maxTagVotosPorVentana = int.Parse(configuration["AntiAbuse:MaxTagVotosPorVentana"] ?? "10");
        _ventanaMinutos = int.Parse(configuration["AntiAbuse:VentanaMinutos"] ?? "10");
    }

    /// <summary>
    /// Valida si un participante puede crear un nuevo pedido.
    /// </summary>
    public async Task ValidarLimitePedidosAsync(int sesionParticipanteId, CancellationToken ct)
    {
        var pedidosEnVentana = await _participanteRepo.CountPedidosByParticipanteEnVentanaAsync(
            sesionParticipanteId, 
            _ventanaMinutos, 
            ct);

        if (pedidosEnVentana >= _maxPedidosPorVentana)
        {
            throw new InvalidOperationException(
                $"Límite de pedidos excedido. Máximo {_maxPedidosPorVentana} pedidos cada {_ventanaMinutos} minutos.");
        }
    }

    /// <summary>
    /// Valida si un participante puede actualizar un rating.
    /// </summary>
    public async Task ValidarLimiteRatingsAsync(int sesionParticipanteId, CancellationToken ct)
    {
        var ratingsEnVentana = await _participanteRepo.CountRatingsByParticipanteEnVentanaAsync(
            sesionParticipanteId, 
            _ventanaMinutos, 
            ct);

        if (ratingsEnVentana >= _maxRatingsPorVentana)
        {
            throw new InvalidOperationException(
                $"Límite de ratings excedido. Máximo {_maxRatingsPorVentana} actualizaciones cada {_ventanaMinutos} minutos.");
        }
    }

    /// <summary>
    /// Valida si un participante puede crear/actualizar votos de tag.
    /// </summary>
    public async Task ValidarLimiteTagVotosAsync(int sesionParticipanteId, CancellationToken ct)
    {
        var votosEnVentana = await _participanteRepo.CountTagVotosByParticipanteEnVentanaAsync(
            sesionParticipanteId,
            _ventanaMinutos,
            ct);

        if (votosEnVentana >= _maxTagVotosPorVentana)
        {
            throw new InvalidOperationException(
                $"Límite de votos de tag excedido. Máximo {_maxTagVotosPorVentana} cada {_ventanaMinutos} minutos.");
        }
    }
}
