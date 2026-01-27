using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;

namespace SGRIA.Application.Services;

public class EstadisticasService
{
    private readonly IEstadisticasRepository _estadisticasRepo;

    public EstadisticasService(IEstadisticasRepository estadisticasRepo)
    {
        _estadisticasRepo = estadisticasRepo;
    }

    public async Task<RankingResponseDto> GetRankingAsync(
        int restauranteId,
        string periodo,
        decimal? minConfianza = null,
        CancellationToken ct = default)
    {
        var (fechaDesde, fechaHasta) = ParsePeriodo(periodo);
        fechaHasta = fechaHasta.AddDays(1).AddTicks(-1); // Incluir todo el último día

        var items = await _estadisticasRepo.GetRankingPedidosAsync(
            restauranteId,
            fechaDesde,
            fechaHasta,
            top: null,
            minConfianza: minConfianza,
            ct: ct);

        return new RankingResponseDto(
            restauranteId,
            periodo,
            fechaDesde,
            fechaHasta,
            items
        );
    }

    public async Task<TrendingResponseDto> GetTrendingAsync(
        int restauranteId,
        int minutos,
        decimal? minConfianza = null,
        CancellationToken ct = default)
    {
        var items = await _estadisticasRepo.GetTrendingAsync(
            restauranteId,
            minutos,
            minConfianza,
            ct);

        return new TrendingResponseDto(
            restauranteId,
            minutos,
            DateTime.UtcNow,
            items
        );
    }

    public async Task<RecomendadosResponseDto> GetRecomendadosAsync(
        int restauranteId,
        int dias,
        decimal? minConfianza = null,
        CancellationToken ct = default)
    {
        var fechaHasta = DateTime.UtcNow;
        var fechaDesde = fechaHasta.AddDays(-dias);

        var items = await _estadisticasRepo.GetRecomendadosAsync(
            restauranteId,
            fechaDesde,
            fechaHasta,
            minimoRatings: 5,
            minConfianza,
            ct);

        return new RecomendadosResponseDto(
            restauranteId,
            dias,
            fechaDesde,
            fechaHasta,
            MinimoRatings: 5,
            items
        );
    }

    private static (DateTime desde, DateTime hasta) ParsePeriodo(string periodo)
    {
        var ahora = DateTime.UtcNow;
        
        return periodo.ToLower() switch
        {
            "1d" or "1dia" or "today" => (ahora.Date, ahora),
            "7d" or "7dias" or "semana" => (ahora.AddDays(-7).Date, ahora),
            "30d" or "30dias" or "mes" => (ahora.AddDays(-30).Date, ahora),
            "90d" or "90dias" or "trimestre" => (ahora.AddDays(-90).Date, ahora),
            _ => throw new ArgumentException($"Período no válido: {periodo}. Use: 1d, 7d, 30d, 90d")
        };
    }
}
