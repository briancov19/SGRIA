using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface IItemMenuRepository
{
    Task<ItemMenu?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<ItemMenu>> GetActivosByRestauranteIdAsync(int restauranteId, CancellationToken ct);
    Task<List<ItemMenu>> GetAllByRestauranteIdAsync(int restauranteId, CancellationToken ct);
    Task<ItemMenu> AddAsync(ItemMenu itemMenu, CancellationToken ct);
    Task<ItemMenu> UpdateAsync(ItemMenu itemMenu, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task LoadRestauranteAsync(ItemMenu itemMenu, CancellationToken ct);
}
