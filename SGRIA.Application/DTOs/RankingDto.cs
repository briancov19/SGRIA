namespace SGRIA.Application.DTOs;

public record ItemRankingDto(
    int ItemMenuId,
    string Nombre,
    string? Categoria,
    decimal? Precio,
    int TotalPedidos,
    int TotalCantidad,
    decimal? PromedioRating,
    int TotalRatings
);

public record RankingResponseDto(
    int RestauranteId,
    string Periodo,
    DateTime FechaDesde,
    DateTime FechaHasta,
    List<ItemRankingDto> Items
);
