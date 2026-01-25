using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class NotificacionClienteService
{
    private readonly INotificacionClienteRepository _repo;

    public NotificacionClienteService(INotificacionClienteRepository repo) => _repo = repo;

    public async Task<NotificacionClienteDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : new NotificacionClienteDto(p.Id, p.FechaCreacion, p.Atendida, p.MesaId, p.Mesa.Numero);
    }

    public async Task<List<NotificacionClienteDto>> GetActivasAsync(int minutosCorte, CancellationToken ct)
    {
        var items = await _repo.GetActivasAsync(minutosCorte, ct);
        return items.Select(p => new NotificacionClienteDto(p.Id, p.FechaCreacion, p.Atendida, p.MesaId, p.Mesa.Numero)).ToList();
    }

    public async Task<NotificacionClienteDto> AddAsync(NotificacionClienteCreateDto dto, CancellationToken ct)
    {
        if (dto.MesaId == 0)
            throw new ArgumentException("Identificador de mesa requerido");
       
        var notificacionCliente = new NotificacionCliente
        {
            MesaId = dto.MesaId,
        };

        var saved = await _repo.AddAsync(notificacionCliente, ct);
        return new NotificacionClienteDto(saved.Id, saved.FechaCreacion, saved.Atendida, saved.MesaId, saved.Mesa.Numero);
    }

    public async Task<NotificacionClienteDto?> MarcarComoAtendidaAsync(int id, CancellationToken ct)
    {   
        // 1. Ejecutamos la actualización en el repositorio
        var exito = await _repo.MarcarComoAtendidaAsync(id, ct);
        
        if (!exito) return null;

        // 2. Obtenemos el registro actualizado para devolver el DTO
        var n = await _repo.GetByIdAsync(id, ct);
        
        return n is null ? null : new NotificacionClienteDto(
            n.Id, 
            n.FechaCreacion, 
            n.Atendida, 
            n.MesaId,
            n.Mesa.Numero
        );
    }
}
