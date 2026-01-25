using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface IMesaRepository
{
    Task<List<Mesa>> GetAllAsync(CancellationToken ct);
    Task<Mesa?> GetByIdAsync(int id, CancellationToken ct);
    Task<Mesa> AddAsync(Mesa mesa, CancellationToken ct);
}
