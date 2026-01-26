using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface IItemMenuAliasRepository
{
    Task<List<ItemMenuAlias>> GetByItemMenuIdAsync(int itemMenuId, CancellationToken ct);
    Task<ItemMenuAlias?> GetByIdAsync(int id, CancellationToken ct);
    Task<ItemMenuAlias> AddAsync(ItemMenuAlias alias, CancellationToken ct);
    Task<ItemMenuAlias> UpdateAsync(ItemMenuAlias alias, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task LoadItemMenuAsync(ItemMenuAlias alias, CancellationToken ct);
}
