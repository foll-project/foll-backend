namespace foll_backend.ExternalServices.Domain.Model;

public record PushNotificationResult
{
    public bool Success { get; init; }
    public string? ProviderMessageId { get; init; }
    public string? ErrorMessage { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public IReadOnlyCollection<string> InvalidTokens { get; init; }
    public IReadOnlyCollection<string> FailedTokens { get; init; }

    public PushNotificationResult(
        bool success,
        string? providerMessageId,
        string? errorMessage,
        int successCount = 0,
        int failureCount = 0,
        IReadOnlyCollection<string>? invalidTokens = null,
        IReadOnlyCollection<string>? failedTokens = null)
    {
        Success = success;
        ProviderMessageId = string.IsNullOrWhiteSpace(providerMessageId) ? null : providerMessageId.Trim();
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim();
        SuccessCount = Math.Max(0, successCount);
        FailureCount = Math.Max(0, failureCount);
        InvalidTokens = NormalizeTokens(invalidTokens);
        FailedTokens = NormalizeTokens(failedTokens);
    }

    public static PushNotificationResult Sent(string? providerMessageId, int successCount = 1, int failureCount = 0, IReadOnlyCollection<string>? failedTokens = null) =>
        new(true, providerMessageId, null, successCount, failureCount, null, failedTokens);

    public static PushNotificationResult Failed(string errorMessage, int failureCount = 0, IReadOnlyCollection<string>? invalidTokens = null, IReadOnlyCollection<string>? failedTokens = null) =>
        new(false, null, errorMessage, 0, failureCount, invalidTokens, failedTokens);

    private static IReadOnlyCollection<string> NormalizeTokens(IReadOnlyCollection<string>? tokens)
    {
        if (tokens is null || tokens.Count == 0) return Array.Empty<string>();

        return tokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Select(token => token.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
