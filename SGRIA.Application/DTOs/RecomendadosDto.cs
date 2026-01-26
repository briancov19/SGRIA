namespace SGRIA.Application.DTOs;

public record ItemRecomendadoDto(
    int ItemMenuId,
    string Nombre,
    string? Categoria,
    decimal? Precio,
    decimal PromedioRating,
    int TotalRatings,
    int RatingsPositivos,
    int RatingsNeutros,
    int RatingsNegativos
);

public record RecomendadosResponseDto(
    int RestauranteId,
    int Dias,
    DateTime FechaDesde,
    DateTime FechaHasta,
    int MinimoRatings,
    List<ItemRecomendadoDto> Items
);
