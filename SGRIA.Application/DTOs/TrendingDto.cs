namespace SGRIA.Application.DTOs;

public record ItemTrendingDto(
    int ItemMenuId,
    string Nombre,
    string? Categoria,
    int PedidosUltimosMinutos,
    int MesasUltimosMinutos, // Nuevo: cantidad de mesas/sesiones distintas
    DateTime UltimoPedido
);

public record TrendingResponseDto(
    int RestauranteId,
    int Minutos,
    DateTime Timestamp,
    List<ItemTrendingDto> Items
);
