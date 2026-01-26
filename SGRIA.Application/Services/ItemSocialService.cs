using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;

namespace SGRIA.Application.Services;

public class ItemSocialService
{
    private readonly IItemSocialRepository _socialRepo;
    private readonly IItemMenuRepository _itemRepo;

    public ItemSocialService(
        IItemSocialRepository socialRepo,
        IItemMenuRepository itemRepo)
    {
        _socialRepo = socialRepo;
        _itemRepo = itemRepo;
    }

    public async Task<ItemSocialDto?> GetItemSocialAsync(
        int itemMenuId,
        int minutos = 30,
        int dias = 30,
        string periodo = "7d",
        CancellationToken ct = default)
    {
        // Validar que el item existe
        var item = await _itemRepo.GetByIdAsync(itemMenuId, ct);
        if (item == null)
            return null;

        // Calcular fechas del período
        var (fechaDesde, fechaHasta) = ParsePeriodo(periodo);
        fechaHasta = fechaHasta.AddDays(1).AddTicks(-1);

        return await _socialRepo.GetItemSocialAsync(
            itemMenuId,
            minutos,
            dias,
            fechaDesde,
            fechaHasta,
            ct);
    }

    private static (DateTime desde, DateTime hasta) ParsePeriodo(string periodo)
    {
        var ahora = DateTime.UtcNow;
        
        return periodo.ToLower() switch
        {
            "1d" or "1dia" or "today" => (ahora.Date, ahora),
            "7d" or "7dias" or "semana" => (ahora.AddDays(-7).Date, ahora),
            "30d" or "30dias" or "mes" => (ahora.AddDays(-30).Date, ahora),
            "90d" or "90dias" or "trimestre" => (ahora.AddDays(-90).Date, ahora),
            _ => throw new ArgumentException($"Período no válido: {periodo}. Use: 1d, 7d, 30d, 90d")
        };
    }
}
