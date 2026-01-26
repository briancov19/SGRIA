namespace SGRIA.Application.DTOs;

public record ItemTrendingDto(
    int ItemMenuId,
    string Nombre,
    string? Categoria,
    int PedidosUltimosMinutos,
    DateTime UltimoPedido
);

public record TrendingResponseDto(
    int RestauranteId,
    int Minutos,
    DateTime Timestamp,
    List<ItemTrendingDto> Items
);
