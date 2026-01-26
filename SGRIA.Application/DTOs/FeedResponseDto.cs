namespace SGRIA.Application.DTOs;

public record FeedResponseDto(
    DateTime Timestamp,
    int SesionId,
    List<ItemTrendingDto> Trending,
    List<ItemRankingDto> Ranking,
    List<ItemRecomendadoDto> Recomendados
);
