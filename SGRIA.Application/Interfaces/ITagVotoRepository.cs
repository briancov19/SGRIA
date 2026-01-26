using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface ITagVotoRepository
{
    Task<VotoTagItemMenu?> GetBySesionItemTagAsync(int sesionId, int itemMenuId, int tagId, CancellationToken ct);
    Task<VotoTagItemMenu> CreateOrUpdateAsync(VotoTagItemMenu voto, CancellationToken ct);
    Task<List<VotoTagItemMenu>> GetByItemMenuIdAsync(int itemMenuId, CancellationToken ct);
}
