using Microsoft.Extensions.Configuration;
using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;

namespace SGRIA.Application.Services;

public class FeedService
{
    private readonly SesionPublicaService _sesionPublicaService;
    private readonly EstadisticasService _estadisticasService;
    private readonly decimal _minConfianzaFeedPublico;

    public FeedService(
        SesionPublicaService sesionPublicaService,
        EstadisticasService estadisticasService,
        IConfiguration configuration)
    {
        _sesionPublicaService = sesionPublicaService;
        _estadisticasService = estadisticasService;
        _minConfianzaFeedPublico = decimal.Parse(configuration["AntiAbuse:MinConfianzaFeedPublico"] ?? "0.3");
    }

    /// <summary>
    /// Obtiene el feed completo desde un token público de sesión.
    /// El restaurante se obtiene desde sesión → mesa → restaurante.
    /// </summary>
    public async Task<FeedResponseDto> GetFeedPorTokenAsync(
        string sesPublicToken,
        int minutos = 30,
        string periodo = "7d",
        int dias = 30,
        CancellationToken ct = default)
    {
        // Validar sesión activa y no expirada
        var sesion = await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesPublicToken, ct);
        
        // Obtener restaurante desde la sesión (ya está cargado con Include)
        var restauranteId = sesion.Mesa.RestauranteId;

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
            sesion.SesPublicToken,
            trending.Items,
            ranking.Items,
            recomendados.Items
        );
    }
}
