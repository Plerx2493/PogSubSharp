using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace PogSubSharp.Webhook;

public class WebHookHandler
{
    private readonly ILogger _logger;

    public WebHookHandler(ILogger logger)
    {
        _logger = logger;
    }


    private async Task<bool> IsValidEventSubRequest(HttpRequest request)
    {
        try
        {
            if (!request.Headers.TryGetValue("Twitch-Eventsub-Message-Signature", out StringValues providedSignatureHeader))
                return false;

            string? providedSignatureString = providedSignatureHeader.FirstOrDefault()?.Split('=').ElementAtOrDefault(1);
            if (string.IsNullOrWhiteSpace(providedSignatureString))
                return false;

            byte[] providedSignature = BytesFromHex(providedSignatureString).ToArray();

            if (!request.Headers.TryGetValue("Twitch-Eventsub-Message-Id", out StringValues idHeader))
                return false;

            string? id = idHeader.First();

            if (!request.Headers.TryGetValue("Twitch-Eventsub-Message-Timestamp", out StringValues timestampHeader))
                return false;

            string? timestamp = timestampHeader.First();

            byte[] computedSignature =
                CalculateSignature(Encoding.UTF8.GetBytes(id + timestamp + await ReadRequestBodyContentAsync(request)));

            return computedSignature.Zip(providedSignature, (a, b) => a == b).Aggregate(true, (a, r) => a && r);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while calculating signature!");
            return false;
        }
    }

    private static Memory<byte> BytesFromHex(ReadOnlySpan<char> content)
    {
        if (content.IsEmpty || content.IsWhiteSpace())
        {
            return Memory<byte>.Empty;
        }

        try
        {
            Memory<byte> data = MemoryPool<byte>.Shared.Rent(content.Length / 2).Memory;
            int input = 0;
            for (int output = 0; output < data.Length; output++)
            {
                data.Span[output] = Convert.ToByte(new string(new[] {content[input++], content[input++]}), 16);
            }

            return input != content.Length ? Memory<byte>.Empty : data;
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException)
        {
            return Memory<byte>.Empty;
        }
    }

    private byte[] CalculateSignature(byte[] payload)
    {
        using HMACSHA256 hmac = new();
        return hmac.ComputeHash(payload);
    }

    private static async Task PrepareRequestBodyAsync(HttpRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (!request.Body.CanSeek)
        {
            request.EnableBuffering();
            await request.Body.DrainAsync(CancellationToken.None);
        }

        request.Body.Seek(0L, SeekOrigin.Begin);
    }

    private static async Task<string> ReadRequestBodyContentAsync(HttpRequest request)
    {
        await PrepareRequestBodyAsync(request);
        using StreamReader reader = new(request.Body, Encoding.UTF8, false, leaveOpen: true);
        string requestBody = await reader.ReadToEndAsync();
        request.Body.Seek(0L, SeekOrigin.Begin);

        return requestBody;
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, string contentType,
    string responseBody)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = contentType;
        await context.Response.WriteAsync(responseBody);
    }
}