using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KhojMarket.Api.DTOs;

namespace KhojMarket.Api.Services;

public sealed class OpenAiRequirementService : IAiRequirementService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiRequirementService> _logger;

    public OpenAiRequirementService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAiRequirementService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiRequirementResponse> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        text = text.Trim();

        if (text.Length < 3)
        {
            throw new InvalidOperationException(
                "Requirement detail is too short.");
        }

        if (text.Length > 4000)
        {
            throw new InvalidOperationException(
                "Requirement detail is too long.");
        }

        var apiKey =
            _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured.");
        }

        var model =
            _configuration["OpenAI:Model"]
            ?? "gpt-5.6-luna";

        var payload = new
        {
            model,
            instructions = """
You convert a buyer's marketplace request into a clean editable requirement form for KhojMarket in Pakistan.

Rules:
- Understand natural English, Urdu and Roman Urdu. Do not use keyword matching.
- Never invent facts the buyer did not provide. Unknown values must be empty strings.
- Create a concise useful title based on the actual request, not generic titles such as "Requirement".
- requirementType must be exactly "product" or "service".
- Choose a practical category and optional subCategory.
- fields must contain useful category-specific details from the request.
- Always include Country and City fields. Use field ids "country" and "city". They are required, but leave their values empty if not stated.
- Include "area" only when supplied or useful.
- Include a field with id "budget" and label "Budget / Range"; it is optional unless the buyer explicitly makes budget essential.
- Preserve constraints exactly. Examples: quantity "more than 40" must not become "40"; room size "12x12" must remain 12x12.
- For product requests, useful fields can include quantity, brand/company preference, model, size/capacity, room size, condition, specifications, delivery details, etc.
- For service requests, useful fields can include work details, scope, quantity/area, preferred date, timeline, location details, etc.
- Mark a field required only when it is essential to submit a meaningful request.
- Use stable camelCase field ids containing letters/numbers only.
- Do not add image fields.
- Return only data matching the supplied JSON schema.
""",
            input = text,
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "khojmarket_requirement",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            category = new { type = "string" },
                            subCategory = new { type = "string" },
                            title = new { type = "string" },
                            requirementType = new
                            {
                                type = "string",
                                @enum = new[] { "product", "service" }
                            },
                            fields = new
                            {
                                type = "array",
                                minItems = 3,
                                maxItems = 16,
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    properties = new
                                    {
                                        id = new { type = "string" },
                                        label = new { type = "string" },
                                        value = new { type = "string" },
                                        required = new { type = "boolean" }
                                    },
                                    required = new[]
                                    {
                                        "id", "label", "value", "required"
                                    }
                                }
                            }
                        },
                        required = new[]
                        {
                            "category",
                            "subCategory",
                            "title",
                            "requirementType",
                            "fields"
                        }
                    }
                }
            }
        };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/responses");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);

        request.Content =
            new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        var responseJson =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenAI requirement generation failed with {StatusCode}: {Response}",
                (int)response.StatusCode,
                responseJson);

            throw new InvalidOperationException(
                "AI requirement generation is temporarily unavailable.");
        }

        using var document =
            JsonDocument.Parse(responseJson);

        var outputText =
            document.RootElement
                .GetProperty("output")
                .EnumerateArray()
                .SelectMany(item =>
                    item.TryGetProperty("content", out var content)
                        ? content.EnumerateArray()
                        : Enumerable.Empty<JsonElement>())
                .FirstOrDefault(item =>
                    item.TryGetProperty("type", out var type) &&
                    type.GetString() == "output_text");

        if (outputText.ValueKind == JsonValueKind.Undefined ||
            !outputText.TryGetProperty("text", out var textElement))
        {
            throw new InvalidOperationException(
                "AI returned an empty requirement.");
        }

        var result =
            JsonSerializer.Deserialize<AiRequirementResponse>(
                textElement.GetString() ?? string.Empty,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? throw new InvalidOperationException(
                "AI returned an invalid requirement.");

        NormalizeAndValidate(result);

        return result;
    }

    private static void NormalizeAndValidate(
        AiRequirementResponse result)
    {
        result.Title = result.Title.Trim();
        result.Category = result.Category.Trim();
        result.SubCategory = result.SubCategory?.Trim();

        result.RequirementType =
            result.RequirementType.Equals(
                "service",
                StringComparison.OrdinalIgnoreCase)
                ? "service"
                : "product";

        result.Fields = result.Fields
            .Where(field =>
                !string.IsNullOrWhiteSpace(field.Id) &&
                !string.IsNullOrWhiteSpace(field.Label))
            .GroupBy(
                field => NormalizeId(field.Id),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(16)
            .ToList();

        foreach (var field in result.Fields)
        {
            field.Id = NormalizeId(field.Id);
            field.Label = field.Label.Trim();
            field.Value = field.Value?.Trim() ?? string.Empty;
        }

        EnsureLocationField(
            result.Fields,
            "country",
            "Country");

        EnsureLocationField(
            result.Fields,
            "city",
            "City");

        if (!result.Fields.Any(field =>
                field.Id.Equals(
                    "budget",
                    StringComparison.OrdinalIgnoreCase)))
        {
            result.Fields.Add(
                new AiRequirementFieldResponse
                {
                    Id = "budget",
                    Label = "Budget / Range",
                    Value = string.Empty,
                    Required = false
                });
        }

        if (string.IsNullOrWhiteSpace(result.Title))
        {
            throw new InvalidOperationException(
                "AI could not create a requirement title.");
        }
    }

    private static void EnsureLocationField(
        List<AiRequirementFieldResponse> fields,
        string id,
        string label)
    {
        var existing =
            fields.FirstOrDefault(field =>
                field.Id.Equals(
                    id,
                    StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            existing.Id = id;
            existing.Label = label;
            existing.Required = true;
            return;
        }

        fields.Add(
            new AiRequirementFieldResponse
            {
                Id = id,
                Label = label,
                Value = string.Empty,
                Required = true
            });
    }

    private static string NormalizeId(string value)
    {
        var parts =
            value
                .Trim()
                .Split(
                    new[] { ' ', '-', '_', '/', '.' },
                    StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return "detail";
        }

        return char.ToLowerInvariant(parts[0][0]) +
               parts[0][1..] +
               string.Concat(
                   parts.Skip(1).Select(part =>
                       char.ToUpperInvariant(part[0]) +
                       part[1..]));
    }
}
