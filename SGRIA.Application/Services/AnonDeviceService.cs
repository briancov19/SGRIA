using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

/// <summary>
/// Servicio para manejar dispositivos anónimos y calcular hashes de identificación.
/// </summary>
public class AnonDeviceService
{
    private readonly IAnonDeviceRepository _deviceRepo;
    private readonly string _serverSalt;

    public AnonDeviceService(
        IAnonDeviceRepository deviceRepo,
        IConfiguration configuration)
    {
        _deviceRepo = deviceRepo;
        _serverSalt = configuration["AntiAbuse:ServerSalt"] ?? "SGRIA-DEFAULT-SALT-CHANGE-IN-PRODUCTION";
    }

    /// <summary>
    /// Calcula el hash del dispositivo a partir del clientId y el salt del servidor.
    /// </summary>
    public string CalcularDeviceHash(string clientId)
    {
        var input = $"{clientId}:{_serverSalt}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Obtiene o crea un dispositivo anónimo basado en el clientId.
    /// </summary>
    public async Task<AnonDevice> GetOrCreateDeviceAsync(string clientId, CancellationToken ct)
    {
        var deviceHash = CalcularDeviceHash(clientId);
        
        var device = await _deviceRepo.GetByDeviceHashAsync(deviceHash, ct);
        if (device != null)
        {
            return device;
        }

        // Crear nuevo dispositivo
        var nuevoDevice = new AnonDevice
        {
            DeviceHash = deviceHash,
            FechaCreacion = DateTime.UtcNow
        };

        return await _deviceRepo.CreateAsync(nuevoDevice, ct);
    }
}
