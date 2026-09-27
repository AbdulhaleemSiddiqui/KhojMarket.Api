using System.Text.RegularExpressions;
using KhojMarket.Api.DTOs;

namespace KhojMarket.Api.Services;

public sealed class DevelopmentAiRequirementService : IAiRequirementService
{
    public Task<AiRequirementResponse> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        text = text.Trim();

        if (text.Length < 3)
        {
            throw new InvalidOperationException(
                "Requirement detail is too short.");
        }

        var lower = text.ToLowerInvariant();

        // Development-only fallback. It exists to exercise the complete
        // frontend/API/save flow without paid AI access. Production uses
        // OpenAiRequirementService.
        var isService =
            Regex.IsMatch(
                lower,
                @"\b(repair|service|installation|install|technician|contractor|cleaning|maintenance|karwana|lagwana)\b",
                RegexOptions.IgnoreCase);

        var isAc =
            Regex.IsMatch(
                lower,
                @"\b(ac|air\s*conditioner|air\s*conditioning)\b",
                RegexOptions.IgnoreCase);

        var category =
            isAc
                ? "Home Appliances"
                : isService
                    ? "Services"
                    : "General";

        var subCategory =
            isAc
                ? "Air Conditioners"
                : string.Empty;

        var title =
            isAc
                ? (isService
                    ? "AC Service / Installation Required"
                    : "Air Conditioner Required")
                : isService
                    ? "Service Required"
                    : "Marketplace Requirement";

        var fields =
            new List<AiRequirementFieldResponse>();

        fields.Add(new AiRequirementFieldResponse
        {
            Id = "requirementDetails",
            Label = "Requirement Details",
            Value = text,
            Required = true
        });

        if (isAc)
        {
            fields.Add(new AiRequirementFieldResponse
            {
                Id = "preferredCompany",
                Label = "Preferred Company / Brand",
                Value = ExtractBrand(lower),
                Required = false
            });

            fields.Add(new AiRequirementFieldResponse
            {
                Id = "quantity",
                Label = "Quantity",
                Value = ExtractQuantity(text),
                Required = true
            });

            fields.Add(new AiRequirementFieldResponse
            {
                Id = "roomSize",
                Label = "Room Size",
                Value = ExtractRoomSize(text),
                Required = false
            });

            fields.Add(new AiRequirementFieldResponse
            {
                Id = "capacity",
                Label = "AC Capacity",
                Value = string.Empty,
                Required = false
            });
        }

        fields.Add(new AiRequirementFieldResponse
        {
            Id = "country",
            Label = "Country",
            Value = ExtractCountry(lower),
            Required = true
        });

        fields.Add(new AiRequirementFieldResponse
        {
            Id = "city",
            Label = "City",
            Value = ExtractCity(lower),
            Required = true
        });

        fields.Add(new AiRequirementFieldResponse
        {
            Id = "budget",
            Label = "Budget / Range",
            Value = string.Empty,
            Required = false
        });

        return Task.FromResult(
            new AiRequirementResponse
            {
                Category = category,
                SubCategory = subCategory,
                Title = title,
                RequirementType = isService ? "service" : "product",
                Fields = fields
            });
    }

    private static string ExtractQuantity(string text)
    {
        var moreThan =
            Regex.Match(
                text,
                @"(?:more\s+than|above|over|greater\s+than|se\s+zyada|sy\s+zyada)\s*(\d+)",
                RegexOptions.IgnoreCase);

        if (moreThan.Success)
        {
            return $"More than {moreThan.Groups[1].Value}";
        }

        var quantity =
            Regex.Match(
                text,
                @"(?:quantity|qty)\s*(?:is|=|:)?\s*(\d+)",
                RegexOptions.IgnoreCase);

        return quantity.Success
            ? quantity.Groups[1].Value
            : string.Empty;
    }

    private static string ExtractRoomSize(string text)
    {
        var match =
            Regex.Match(
                text,
                @"\b(\d+(?:\.\d+)?)\s*[x×]\s*(\d+(?:\.\d+)?)\b",
                RegexOptions.IgnoreCase);

        return match.Success
            ? $"{match.Groups[1].Value}x{match.Groups[2].Value}"
            : string.Empty;
    }

    private static string ExtractBrand(string lower)
    {
        var brands = new[]
        {
            "daikin", "gree", "haier", "dawlance",
            "orient", "pel", "kenwood", "tcl"
        };

        return brands.FirstOrDefault(lower.Contains) ?? string.Empty;
    }

    private static string ExtractCountry(string lower)
    {
        return lower.Contains("pakistan")
            ? "Pakistan"
            : string.Empty;
    }

    private static string ExtractCity(string lower)
    {
        var cities = new[]
        {
            "Karachi", "Lahore", "Islamabad",
            "Rawalpindi", "Peshawar", "Quetta",
            "Multan", "Faisalabad"
        };

        return cities.FirstOrDefault(
                   city => lower.Contains(
                       city.ToLowerInvariant()))
               ?? string.Empty;
    }
}
