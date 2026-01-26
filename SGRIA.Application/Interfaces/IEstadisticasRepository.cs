using SGRIA.Application.DTOs;

namespace SGRIA.Application.Interfaces;

public interface IEstadisticasRepository
{
    Task<List<ItemRankingDto>> GetRankingPedidosAsync(
        int restauranteId,
        DateTime fechaDesde,
        DateTime fechaHasta,
        int? top = null,
        CancellationToken ct = default);

    Task<List<ItemTrendingDto>> GetTrendingAsync(
        int restauranteId,
        int minutos,
        CancellationToken ct = default);

    Task<List<ItemRecomendadoDto>> GetRecomendadosAsync(
        int restauranteId,
        DateTime fechaDesde,
        DateTime fechaHasta,
        int minimoRatings = 5,
        CancellationToken ct = default);
}
