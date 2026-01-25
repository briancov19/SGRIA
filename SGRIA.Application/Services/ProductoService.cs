using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class ProductoService
{
    private readonly IProductoRepository _repo;

    public ProductoService(IProductoRepository repo) => _repo = repo;

    public async Task<List<ProductoDto>> GetAllAsync(CancellationToken ct)
    {
        var items = await _repo.GetAllAsync(ct);
        return items.Select(p => new ProductoDto(p.Id, p.Nombre, p.Precio, p.CreatedAt)).ToList();
    }

    public async Task<ProductoDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : new ProductoDto(p.Id, p.Nombre, p.Precio, p.CreatedAt);
    }

    public async Task<ProductoDto> CreateAsync(ProductoCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("Nombre requerido");
        if (dto.Precio < 0)
            throw new ArgumentException("Precio inválido");

        var producto = new Producto
        {
            Nombre = dto.Nombre.Trim(),
            Precio = dto.Precio
        };

        var saved = await _repo.AddAsync(producto, ct);
        return new ProductoDto(saved.Id, saved.Nombre, saved.Precio, saved.CreatedAt);
    }
}
