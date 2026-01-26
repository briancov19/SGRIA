using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfItemMenuAliasRepository : IItemMenuAliasRepository
{
    private readonly AppDbContext _db;

    public EfItemMenuAliasRepository(AppDbContext db) => _db = db;

    public Task<List<ItemMenuAlias>> GetByItemMenuIdAsync(int itemMenuId, CancellationToken ct)
        => _db.ItemsMenuAlias
              .Include(a => a.ItemMenu)
              .AsNoTracking()
              .Where(a => a.ItemMenuId == itemMenuId)
              .OrderBy(a => a.AliasTexto)
              .ToListAsync(ct);

    public Task<ItemMenuAlias?> GetByIdAsync(int id, CancellationToken ct)
        => _db.ItemsMenuAlias
              .Include(a => a.ItemMenu)
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<ItemMenuAlias> AddAsync(ItemMenuAlias alias, CancellationToken ct)
    {
        _db.ItemsMenuAlias.Add(alias);
        await _db.SaveChangesAsync(ct);
        return alias;
    }

    public async Task<ItemMenuAlias> UpdateAsync(ItemMenuAlias alias, CancellationToken ct)
    {
        _db.ItemsMenuAlias.Update(alias);
        await _db.SaveChangesAsync(ct);
        return alias;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var alias = await _db.ItemsMenuAlias.FindAsync(new object[] { id }, ct);
        if (alias == null) return false;

        _db.ItemsMenuAlias.Remove(alias);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task LoadItemMenuAsync(ItemMenuAlias alias, CancellationToken ct)
    {
        await _db.Entry(alias).Reference(a => a.ItemMenu).LoadAsync(ct);
    }
}
