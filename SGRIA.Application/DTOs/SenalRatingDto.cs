namespace SGRIA.Application.DTOs;

public record SenalRatingDto(
    int Id,
    int SenalPedidoId,
    short Puntaje, // -1, 0, 1
    DateTime FechaHora
);

public record SenalRatingCreateDto(
    short Puntaje // -1 (👎), 0 (😐), 1 (👍)
);
