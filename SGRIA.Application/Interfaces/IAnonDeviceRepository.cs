using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface IAnonDeviceRepository
{
    Task<AnonDevice?> GetByDeviceHashAsync(string deviceHash, CancellationToken ct);
    Task<AnonDevice> CreateAsync(AnonDevice device, CancellationToken ct);
}
