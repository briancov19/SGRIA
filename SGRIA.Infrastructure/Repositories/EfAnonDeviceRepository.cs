using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfAnonDeviceRepository : IAnonDeviceRepository
{
    private readonly AppDbContext _context;

    public EfAnonDeviceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AnonDevice?> GetByDeviceHashAsync(string deviceHash, CancellationToken ct)
    {
        return await _context.AnonDevices
            .FirstOrDefaultAsync(d => d.DeviceHash == deviceHash, ct);
    }

    public async Task<AnonDevice> CreateAsync(AnonDevice device, CancellationToken ct)
    {
        _context.AnonDevices.Add(device);
        await _context.SaveChangesAsync(ct);
        return device;
    }
}
