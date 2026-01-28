using System.ComponentModel.DataAnnotations;

namespace SGRIA.Application.DTOs;

public record SenalRatingDto(
    int Id,
    int SenalPedidoId,
    short Puntaje, // -1, 0, 1
    DateTime FechaHora
);

public record SenalRatingCreateDto(
    [property: Required][property: Range(-1, 1)] short Puntaje // -1 (👎), 0 (😐), 1 (👍)
);
