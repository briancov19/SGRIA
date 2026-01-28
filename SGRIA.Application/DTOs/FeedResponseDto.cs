namespace SGRIA.Application.DTOs;

public record FeedResponseDto(
    DateTime Timestamp,
    string SesPublicToken,
    List<ItemTrendingDto> Trending,
    List<ItemRankingDto> Ranking,
    List<ItemRecomendadoDto> Recomendados
);
