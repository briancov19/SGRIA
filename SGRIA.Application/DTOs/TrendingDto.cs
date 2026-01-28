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

/// <summary>Versión sin RestauranteId para endpoints públicos por sesión.</summary>
public record TrendingPublicDto(
    int Minutos,
    DateTime Timestamp,
    List<ItemTrendingDto> Items
);
