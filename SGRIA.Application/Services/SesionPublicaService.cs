using Microsoft.Extensions.Configuration;
using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

/// <summary>
/// Servicio centralizado para manejar sesiones públicas usando tokens públicos.
/// Evita enumeración de IDs internos y centraliza la lógica de validación de sesiones.
/// </summary>
public class SesionPublicaService
{
    private readonly ISesionMesaRepository _sesionRepo;
    private readonly IMesaRepository _mesaRepo;
    private readonly AnonDeviceService _deviceService;
    private readonly ISesionParticipanteRepository _participanteRepo;
    private readonly int _timeoutMinutos;

    public SesionPublicaService(
        ISesionMesaRepository sesionRepo,
        IMesaRepository mesaRepo,
        AnonDeviceService deviceService,
        ISesionParticipanteRepository participanteRepo,
        IConfiguration configuration)
    {
        _sesionRepo = sesionRepo;
        _mesaRepo = mesaRepo;
        _deviceService = deviceService;
        _participanteRepo = participanteRepo;
        _timeoutMinutos = int.Parse(configuration["Session:TimeoutMinutes"] ?? "90");
    }

    /// <summary>
    /// Obtiene o crea una sesión desde un QR token. Devuelve el token público.
    /// </summary>
    public async Task<SesionPublicaDto> GetOrCreateSessionByQrTokenAsync(
        string qrToken,
        string? clientId,
        SesionMesaCreateDto? request,
        CancellationToken ct)
    {
        // Buscar mesa por QR token
        var mesa = await _mesaRepo.GetByQrTokenAsync(qrToken, ct);
        if (mesa == null)
        {
            throw new ArgumentException("Mesa no encontrada con el QR token proporcionado.");
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
            
            // Actualizar última actividad de la sesión
            sesionFinal.FechaHoraUltActividad = DateTime.UtcNow;
            await _sesionRepo.UpdateAsync(sesionFinal, ct);
            
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
                    // Actualizar última actividad del participante
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
                SesPublicToken = Guid.NewGuid().ToString(), // Generar token único
                CantidadPersonas = request?.CantidadPersonas,
                Origen = request?.Origen ?? "QR",
                FechaHoraInicio = DateTime.UtcNow,
                FechaHoraUltActividad = DateTime.UtcNow
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

        return new SesionPublicaDto(
            sesionFinal.SesPublicToken,
            sesionFinal.FechaHoraInicio,
            sesionFinal.FechaHoraFin,
            sesionFinal.CantidadPersonas,
            sesionFinal.Origen
        );
    }

    /// <summary>
    /// Obtiene una sesión activa por su token público. Valida timeout.
    /// </summary>
    public async Task<SesionMesa> GetActiveSessionByPublicTokenAsync(
        string sesPublicToken,
        CancellationToken ct)
    {
        var sesion = await _sesionRepo.GetByPublicTokenAsync(sesPublicToken, ct);
        if (sesion == null)
        {
            throw new ArgumentException("Sesión no encontrada con el token proporcionado.");
        }

        // Validar que la sesión esté activa
        if (sesion.FechaHoraFin.HasValue)
        {
            throw new InvalidOperationException("La sesión ya está cerrada");
        }

        // Validar timeout
        var ahora = DateTime.UtcNow;
        var fechaLimite = ahora.AddMinutes(-_timeoutMinutos);
        
        if (sesion.FechaHoraUltActividad < fechaLimite)
        {
            // Sesión expirada, cerrarla
            sesion.FechaHoraFin = sesion.FechaHoraUltActividad.AddMinutes(_timeoutMinutos);
            await _sesionRepo.UpdateAsync(sesion, ct);
            throw new InvalidOperationException("Sesión expirada. Por favor, re-escanea el QR.");
        }

        return sesion;
    }

    /// <summary>
    /// Actualiza la última actividad de una sesión (touch).
    /// </summary>
    public async Task TouchSessionAsync(int sesionId, CancellationToken ct)
    {
        var sesion = await _sesionRepo.GetByIdAsync(sesionId, ct);
        if (sesion == null)
        {
            throw new ArgumentException("Sesión no encontrada.");
        }

        sesion.FechaHoraUltActividad = DateTime.UtcNow;
        await _sesionRepo.UpdateAsync(sesion, ct);
    }
}
