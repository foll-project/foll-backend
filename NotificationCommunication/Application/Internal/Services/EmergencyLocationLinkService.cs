using System.Security.Cryptography;
using System.Text;
using foll_backend.ExternalServices.Infrastructure.Configuration;
using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace foll_backend.NotificationCommunication.Application.Internal.Services;

public class EmergencyLocationLinkService : IEmergencyLocationLinkService
{
    private readonly IEmergencyLocationAccessLinkRepository _linkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationOptions _notificationOptions;

    public EmergencyLocationLinkService(
        IEmergencyLocationAccessLinkRepository linkRepository,
        IUnitOfWork unitOfWork,
        IOptions<NotificationOptions> notificationOptions)
    {
        _linkRepository = linkRepository;
        _unitOfWork = unitOfWork;
        _notificationOptions = notificationOptions.Value;
    }

    public async Task<EmergencyLocationLinkResult> CreateLinkAsync(
        Guid incidentKey,
        long patientId,
        long? deviceId,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        var token = CreateToken();
        var tokenHash = HashToken(token);
        var expirationMinutes = Math.Max(1, _notificationOptions.EmergencyLocationLinkExpirationMinutes);
        var expiresAt = NormalizeTimestamp(createdAt).AddMinutes(expirationMinutes);

        var link = new EmergencyLocationAccessLink(
            incidentKey,
            patientId,
            deviceId,
            tokenHash,
            expiresAt,
            createdAt);

        await _linkRepository.AddAsync(link);

        var publicBaseUrl = string.IsNullOrWhiteSpace(_notificationOptions.PublicBaseUrl)
            ? "http://localhost:5000"
            : _notificationOptions.PublicBaseUrl.Trim().TrimEnd('/');

        return new EmergencyLocationLinkResult(
            $"{publicBaseUrl}/api/emergency/public-location/{token}",
            expiresAt);
    }

    public async Task<EmergencyLocationLinkResolution?> ResolveAsync(
        string token,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var tokenHash = HashToken(token.Trim());
        var link = await _linkRepository.FindByTokenHashAsync(tokenHash);
        if (link is null || !link.IsActive(now)) return null;

        link.MarkAccessed(now);
        _linkRepository.Update(link);
        await _unitOfWork.CompleteAsync();

        return new EmergencyLocationLinkResolution(link);
    }

    private static string CreateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static DateTime NormalizeTimestamp(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
    }
}
