using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class MesaService
{
    private readonly IMesaRepository _mesaRepo;
    private readonly IRestauranteRepository _restauranteRepo;

    public MesaService(IMesaRepository mesaRepo, IRestauranteRepository restauranteRepo)
    {
        _mesaRepo = mesaRepo;
        _restauranteRepo = restauranteRepo;
    }

    public async Task<List<MesaDto>> GetAllAsync(CancellationToken ct)
    {
        var items = await _mesaRepo.GetAllAsync(ct);
        return items.Select(m => new MesaDto(
            m.Id,
            m.RestauranteId,
            m.Restaurante.Nombre,
            m.Numero,
            m.CantidadSillas,
            m.QrToken,
            m.Activa,
            m.FechaModificacion
        )).ToList();
    }

    public async Task<MesaDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var m = await _mesaRepo.GetByIdAsync(id, ct);
        if (m == null) return null;
        
        return new MesaDto(
            m.Id,
            m.RestauranteId,
            m.Restaurante.Nombre,
            m.Numero,
            m.CantidadSillas,
            m.QrToken,
            m.Activa,
            m.FechaModificacion
        );
    }

    public async Task<MesaDto> CreateAsync(MesaCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.QrToken))
            throw new ArgumentException("QrToken requerido");
        if (dto.Numero <= 0)
            throw new ArgumentException("Número debe ser mayor a 0");
        if (dto.CantidadSillas <= 0)
            throw new ArgumentException("Cantidad de sillas debe ser mayor a 0");

        // Validar que el restaurante existe
        var restaurante = await _restauranteRepo.GetByIdAsync(dto.RestauranteId, ct);
        if (restaurante == null)
            throw new ArgumentException($"Restaurante no encontrado: {dto.RestauranteId}");

        // Validar que el QR token sea único
        var mesaExistente = await _mesaRepo.GetByQrTokenAsync(dto.QrToken, ct);
        if (mesaExistente != null)
            throw new ArgumentException($"Ya existe una mesa con el QR token: {dto.QrToken}");

        var mesa = new Mesa
        {
            RestauranteId = dto.RestauranteId,
            Numero = dto.Numero,
            CantidadSillas = dto.CantidadSillas,
            QrToken = dto.QrToken,
            Activa = true,
            FechaModificacion = DateTime.UtcNow
        };

        var saved = await _mesaRepo.AddAsync(mesa, ct);
        await _mesaRepo.LoadRestauranteAsync(saved, ct);
        
        return new MesaDto(
            saved.Id,
            saved.RestauranteId,
            saved.Restaurante.Nombre,
            saved.Numero,
            saved.CantidadSillas,
            saved.QrToken,
            saved.Activa,
            saved.FechaModificacion
        );
    }

    public async Task<MesaDto?> UpdateAsync(int id, MesaUpdateDto dto, CancellationToken ct)
    {
        var mesa = await _mesaRepo.GetByIdAsync(id, ct);
        if (mesa == null) return null;

        if (dto.Numero.HasValue && dto.Numero.Value <= 0)
            throw new ArgumentException("Número debe ser mayor a 0");
        if (dto.CantidadSillas.HasValue && dto.CantidadSillas.Value <= 0)
            throw new ArgumentException("Cantidad de sillas debe ser mayor a 0");

        if (dto.Numero.HasValue)
            mesa.Numero = dto.Numero.Value;
        if (dto.CantidadSillas.HasValue)
            mesa.CantidadSillas = dto.CantidadSillas.Value;
        if (dto.Activa.HasValue)
            mesa.Activa = dto.Activa.Value;
        if (!string.IsNullOrWhiteSpace(dto.QrToken))
        {
            // Validar que el nuevo QR token sea único (si es diferente)
            if (mesa.QrToken != dto.QrToken)
            {
                var mesaExistente = await _mesaRepo.GetByQrTokenAsync(dto.QrToken, ct);
                if (mesaExistente != null && mesaExistente.Id != id)
                    throw new ArgumentException($"Ya existe una mesa con el QR token: {dto.QrToken}");
                mesa.QrToken = dto.QrToken;
            }
        }

        mesa.FechaModificacion = DateTime.UtcNow;
        var updated = await _mesaRepo.UpdateAsync(mesa, ct);
        await _mesaRepo.LoadRestauranteAsync(updated, ct);

        return new MesaDto(
            updated.Id,
            updated.RestauranteId,
            updated.Restaurante.Nombre,
            updated.Numero,
            updated.CantidadSillas,
            updated.QrToken,
            updated.Activa,
            updated.FechaModificacion
        );
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var mesa = await _mesaRepo.GetByIdAsync(id, ct);
        if (mesa == null) return false;

        return await _mesaRepo.DeleteAsync(id, ct);
    }
}
