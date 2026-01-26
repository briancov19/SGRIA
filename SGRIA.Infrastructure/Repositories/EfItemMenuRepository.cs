using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfItemMenuRepository : IItemMenuRepository
{
    private readonly AppDbContext _db;

    public EfItemMenuRepository(AppDbContext db) => _db = db;

    public Task<ItemMenu?> GetByIdAsync(int id, CancellationToken ct)
        => _db.ItemsMenu
              .Include(i => i.Restaurante)
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<ItemMenu>> GetActivosByRestauranteIdAsync(int restauranteId, CancellationToken ct)
        => _db.ItemsMenu
              .Include(i => i.Restaurante)
              .AsNoTracking()
              .Where(x => x.RestauranteId == restauranteId && x.Activo)
              .OrderBy(x => x.Categoria)
              .ThenBy(x => x.Nombre)
              .ToListAsync(ct);

    public Task<List<ItemMenu>> GetAllByRestauranteIdAsync(int restauranteId, CancellationToken ct)
        => _db.ItemsMenu
              .Include(i => i.Restaurante)
              .AsNoTracking()
              .Where(x => x.RestauranteId == restauranteId)
              .OrderBy(x => x.Categoria)
              .ThenBy(x => x.Nombre)
              .ToListAsync(ct);

    public async Task<ItemMenu> AddAsync(ItemMenu itemMenu, CancellationToken ct)
    {
        _db.ItemsMenu.Add(itemMenu);
        await _db.SaveChangesAsync(ct);
        return itemMenu;
    }

    public async Task<ItemMenu> UpdateAsync(ItemMenu itemMenu, CancellationToken ct)
    {
        _db.ItemsMenu.Update(itemMenu);
        await _db.SaveChangesAsync(ct);
        return itemMenu;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var item = await _db.ItemsMenu.FindAsync(new object[] { id }, ct);
        if (item == null) return false;

        _db.ItemsMenu.Remove(item);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task LoadRestauranteAsync(ItemMenu itemMenu, CancellationToken ct)
    {
        await _db.Entry(itemMenu).Reference(i => i.Restaurante).LoadAsync(ct);
    }
}
