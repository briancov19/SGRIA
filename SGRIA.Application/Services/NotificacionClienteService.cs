using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class NotificacionClienteService
{
    private readonly INotificacionClienteRepository _repo;
    private readonly IMesaRepository _mesaRepo;
    private readonly ISesionMesaRepository _sesionRepo;

    public NotificacionClienteService(
        INotificacionClienteRepository repo,
        IMesaRepository mesaRepo,
        ISesionMesaRepository sesionRepo)
    {
        _repo = repo;
        _mesaRepo = mesaRepo;
        _sesionRepo = sesionRepo;
    }

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
        if (string.IsNullOrWhiteSpace(dto.QrToken))
            throw new ArgumentException("QrToken requerido");

        // Buscar mesa por QR token
        var mesa = await _mesaRepo.GetByQrTokenAsync(dto.QrToken, ct);
        if (mesa == null)
            throw new ArgumentException($"Mesa no encontrada con QR token: {dto.QrToken}");

        if (!mesa.Activa)
            throw new InvalidOperationException("La mesa no está activa");

        // Asegurar que existe una sesión activa (crear/reutilizar si no existe)
        var sesionActiva = await _sesionRepo.GetActivaConActividadRecienteAsync(mesa.Id, 90, ct);
        
        if (sesionActiva == null)
        {
            // Crear nueva sesión si no existe una activa
            var nuevaSesion = new SesionMesa
            {
                MesaId = mesa.Id,
                Origen = "QR",
                FechaHoraInicio = DateTime.UtcNow
            };
            sesionActiva = await _sesionRepo.CreateAsync(nuevaSesion, ct);
        }
       
        var notificacionCliente = new NotificacionCliente
        {
            MesaId = mesa.Id,
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
