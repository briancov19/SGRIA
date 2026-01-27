using Microsoft.Extensions.Configuration;
using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;

namespace SGRIA.Application.Services;

public class FeedService
{
    private readonly SesionMesaService _sesionService;
    private readonly EstadisticasService _estadisticasService;
    private readonly IMesaRepository _mesaRepo;
    private readonly decimal _minConfianzaFeedPublico;

    public FeedService(
        SesionMesaService sesionService,
        EstadisticasService estadisticasService,
        IMesaRepository mesaRepo,
        IConfiguration configuration)
    {
        _sesionService = sesionService;
        _estadisticasService = estadisticasService;
        _mesaRepo = mesaRepo;
        _minConfianzaFeedPublico = decimal.Parse(configuration["AntiAbuse:MinConfianzaFeedPublico"] ?? "0.3");
    }

    public async Task<FeedResponseDto> GetFeedAsync(
        string qrToken,
        int minutos = 30,
        string periodo = "7d",
        int dias = 30,
        CancellationToken ct = default)
    {
        // Resolver mesa y crear/reutilizar sesión (feed no requiere clientId, es solo lectura)
        var sesion = await _sesionService.CrearOReutilizarSesionAsync(qrToken, null, null, ct);
        
        // Obtener restaurante desde la mesa
        var mesa = await _mesaRepo.GetByQrTokenAsync(qrToken, ct);
        if (mesa == null)
            throw new InvalidOperationException("Mesa no encontrada después de crear sesión");

        var restauranteId = mesa.RestauranteId;

        // Obtener todas las estadísticas en paralelo con confianza mínima para feed público
        var trendingTask = _estadisticasService.GetTrendingAsync(restauranteId, minutos, _minConfianzaFeedPublico, ct);
        var rankingTask = _estadisticasService.GetRankingAsync(restauranteId, periodo, _minConfianzaFeedPublico, ct);
        var recomendadosTask = _estadisticasService.GetRecomendadosAsync(restauranteId, dias, _minConfianzaFeedPublico, ct);

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
