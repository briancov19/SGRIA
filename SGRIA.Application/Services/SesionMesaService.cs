using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class SesionMesaService
{
    private readonly ISesionMesaRepository _sesionRepo;
    private readonly IMesaRepository _mesaRepo;

    public SesionMesaService(ISesionMesaRepository sesionRepo, IMesaRepository mesaRepo)
    {
        _sesionRepo = sesionRepo;
        _mesaRepo = mesaRepo;
    }

    public async Task<SesionMesaDto> CrearOReutilizarSesionAsync(
        string qrToken,
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

        // Buscar sesión activa (sin fecha de fin)
        var sesionActiva = await _sesionRepo.GetActivaByMesaIdAsync(mesa.Id, ct);

        if (sesionActiva != null)
        {
            // Reutilizar sesión existente
            return new SesionMesaDto(
                sesionActiva.Id,
                sesionActiva.MesaId,
                sesionActiva.FechaHoraInicio,
                sesionActiva.FechaHoraFin,
                sesionActiva.CantidadPersonas,
                sesionActiva.Origen
            );
        }

        // Crear nueva sesión
        var nuevaSesion = new SesionMesa
        {
            MesaId = mesa.Id,
            CantidadPersonas = dto?.CantidadPersonas,
            Origen = dto?.Origen ?? "QR",
            FechaHoraInicio = DateTime.UtcNow
        };

        var sesionCreada = await _sesionRepo.CreateAsync(nuevaSesion, ct);

        return new SesionMesaDto(
            sesionCreada.Id,
            sesionCreada.MesaId,
            sesionCreada.FechaHoraInicio,
            sesionCreada.FechaHoraFin,
            sesionCreada.CantidadPersonas,
            sesionCreada.Origen
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
