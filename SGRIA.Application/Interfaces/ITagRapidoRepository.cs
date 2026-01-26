using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface ITagRapidoRepository
{
    Task<List<TagRapido>> GetAllAsync(CancellationToken ct);
    Task<List<TagRapido>> GetActivosAsync(CancellationToken ct);
    Task<TagRapido?> GetByIdAsync(int id, CancellationToken ct);
    Task<TagRapido> AddAsync(TagRapido tag, CancellationToken ct);
    Task<TagRapido> UpdateAsync(TagRapido tag, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
