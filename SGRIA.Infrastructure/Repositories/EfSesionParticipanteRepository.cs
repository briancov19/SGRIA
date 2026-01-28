using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfSesionParticipanteRepository : ISesionParticipanteRepository
{
    private readonly AppDbContext _context;

    public EfSesionParticipanteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SesionParticipante?> GetBySesionAndDeviceAsync(int sesionId, int anonDeviceId, CancellationToken ct)
    {
        return await _context.SesionParticipantes
            .FirstOrDefaultAsync(p => p.SesionMesaId == sesionId && p.AnonDeviceId == anonDeviceId, ct);
    }

    public async Task<SesionParticipante?> GetActivoBySesionAndDeviceHashAsync(int sesionId, string deviceHash, CancellationToken ct)
    {
        return await _context.SesionParticipantes
            .Include(p => p.AnonDevice)
            .Include(p => p.SesionMesa)
            .FirstOrDefaultAsync(p => 
                p.SesionMesaId == sesionId 
                && p.AnonDevice.DeviceHash == deviceHash
                && !p.SesionMesa.FechaHoraFin.HasValue, ct);
    }

    public async Task<SesionParticipante> CreateAsync(SesionParticipante participante, CancellationToken ct)
    {
        _context.SesionParticipantes.Add(participante);
        await _context.SaveChangesAsync(ct);
        return participante;
    }

    public async Task<SesionParticipante> UpdateAsync(SesionParticipante participante, CancellationToken ct)
    {
        _context.SesionParticipantes.Update(participante);
        await _context.SaveChangesAsync(ct);
        return participante;
    }

    public async Task<int> CountPedidosByParticipanteEnVentanaAsync(int sesionParticipanteId, int minutos, CancellationToken ct)
    {
        var participante = await _context.SesionParticipantes
            .FirstOrDefaultAsync(p => p.Id == sesionParticipanteId, ct);

        if (participante == null) return 0;

        var ventanaInicio = DateTime.UtcNow.AddMinutes(-minutos);
        
        // Contar pedidos de esta sesión creados en la ventana de tiempo
        // Nota: Por ahora contamos todos los pedidos de la sesión en la ventana
        // En el futuro podríamos asociar pedidos directamente con participantes
        return await _context.SenalesPedido
            .Where(p => p.SesionMesaId == participante.SesionMesaId 
                     && p.FechaHoraConfirmacion >= ventanaInicio)
            .CountAsync(ct);
    }

    public async Task<int> CountRatingsByParticipanteEnVentanaAsync(int sesionParticipanteId, int minutos, CancellationToken ct)
    {
        var participante = await _context.SesionParticipantes
            .FirstOrDefaultAsync(p => p.Id == sesionParticipanteId, ct);

        if (participante == null) return 0;

        var ventanaInicio = DateTime.UtcNow.AddMinutes(-minutos);
        
        // Contar ratings actualizados en la ventana de tiempo para esta sesión
        return await _context.SenalesRating
            .Where(r => r.SenalPedido.SesionMesaId == participante.SesionMesaId 
                     && r.FechaHora >= ventanaInicio)
            .CountAsync(ct);
    }

    public async Task<int> CountTagVotosByParticipanteEnVentanaAsync(int sesionParticipanteId, int minutos, CancellationToken ct)
    {
        var participante = await _context.SesionParticipantes
            .FirstOrDefaultAsync(p => p.Id == sesionParticipanteId, ct);

        if (participante == null) return 0;

        var ventanaInicio = DateTime.UtcNow.AddMinutes(-minutos);
        return await _context.VotosTagItemMenu
            .Where(v => v.SesionMesaId == participante.SesionMesaId && v.FechaHora >= ventanaInicio)
            .CountAsync(ct);
    }
}
