using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class SesionMesaService
{
    private readonly ISesionMesaRepository _sesionRepo;
    private readonly IMesaRepository _mesaRepo;
    private readonly AnonDeviceService _deviceService;
    private readonly ISesionParticipanteRepository _participanteRepo;
    private readonly int _timeoutMinutos;

    public SesionMesaService(
        ISesionMesaRepository sesionRepo,
        IMesaRepository mesaRepo,
        AnonDeviceService deviceService,
        ISesionParticipanteRepository participanteRepo,
        int timeoutMinutos = 90)
    {
        _sesionRepo = sesionRepo;
        _mesaRepo = mesaRepo;
        _deviceService = deviceService;
        _participanteRepo = participanteRepo;
        _timeoutMinutos = timeoutMinutos;
    }

    public async Task<SesionMesaDto> CrearOReutilizarSesionAsync(
        string qrToken,
        string? clientId,
        SesionMesaCreateDto? dto,
        CancellationToken ct)
    {
        // Buscar mesa por QR token
        var mesa = await _mesaRepo.GetByQrTokenAsync(qrToken, ct);
        if (mesa == null)
        {
            throw new ArgumentException($"Mesa no encontrada con QR token: {qrToken}");
        }

        if (!mesa.Activa)
        {
            throw new InvalidOperationException("La mesa no está activa");
        }

        // Obtener o crear dispositivo anónimo
        AnonDevice? device = null;
        SesionParticipante? participante = null;
        
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            device = await _deviceService.GetOrCreateDeviceAsync(clientId, ct);
        }

        // Buscar sesión activa con actividad reciente (considerando timeout)
        var sesionActiva = await _sesionRepo.GetActivaConActividadRecienteAsync(
            mesa.Id,
            _timeoutMinutos,
            ct);

        SesionMesa sesionFinal;
        
        if (sesionActiva != null)
        {
            // Reutilizar sesión existente (tiene actividad reciente)
            sesionFinal = sesionActiva;
            
            // Asociar participante si existe
            if (device != null)
            {
                participante = await _participanteRepo.GetBySesionAndDeviceAsync(
                    sesionActiva.Id, 
                    device.Id, 
                    ct);
                
                if (participante == null)
                {
                    // Crear nuevo participante para esta sesión
                    participante = new SesionParticipante
                    {
                        SesionMesaId = sesionActiva.Id,
                        AnonDeviceId = device.Id,
                        FechaHoraJoin = DateTime.UtcNow,
                        UltimaActividad = DateTime.UtcNow
                    };
                    participante = await _participanteRepo.CreateAsync(participante, ct);
                }
                else
                {
                    // Actualizar última actividad
                    participante.UltimaActividad = DateTime.UtcNow;
                    participante = await _participanteRepo.UpdateAsync(participante, ct);
                }
            }
        }
        else
        {
            // Crear nueva sesión (no hay sesión activa o la existente expiró)
            var nuevaSesion = new SesionMesa
            {
                MesaId = mesa.Id,
                CantidadPersonas = dto?.CantidadPersonas,
                Origen = dto?.Origen ?? "QR",
                FechaHoraInicio = DateTime.UtcNow
            };

            sesionFinal = await _sesionRepo.CreateAsync(nuevaSesion, ct);
            
            // Asociar participante si existe
            if (device != null)
            {
                participante = new SesionParticipante
                {
                    SesionMesaId = sesionFinal.Id,
                    AnonDeviceId = device.Id,
                    FechaHoraJoin = DateTime.UtcNow,
                    UltimaActividad = DateTime.UtcNow
                };
                participante = await _participanteRepo.CreateAsync(participante, ct);
            }
        }

        return new SesionMesaDto(
            sesionFinal.Id,
            sesionFinal.MesaId,
            sesionFinal.FechaHoraInicio,
            sesionFinal.FechaHoraFin,
            sesionFinal.CantidadPersonas,
            sesionFinal.Origen
        );
    }

    public async Task<SesionMesaDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var sesion = await _sesionRepo.GetByIdAsync(id, ct);
        if (sesion == null) return null;

        return new SesionMesaDto(
            sesion.Id,
            sesion.MesaId,
            sesion.FechaHoraInicio,
            sesion.FechaHoraFin,
            sesion.CantidadPersonas,
            sesion.Origen
        );
    }
}
