using System.ComponentModel.DataAnnotations;

namespace SGRIA.Application.DTOs;

public record NotificacionClienteCreateDto(
    [property: Required][property: MinLength(1)][property: MaxLength(100)] string QrToken
);