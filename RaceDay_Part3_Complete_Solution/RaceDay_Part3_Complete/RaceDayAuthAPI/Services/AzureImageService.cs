using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDayAuthAPI.Services;

/// <summary>All Azure access stays in the API. The MVC project only makes HTTP requests.</summary>
public sealed class AzureImageService
{
    private readonly IConfiguration _configuration;
    private const long MaxBytes = 5 * 1024 * 1024;

    public AzureImageService(IConfiguration configuration) => _configuration = configuration;

    private BlobContainerClient GetContainer()
    {
        var connectionString = _configuration["AzureStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Azure Blob Storage is not configured on the API.");
        string name = _configuration["AzureStorage:Container"] ?? "raceday-images";
        return new BlobServiceClient(connectionString).GetBlobContainerClient(name);
    }

    public static (string Extension, string Mime)? Inspect(IFormFile file)
    {
        if (file.Length is < 1 or > MaxBytes) return null;
        Span<byte> header = stackalloc byte[12];
        using var stream = file.OpenReadStream();
        if (stream.Read(header) < 12) return null;
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return ("jpg", "image/jpeg");
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return ("png", "image/png");
        if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            return ("webp", "image/webp");
        return null;
    }

    public async Task<(string AzureUrl, string BlobName)> UploadAsync(IFormFile file, CancellationToken ct)
    {
        var image = Inspect(file) ?? throw new ArgumentException("Choose a valid JPG, PNG or WebP image under 5 MB.");
        var container = GetContainer();
        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        string blobName = $"{Guid.NewGuid():N}.{image.Extension}";
        var blob = container.GetBlobClient(blobName);
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = image.Mime }
        }, ct);
        return (blob.Uri.AbsoluteUri, blobName);
    }

    public async Task<(byte[] Bytes, string Mime)?> DownloadAsync(string name, CancellationToken ct)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-f0-9]{32}\.(jpg|png|webp)$"))
            return null;
        var blob = GetContainer().GetBlobClient(name);
        if (!(await blob.ExistsAsync(ct)).Value) return null;
        var result = await blob.DownloadContentAsync(ct);
        return (result.Value.Content.ToArray(), result.Value.Details.ContentType ?? "application/octet-stream");
    }
}
