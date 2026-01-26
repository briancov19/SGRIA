using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface IMesaRepository
{
    Task<List<Mesa>> GetAllAsync(CancellationToken ct);
    Task<Mesa?> GetByIdAsync(int id, CancellationToken ct);
    Task<Mesa?> GetByQrTokenAsync(string qrToken, CancellationToken ct);
    Task<Mesa> AddAsync(Mesa mesa, CancellationToken ct);
    Task<Mesa> UpdateAsync(Mesa mesa, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task LoadRestauranteAsync(Mesa mesa, CancellationToken ct);
}
