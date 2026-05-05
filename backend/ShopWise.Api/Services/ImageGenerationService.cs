using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShopWise.Api.Services;

public class ImageGenerationException : Exception
{
    public int StatusCode { get; }

    public ImageGenerationException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

public record GeneratedImage(byte[] Bytes, string MimeType);

public class ImageGenerationService
{
    private const string Endpoint =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image:generateContent";

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<ImageGenerationService> _logger;

    public ImageGenerationService(HttpClient http, IConfiguration config, ILogger<ImageGenerationService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<GeneratedImage> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ImageGenerationException(500,
                "Gemini API key is not configured. Set Gemini__ApiKey (env) or Gemini:ApiKey (config).");

        var payload = new GeminiRequest(
            new[] { new GeminiContent(new[] { new GeminiPart(prompt) }) }
        );
        var json = JsonSerializer.Serialize(payload);

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{Endpoint}?key={apiKey}")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini API error {Status}: {Body}", (int)res.StatusCode, body);
            if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                || (int)res.StatusCode == 400 && body.Contains("API_KEY", StringComparison.OrdinalIgnoreCase))
            {
                throw new ImageGenerationException(401,
                    "Gemini API key is invalid or lacks permission for the image model.");
            }
            throw new ImageGenerationException(502,
                $"Gemini API request failed ({(int)res.StatusCode}): {Truncate(body, 300)}");
        }

        GeminiResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<GeminiResponse>(body);
        }
        catch (JsonException ex)
        {
            throw new ImageGenerationException(502, $"Could not parse Gemini response: {ex.Message}");
        }

        var inline = parsed?.Candidates?
            .SelectMany(c => c.Content?.Parts ?? Array.Empty<GeminiResponsePart>())
            .Select(p => p.InlineData)
            .FirstOrDefault(d => d is not null && !string.IsNullOrEmpty(d.Data));

        if (inline is null)
            throw new ImageGenerationException(502, "Gemini response did not contain an image.");

        return new GeneratedImage(Convert.FromBase64String(inline.Data!), inline.MimeType ?? "image/png");
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "...";

    private record GeminiRequest(
        [property: JsonPropertyName("contents")] GeminiContent[] Contents);

    private record GeminiContent(
        [property: JsonPropertyName("parts")] GeminiPart[] Parts);

    private record GeminiPart(
        [property: JsonPropertyName("text")] string Text);

    private record GeminiResponse(
        [property: JsonPropertyName("candidates")] GeminiCandidate[]? Candidates);

    private record GeminiCandidate(
        [property: JsonPropertyName("content")] GeminiResponseContent? Content);

    private record GeminiResponseContent(
        [property: JsonPropertyName("parts")] GeminiResponsePart[]? Parts);

    private record GeminiResponsePart(
        [property: JsonPropertyName("inlineData")] GeminiInlineData? InlineData);

    private record GeminiInlineData(
        [property: JsonPropertyName("mimeType")] string? MimeType,
        [property: JsonPropertyName("data")] string? Data);
}
