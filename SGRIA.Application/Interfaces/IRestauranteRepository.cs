using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface IRestauranteRepository
{
    Task<List<Restaurante>> GetAllAsync(CancellationToken ct);
    Task<Restaurante?> GetByIdAsync(int id, CancellationToken ct);
    Task<Restaurante> AddAsync(Restaurante restaurante, CancellationToken ct);
    Task<Restaurante> UpdateAsync(Restaurante restaurante, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
