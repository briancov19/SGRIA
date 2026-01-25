using SGRIA.Domain.Entities;

namespace SGRIA.Application.DTOs;

public record NotificacionClienteDto(int Id, DateTime FechaCreacion, bool Atendida, int MesaId, int MesaNumero);
