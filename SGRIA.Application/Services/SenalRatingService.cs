using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class SenalRatingService
{
    private readonly ISenalRatingRepository _ratingRepo;
    private readonly ISenalPedidoRepository _pedidoRepo;

    public SenalRatingService(
        ISenalRatingRepository ratingRepo,
        ISenalPedidoRepository pedidoRepo)
    {
        _ratingRepo = ratingRepo;
        _pedidoRepo = pedidoRepo;
    }

    public async Task<SenalRatingDto> RegistrarRatingAsync(
        int pedidoId,
        SenalRatingCreateDto dto,
        CancellationToken ct)
    {
        // Validar puntaje
        if (dto.Puntaje < -1 || dto.Puntaje > 1)
        {
            throw new ArgumentException("El puntaje debe ser -1, 0 o 1");
        }

        // Validar pedido
        var pedido = await _pedidoRepo.GetByIdAsync(pedidoId, ct);
        if (pedido == null)
        {
            throw new ArgumentException($"Pedido no encontrado: {pedidoId}");
        }

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
