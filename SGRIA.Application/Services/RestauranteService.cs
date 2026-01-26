using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class RestauranteService
{
    private readonly IRestauranteRepository _repo;

    public RestauranteService(IRestauranteRepository repo) => _repo = repo;

    public async Task<List<RestauranteDto>> GetAllAsync(CancellationToken ct)
    {
        var items = await _repo.GetAllAsync(ct);
        return items.Select(r => new RestauranteDto(
            r.Id,
            r.Nombre,
            r.TimeZone,
            r.Activo,
            r.FechaCreacion
        )).ToList();
    }

    public async Task<RestauranteDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var r = await _repo.GetByIdAsync(id, ct);
        if (r == null) return null;

        return new RestauranteDto(
            r.Id,
            r.Nombre,
            r.TimeZone,
            r.Activo,
            r.FechaCreacion
        );
    }

    public async Task<RestauranteDto> CreateAsync(RestauranteCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("Nombre requerido");

        var restaurante = new Restaurante
        {
            Nombre = dto.Nombre.Trim(),
            TimeZone = dto.TimeZone ?? "America/Montevideo",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var saved = await _repo.AddAsync(restaurante, ct);
        return new RestauranteDto(
            saved.Id,
            saved.Nombre,
            saved.TimeZone,
            saved.Activo,
            saved.FechaCreacion
        );
    }

    public async Task<RestauranteDto?> UpdateAsync(int id, RestauranteUpdateDto dto, CancellationToken ct)
    {
        var restaurante = await _repo.GetByIdAsync(id, ct);
        if (restaurante == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            restaurante.Nombre = dto.Nombre.Trim();
        if (!string.IsNullOrWhiteSpace(dto.TimeZone))
            restaurante.TimeZone = dto.TimeZone;
        if (dto.Activo.HasValue)
            restaurante.Activo = dto.Activo.Value;

        var updated = await _repo.UpdateAsync(restaurante, ct);
        return new RestauranteDto(
            updated.Id,
            updated.Nombre,
            updated.TimeZone,
            updated.Activo,
            updated.FechaCreacion
        );
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        return await _repo.DeleteAsync(id, ct);
    }
}
