using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Services;
using System.Text.RegularExpressions;

namespace RaceDayMVC.Controllers;

public sealed class ImagesController : Controller
{
    private readonly RaceDayApiClient _api;
    public ImagesController(RaceDayApiClient api) => _api = api;

    // Azure container remains private; MVC proxies only images through the Part 2 API.
    [HttpGet]
    public async Task<IActionResult> Show(string? imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            return NotFound();
        string name = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));
        if (!Regex.IsMatch(name, @"^[a-f0-9]{32}\.(jpg|png|webp)$")) return NotFound();
        var image = await _api.ImageAsync(name);
        if (image is null) return NotFound();
        Response.Headers.CacheControl = "public,max-age=3600";
        return File(image.Value.Bytes, image.Value.Mime);
    }
}
