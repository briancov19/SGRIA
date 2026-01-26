using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class TagRapidoService
{
    private readonly ITagRapidoRepository _repo;

    public TagRapidoService(ITagRapidoRepository repo) => _repo = repo;

    public async Task<List<TagRapidoDto>> GetAllAsync(bool soloActivos = false, CancellationToken ct = default)
    {
        var tags = soloActivos
            ? await _repo.GetActivosAsync(ct)
            : await _repo.GetAllAsync(ct);

        return tags.Select(t => new TagRapidoDto(
            t.Id,
            t.Nombre,
            t.Tipo,
            t.Activo
        )).ToList();
    }

    public async Task<TagRapidoDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var t = await _repo.GetByIdAsync(id, ct);
        if (t == null) return null;

        return new TagRapidoDto(
            t.Id,
            t.Nombre,
            t.Tipo,
            t.Activo
        );
    }

    public async Task<TagRapidoDto> CreateAsync(TagRapidoCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("Nombre requerido");

        var tag = new TagRapido
        {
            Nombre = dto.Nombre.Trim(),
            Tipo = dto.Tipo?.Trim(),
            Activo = true
        };

        var saved = await _repo.AddAsync(tag, ct);
        return new TagRapidoDto(
            saved.Id,
            saved.Nombre,
            saved.Tipo,
            saved.Activo
        );
    }

    public async Task<TagRapidoDto?> UpdateAsync(int id, TagRapidoUpdateDto dto, CancellationToken ct)
    {
        var tag = await _repo.GetByIdAsync(id, ct);
        if (tag == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            tag.Nombre = dto.Nombre.Trim();
        if (dto.Tipo != null)
            tag.Tipo = dto.Tipo.Trim();
        if (dto.Activo.HasValue)
            tag.Activo = dto.Activo.Value;

        var updated = await _repo.UpdateAsync(tag, ct);
        return new TagRapidoDto(
            updated.Id,
            updated.Nombre,
            updated.Tipo,
            updated.Activo
        );
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        return await _repo.DeleteAsync(id, ct);
    }
}
