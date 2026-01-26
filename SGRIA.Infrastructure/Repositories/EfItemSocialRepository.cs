using Microsoft.EntityFrameworkCore;
using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfItemSocialRepository : IItemSocialRepository
{
    private readonly AppDbContext _db;

    public EfItemSocialRepository(AppDbContext db) => _db = db;

    public async Task<ItemSocialDto?> GetItemSocialAsync(
        int itemMenuId,
        int minutos,
        int dias,
        DateTime fechaDesdePeriodo,
        DateTime fechaHastaPeriodo,
        CancellationToken ct = default)
    {
        var item = await _db.ItemsMenu
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemMenuId, ct);

        if (item == null)
            return null;

        var desdeMinutos = DateTime.UtcNow.AddMinutes(-minutos);
        var desdeDias = DateTime.UtcNow.AddDays(-dias);

        // Pedidos últimos X minutos
        var pedidosUltimosMinutos = await _db.SenalesPedido
            .Where(p => p.ItemMenuId == itemMenuId && p.FechaHoraConfirmacion >= desdeMinutos)
            .CountAsync(ct);

        // Mesas distintas últimos X minutos
        var mesasUltimosMinutos = await _db.SenalesPedido
            .Where(p => p.ItemMenuId == itemMenuId && p.FechaHoraConfirmacion >= desdeMinutos)
            .Select(p => p.SesionMesaId)
            .Distinct()
            .CountAsync(ct);

        // Total pedidos en período
        var totalPedidosPeriodo = await _db.SenalesPedido
            .Where(p => p.ItemMenuId == itemMenuId
                     && p.FechaHoraConfirmacion >= fechaDesdePeriodo
                     && p.FechaHoraConfirmacion <= fechaHastaPeriodo)
            .CountAsync(ct);

        // Ratings últimos N días
        var ratingsQuery = from r in _db.SenalesRating
                          join p in _db.SenalesPedido on r.SenalPedidoId equals p.Id
                          where p.ItemMenuId == itemMenuId
                             && r.FechaHora >= desdeDias
                          select r.Puntaje;

        var ratings = await ratingsQuery.ToListAsync(ct);

        var promedioRating = ratings.Any() ? (decimal?)ratings.Average(r => (decimal)r) : null;
        var totalRatings = ratings.Count;
        var ratingsPositivos = ratings.Count(r => r == 1);
        var ratingsNeutros = ratings.Count(r => r == 0);
        var ratingsNegativos = ratings.Count(r => r == -1);

        return new ItemSocialDto(
            item.Id,
            item.Nombre,
            item.Categoria,
            pedidosUltimosMinutos,
            mesasUltimosMinutos,
            totalPedidosPeriodo,
            promedioRating,
            totalRatings,
            ratingsPositivos,
            ratingsNeutros,
            ratingsNegativos
        );
    }
}
