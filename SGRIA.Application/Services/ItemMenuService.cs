using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class ItemMenuService
{
    private readonly IItemMenuRepository _itemRepo;
    private readonly IRestauranteRepository _restauranteRepo;

    public ItemMenuService(IItemMenuRepository itemRepo, IRestauranteRepository restauranteRepo)
    {
        _itemRepo = itemRepo;
        _restauranteRepo = restauranteRepo;
    }

    public async Task<List<ItemMenuDto>> GetByRestauranteIdAsync(int restauranteId, bool soloActivos = true, CancellationToken ct = default)
    {
        var items = soloActivos
            ? await _itemRepo.GetActivosByRestauranteIdAsync(restauranteId, ct)
            : await _itemRepo.GetAllByRestauranteIdAsync(restauranteId, ct);

        return items.Select(i => new ItemMenuDto(
            i.Id,
            i.RestauranteId,
            i.Restaurante.Nombre,
            i.Nombre,
            i.Descripcion,
            i.Categoria,
            i.Precio,
            i.Activo,
            i.ImagenUrl
        )).ToList();
    }

    public async Task<ItemMenuDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var i = await _itemRepo.GetByIdAsync(id, ct);
        if (i == null) return null;

        return new ItemMenuDto(
            i.Id,
            i.RestauranteId,
            i.Restaurante.Nombre,
            i.Nombre,
            i.Descripcion,
            i.Categoria,
            i.Precio,
            i.Activo,
            i.ImagenUrl
        );
    }

    public async Task<ItemMenuDto> CreateAsync(ItemMenuCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("Nombre requerido");

        var restaurante = await _restauranteRepo.GetByIdAsync(dto.RestauranteId, ct);
        if (restaurante == null)
            throw new ArgumentException($"Restaurante no encontrado: {dto.RestauranteId}");

        if (dto.Precio.HasValue && dto.Precio.Value < 0)
            throw new ArgumentException("El precio no puede ser negativo");

        var item = new ItemMenu
        {
            RestauranteId = dto.RestauranteId,
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Categoria = dto.Categoria?.Trim(),
            Precio = dto.Precio,
            Activo = true,
            ImagenUrl = dto.ImagenUrl?.Trim()
        };

        var saved = await _itemRepo.AddAsync(item, ct);
        await _itemRepo.LoadRestauranteAsync(saved, ct);

        return new ItemMenuDto(
            saved.Id,
            saved.RestauranteId,
            saved.Restaurante.Nombre,
            saved.Nombre,
            saved.Descripcion,
            saved.Categoria,
            saved.Precio,
            saved.Activo,
            saved.ImagenUrl
        );
    }

    public async Task<ItemMenuDto?> UpdateAsync(int id, ItemMenuUpdateDto dto, CancellationToken ct)
    {
        var item = await _itemRepo.GetByIdAsync(id, ct);
        if (item == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            item.Nombre = dto.Nombre.Trim();
        if (dto.Descripcion != null)
            item.Descripcion = dto.Descripcion.Trim();
        if (dto.Categoria != null)
            item.Categoria = dto.Categoria.Trim();
        if (dto.Precio.HasValue)
        {
            if (dto.Precio.Value < 0)
                throw new ArgumentException("El precio no puede ser negativo");
            item.Precio = dto.Precio.Value;
        }
        if (dto.Activo.HasValue)
            item.Activo = dto.Activo.Value;
        if (dto.ImagenUrl != null)
            item.ImagenUrl = dto.ImagenUrl.Trim();

        var updated = await _itemRepo.UpdateAsync(item, ct);
        await _itemRepo.LoadRestauranteAsync(updated, ct);

        return new ItemMenuDto(
            updated.Id,
            updated.RestauranteId,
            updated.Restaurante.Nombre,
            updated.Nombre,
            updated.Descripcion,
            updated.Categoria,
            updated.Precio,
            updated.Activo,
            updated.ImagenUrl
        );
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        return await _itemRepo.DeleteAsync(id, ct);
    }
}
