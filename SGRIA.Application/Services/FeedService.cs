using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;

namespace SGRIA.Application.Services;

public class FeedService
{
    private readonly SesionMesaService _sesionService;
    private readonly EstadisticasService _estadisticasService;
    private readonly IMesaRepository _mesaRepo;

    public FeedService(
        SesionMesaService sesionService,
        EstadisticasService estadisticasService,
        IMesaRepository mesaRepo)
    {
        _sesionService = sesionService;
        _estadisticasService = estadisticasService;
        _mesaRepo = mesaRepo;
    }

    public async Task<FeedResponseDto> GetFeedAsync(
        string qrToken,
        int minutos = 30,
        string periodo = "7d",
        int dias = 30,
        CancellationToken ct = default)
    {
        // Resolver mesa y crear/reutilizar sesión
        var sesion = await _sesionService.CrearOReutilizarSesionAsync(qrToken, null, ct);
        
        // Obtener restaurante desde la mesa
        var mesa = await _mesaRepo.GetByQrTokenAsync(qrToken, ct);
        if (mesa == null)
            throw new InvalidOperationException("Mesa no encontrada después de crear sesión");

        var restauranteId = mesa.RestauranteId;

        // Obtener todas las estadísticas en paralelo
        var trendingTask = _estadisticasService.GetTrendingAsync(restauranteId, minutos, ct);
        var rankingTask = _estadisticasService.GetRankingAsync(restauranteId, periodo, ct);
        var recomendadosTask = _estadisticasService.GetRecomendadosAsync(restauranteId, dias, ct);

        await Task.WhenAll(trendingTask, rankingTask, recomendadosTask);

        var trending = await trendingTask;
        var ranking = await rankingTask;
        var recomendados = await recomendadosTask;

        return new FeedResponseDto(
            DateTime.UtcNow,
            sesion.Id,
            trending.Items,
            ranking.Items,
            recomendados.Items
        );
    }
}
