namespace SGRIA.Application.DTOs;

public record ItemSocialDto(
    int ItemMenuId,
    string Nombre,
    string? Categoria,
    int PedidosUltimosMinutos,
    int MesasUltimosMinutos,
    int TotalPedidosPeriodo,
    decimal? PromedioRating,
    int TotalRatings,
    int RatingsPositivos,
    int RatingsNeutros,
    int RatingsNegativos
);
