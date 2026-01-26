using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfTagVotoRepository : ITagVotoRepository
{
    private readonly AppDbContext _db;

    public EfTagVotoRepository(AppDbContext db) => _db = db;

    public Task<VotoTagItemMenu?> GetBySesionItemTagAsync(int sesionId, int itemMenuId, int tagId, CancellationToken ct)
        => _db.VotosTagItemMenu
              .Include(v => v.TagRapido)
              .FirstOrDefaultAsync(v => v.SesionMesaId == sesionId
                                     && v.ItemMenuId == itemMenuId
                                     && v.TagRapidoId == tagId, ct);

    public async Task<VotoTagItemMenu> CreateOrUpdateAsync(VotoTagItemMenu voto, CancellationToken ct)
    {
        var existente = await GetBySesionItemTagAsync(voto.SesionMesaId, voto.ItemMenuId, voto.TagRapidoId, ct);

        if (existente != null)
        {
            // Actualizar voto existente
            existente.Valor = voto.Valor;
            existente.FechaHora = DateTime.UtcNow;
            _db.VotosTagItemMenu.Update(existente);
            await _db.SaveChangesAsync(ct);
            await _db.Entry(existente).Reference(v => v.TagRapido).LoadAsync(ct);
            return existente;
        }

        // Crear nuevo voto
        voto.FechaHora = DateTime.UtcNow;
        _db.VotosTagItemMenu.Add(voto);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(voto).Reference(v => v.TagRapido).LoadAsync(ct);
        return voto;
    }

    public Task<List<VotoTagItemMenu>> GetByItemMenuIdAsync(int itemMenuId, CancellationToken ct)
        => _db.VotosTagItemMenu
              .Include(v => v.TagRapido)
              .Where(v => v.ItemMenuId == itemMenuId)
              .ToListAsync(ct);
}
