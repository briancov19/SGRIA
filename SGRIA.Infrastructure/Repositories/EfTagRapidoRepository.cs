using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfTagRapidoRepository : ITagRapidoRepository
{
    private readonly AppDbContext _db;

    public EfTagRapidoRepository(AppDbContext db) => _db = db;

    public Task<List<TagRapido>> GetAllAsync(CancellationToken ct)
        => _db.TagsRapido
              .AsNoTracking()
              .OrderBy(t => t.Nombre)
              .ToListAsync(ct);

    public Task<List<TagRapido>> GetActivosAsync(CancellationToken ct)
        => _db.TagsRapido
              .AsNoTracking()
              .Where(t => t.Activo)
              .OrderBy(t => t.Nombre)
              .ToListAsync(ct);

    public Task<TagRapido?> GetByIdAsync(int id, CancellationToken ct)
        => _db.TagsRapido
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<TagRapido> AddAsync(TagRapido tag, CancellationToken ct)
    {
        _db.TagsRapido.Add(tag);
        await _db.SaveChangesAsync(ct);
        return tag;
    }

    public async Task<TagRapido> UpdateAsync(TagRapido tag, CancellationToken ct)
    {
        _db.TagsRapido.Update(tag);
        await _db.SaveChangesAsync(ct);
        return tag;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var tag = await _db.TagsRapido.FindAsync(new object[] { id }, ct);
        if (tag == null) return false;

        _db.TagsRapido.Remove(tag);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
