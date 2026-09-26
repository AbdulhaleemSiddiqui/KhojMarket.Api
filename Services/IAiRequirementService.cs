using KhojMarket.Api.DTOs;

namespace KhojMarket.Api.Services;

public interface IAiRequirementService
{
    Task<AiRequirementResponse> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default);
}
