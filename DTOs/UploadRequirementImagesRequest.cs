using Microsoft.AspNetCore.Http;

namespace KhojMarket.Api.DTOs;

public class UploadRequirementImagesRequest
{
    public List<IFormFile> Images { get; set; } = new();
}