using Microsoft.EntityFrameworkCore;
using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfEstadisticasRepository : IEstadisticasRepository
{
    private readonly AppDbContext _db;

    public EfEstadisticasRepository(AppDbContext db) => _db = db;

    public async Task<List<ItemRankingDto>> GetRankingPedidosAsync(
        int restauranteId,
        DateTime fechaDesde,
        DateTime fechaHasta,
        int? top = null,
        CancellationToken ct = default)
    {
        // Paso 1: Agrupar pedidos por item de menú
        var pedidosQuery = from p in _db.SenalesPedido
                          where p.ItemMenu.RestauranteId == restauranteId
                             && p.FechaHoraConfirmacion >= fechaDesde
                             && p.FechaHoraConfirmacion <= fechaHasta
                          group p by new
                          {
                              p.ItemMenuId,
                              p.ItemMenu.Nombre,
                              p.ItemMenu.Categoria,
                              p.ItemMenu.Precio
                          } into g
                          select new
                          {
                              g.Key.ItemMenuId,
                              g.Key.Nombre,
                              g.Key.Categoria,
                              g.Key.Precio,
                              TotalPedidos = g.Count(),
                              TotalCantidad = g.Sum(p => p.Cantidad),
                              PedidoIds = g.Select(p => p.Id)
                          };

        var pedidos = await pedidosQuery.ToListAsync(ct);

        if (!pedidos.Any())
        {
            return new List<ItemRankingDto>();
        }

        // Paso 2: Obtener todos los IDs de pedidos
        var pedidoIds = pedidos.SelectMany(p => p.PedidoIds).Distinct().ToList();

        // Paso 3: Obtener ratings para esos pedidos
        var ratings = await _db.SenalesRating
            .Where(r => pedidoIds.Contains(r.SenalPedidoId))
            .Select(r => new { r.SenalPedidoId, r.Puntaje })
            .ToListAsync(ct);

        // Paso 4: Construir resultado en memoria
        var resultado = new List<ItemRankingDto>();

        foreach (var pedido in pedidos)
        {
            var pedidoIdsDelItem = pedido.PedidoIds.ToList();
            var ratingsDelItem = ratings
                .Where(r => pedidoIdsDelItem.Contains(r.SenalPedidoId))
                .Select(r => (decimal)r.Puntaje)
                .ToList();

            resultado.Add(new ItemRankingDto(
                pedido.ItemMenuId,
                pedido.Nombre,
                pedido.Categoria,
                pedido.Precio,
                pedido.TotalPedidos,
                pedido.TotalCantidad,
                ratingsDelItem.Any() ? ratingsDelItem.Average() : null,
                ratingsDelItem.Count
            ));
        }

        // Paso 5: Ordenar y limitar
        var ordenado = resultado
            .OrderByDescending(x => x.TotalPedidos)
            .ThenByDescending(x => x.TotalCantidad)
            .ToList();

        if (top.HasValue)
        {
            return ordenado.Take(top.Value).ToList();
        }

        return ordenado;
    }

    public async Task<List<ItemTrendingDto>> GetTrendingAsync(
        int restauranteId,
        int minutos,
        CancellationToken ct = default)
    {
        var desde = DateTime.UtcNow.AddMinutes(-minutos);

        var query = from p in _db.SenalesPedido
                    where p.ItemMenu.RestauranteId == restauranteId
                       && p.FechaHoraConfirmacion >= desde
                    group p by new
                    {
                        p.ItemMenuId,
                        p.ItemMenu.Nombre,
                        p.ItemMenu.Categoria
                    } into g
                    select new ItemTrendingDto(
                        g.Key.ItemMenuId,
                        g.Key.Nombre,
                        g.Key.Categoria,
                        g.Count(),
                        g.Max(p => p.FechaHoraConfirmacion)
                    );

        return await query
            .OrderByDescending(x => x.PedidosUltimosMinutos)
            .ThenByDescending(x => x.UltimoPedido)
            .ToListAsync(ct);
    }

    public async Task<List<ItemRecomendadoDto>> GetRecomendadosAsync(
        int restauranteId,
        DateTime fechaDesde,
        DateTime fechaHasta,
        int minimoRatings = 5,
        CancellationToken ct = default)
    {
        var query = from r in _db.SenalesRating
                    join p in _db.SenalesPedido on r.SenalPedidoId equals p.Id
                    where p.ItemMenu.RestauranteId == restauranteId
                       && r.FechaHora >= fechaDesde
                       && r.FechaHora <= fechaHasta
                    group r by new
                    {
                        p.ItemMenuId,
                        p.ItemMenu.Nombre,
                        p.ItemMenu.Categoria,
                        p.ItemMenu.Precio
                    } into g
                    where g.Count() >= minimoRatings
                    select new ItemRecomendadoDto(
                        g.Key.ItemMenuId,
                        g.Key.Nombre,
                        g.Key.Categoria,
                        g.Key.Precio,
                        g.Average(r => (decimal)r.Puntaje),
                        g.Count(),
                        g.Count(r => r.Puntaje == 1),
                        g.Count(r => r.Puntaje == 0),
                        g.Count(r => r.Puntaje == -1)
                    );

        return await query
            .OrderByDescending(x => x.PromedioRating)
            .ThenByDescending(x => x.TotalRatings)
            .ToListAsync(ct);
    }
}
