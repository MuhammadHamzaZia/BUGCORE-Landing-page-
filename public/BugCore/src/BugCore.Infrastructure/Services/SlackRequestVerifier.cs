using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BugCore.Infrastructure.Services;

public interface ISlackRequestVerifier
{
    Task<(bool IsValid, string ErrorMessage)> VerifyRequestAsync(HttpRequest request, string signingSecret);
}

public class SlackRequestVerifier : ISlackRequestVerifier
{
    private readonly ILogger<SlackRequestVerifier> _logger;

    public SlackRequestVerifier(ILogger<SlackRequestVerifier> logger)
    {
        _logger = logger;
    }

    public async Task<(bool IsValid, string ErrorMessage)> VerifyRequestAsync(HttpRequest request, string signingSecret)
    {
        if (string.IsNullOrWhiteSpace(signingSecret))
        {
            // If signing secret is not configured in environment/config, bypass or log warning
            _logger.LogWarning("Slack signing secret is not configured.");
            return (true, string.Empty);
        }

        // 1. Extract Headers
        if (!request.Headers.TryGetValue("X-Slack-Signature", out var signatureHeader) ||
            string.IsNullOrWhiteSpace(signatureHeader))
        {
            return (false, "Missing X-Slack-Signature header.");
        }

        if (!request.Headers.TryGetValue("X-Slack-Request-Timestamp", out var timestampHeader) ||
            !long.TryParse(timestampHeader, out var requestTimestamp))
        {
            return (false, "Missing or invalid X-Slack-Request-Timestamp header.");
        }

        // 2. Replay Attack Prevention (Reject if > 5 minutes / 300 seconds old)
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(currentTimestamp - requestTimestamp) > 300)
        {
            return (false, "Request timestamp is expired or out of allowed window (> 5 minutes).");
        }

        // 3. Read Body Stream without consuming it permanently
        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0; // Reset for controller parsing

        // 4. Construct Signature Base String
        var sigBaseString = $"v0:{requestTimestamp}:{body}";

        // 5. Compute HMAC-SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(sigBaseString));
        var computedSignature = "v0=" + Convert.ToHexString(computedHash).ToLowerInvariant();

        // 6. Constant-Time Verification to prevent timing attacks
        var computedBytes = Encoding.UTF8.GetBytes(computedSignature);
        var headerBytes = Encoding.UTF8.GetBytes(signatureHeader.ToString());

        if (computedBytes.Length != headerBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(computedBytes, headerBytes))
        {
            return (false, "Invalid X-Slack-Signature header.");
        }

        return (true, string.Empty);
    }
}
