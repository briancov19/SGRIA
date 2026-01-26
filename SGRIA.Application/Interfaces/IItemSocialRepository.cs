using SGRIA.Application.DTOs;

namespace SGRIA.Application.Interfaces;

public interface IItemSocialRepository
{
    Task<ItemSocialDto?> GetItemSocialAsync(
        int itemMenuId,
        int minutos,
        int dias,
        DateTime fechaDesdePeriodo,
        DateTime fechaHastaPeriodo,
        CancellationToken ct = default);
}
