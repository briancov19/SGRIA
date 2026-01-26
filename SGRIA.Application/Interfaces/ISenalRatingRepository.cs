using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface ISenalRatingRepository
{
    Task<SenalRating?> GetByPedidoIdAsync(int pedidoId, CancellationToken ct);
    Task<SenalRating> CreateAsync(SenalRating rating, CancellationToken ct);
    Task<SenalRating> UpdateAsync(SenalRating rating, CancellationToken ct);
}
