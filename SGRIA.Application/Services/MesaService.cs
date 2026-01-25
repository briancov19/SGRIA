using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class MesaService
{
    private readonly IMesaRepository _repo;

    public MesaService(IMesaRepository repo) => _repo = repo;

    public async Task<List<MesaDto>> GetAllAsync(CancellationToken ct)
    {
        var items = await _repo.GetAllAsync(ct);
        return items.Select(p => new MesaDto(p.Id, p.Numero, p.CantidadSillas, p.FechaModificacion)).ToList();
    }

    public async Task<MesaDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : new MesaDto(p.Id, p.Numero, p.CantidadSillas, p.FechaModificacion);
    }

    public async Task<MesaDto> CreateAsync(MesaCreateDto dto, CancellationToken ct)
    {
        if (dto.Numero == 0)
            throw new ArgumentException("Número requerido");
        if (dto.CantidadSillas == 0)
            throw new ArgumentException("Cantidad de sillas requeridas");

        var mesa = new Mesa
        {
            Numero = dto.Numero,
            CantidadSillas = dto.CantidadSillas
        };

        var saved = await _repo.AddAsync(mesa, ct);
        return new MesaDto(saved.Id, saved.Numero, saved.CantidadSillas, saved.FechaModificacion);
    }
}
