using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfSenalRatingRepository : ISenalRatingRepository
{
    private readonly AppDbContext _db;

    public EfSenalRatingRepository(AppDbContext db) => _db = db;

    public Task<SenalRating?> GetByPedidoIdAsync(int pedidoId, CancellationToken ct)
        => _db.SenalesRating
              .FirstOrDefaultAsync(x => x.SenalPedidoId == pedidoId, ct);

    public async Task<SenalRating> CreateAsync(SenalRating rating, CancellationToken ct)
    {
        rating.FechaHora = DateTime.UtcNow;
        _db.SenalesRating.Add(rating);
        await _db.SaveChangesAsync(ct);
        return rating;
    }

    public async Task<SenalRating> UpdateAsync(SenalRating rating, CancellationToken ct)
    {
        rating.FechaHora = DateTime.UtcNow;
        _db.SenalesRating.Update(rating);
        await _db.SaveChangesAsync(ct);
        return rating;
    }
}
