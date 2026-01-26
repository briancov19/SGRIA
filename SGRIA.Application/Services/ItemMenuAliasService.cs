using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class ItemMenuAliasService
{
    private readonly IItemMenuAliasRepository _aliasRepo;
    private readonly IItemMenuRepository _itemRepo;

    public ItemMenuAliasService(IItemMenuAliasRepository aliasRepo, IItemMenuRepository itemRepo)
    {
        _aliasRepo = aliasRepo;
        _itemRepo = itemRepo;
    }

    public async Task<List<ItemMenuAliasDto>> GetByItemMenuIdAsync(int itemMenuId, CancellationToken ct)
    {
        var aliases = await _aliasRepo.GetByItemMenuIdAsync(itemMenuId, ct);
        return aliases.Select(a => new ItemMenuAliasDto(
            a.Id,
            a.ItemMenuId,
            a.ItemMenu.Nombre,
            a.AliasTexto,
            a.Activo
        )).ToList();
    }

    public async Task<ItemMenuAliasDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var a = await _aliasRepo.GetByIdAsync(id, ct);
        if (a == null) return null;

        return new ItemMenuAliasDto(
            a.Id,
            a.ItemMenuId,
            a.ItemMenu.Nombre,
            a.AliasTexto,
            a.Activo
        );
    }

    public async Task<ItemMenuAliasDto> CreateAsync(ItemMenuAliasCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.AliasTexto))
            throw new ArgumentException("AliasTexto requerido");

        var itemMenu = await _itemRepo.GetByIdAsync(dto.ItemMenuId, ct);
        if (itemMenu == null)
            throw new ArgumentException($"Item de menú no encontrado: {dto.ItemMenuId}");

        var alias = new ItemMenuAlias
        {
            ItemMenuId = dto.ItemMenuId,
            AliasTexto = dto.AliasTexto.Trim(),
            Activo = true
        };

        var saved = await _aliasRepo.AddAsync(alias, ct);
        await _aliasRepo.LoadItemMenuAsync(saved, ct);

        return new ItemMenuAliasDto(
            saved.Id,
            saved.ItemMenuId,
            saved.ItemMenu.Nombre,
            saved.AliasTexto,
            saved.Activo
        );
    }

    public async Task<ItemMenuAliasDto?> UpdateAsync(int id, ItemMenuAliasUpdateDto dto, CancellationToken ct)
    {
        var alias = await _aliasRepo.GetByIdAsync(id, ct);
        if (alias == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.AliasTexto))
            alias.AliasTexto = dto.AliasTexto.Trim();
        if (dto.Activo.HasValue)
            alias.Activo = dto.Activo.Value;

        var updated = await _aliasRepo.UpdateAsync(alias, ct);
        await _aliasRepo.LoadItemMenuAsync(updated, ct);

        return new ItemMenuAliasDto(
            updated.Id,
            updated.ItemMenuId,
            updated.ItemMenu.Nombre,
            updated.AliasTexto,
            updated.Activo
        );
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        return await _aliasRepo.DeleteAsync(id, ct);
    }
}
