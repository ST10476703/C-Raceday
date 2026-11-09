using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Services;

public sealed record ApiResult<T>(bool Success, T? Value, string Error, HttpStatusCode StatusCode)
{
    public static ApiResult<T> Failure(string error, HttpStatusCode status = HttpStatusCode.ServiceUnavailable)
        => new(false, default, error, status);
}

public sealed class RaceDayApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    public RaceDayApiClient(HttpClient http) => _http = http;

    public Task<ApiResult<T>> GetAsync<T>(string path, string? token = null) => SendAsync<T>(HttpMethod.Get, path, token);
    public Task<ApiResult<T>> PostAsync<T>(string path, object body, string? token = null) => SendAsync<T>(HttpMethod.Post, path, token, body);
    public Task<ApiResult<T>> PutAsync<T>(string path, object body, string? token = null) => SendAsync<T>(HttpMethod.Put, path, token, body);
    public Task<ApiResult<T>> DeleteAsync<T>(string path, string? token = null) => SendAsync<T>(HttpMethod.Delete, path, token);

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, string? token, object? body = null)
    {
        try
        {
            using var request = new HttpRequestMessage(method, path.TrimStart('/'));
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (body != null) request.Content = JsonContent.Create(body);
            using var response = await _http.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return ApiResult<T>.Failure(ExtractError(content, response.StatusCode), response.StatusCode);
            if (string.IsNullOrWhiteSpace(content)) return new(true, default, "", response.StatusCode);
            var result = JsonSerializer.Deserialize<T>(content, _json);
            return new(true, result, "", response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Failure("Cannot contact the RaceDay API. Start the API project and check Api:BaseUrl.");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("RaceDay API request timed out.");
        }
        catch (JsonException)
        {
            return ApiResult<T>.Failure("The API returned an unexpected response.");
        }
    }

    public async Task<ApiResult<T>> UploadAsync<T>(string path, IFormFile file, string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path.TrimStart('/'));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var multipart = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            multipart.Add(content, "file", Path.GetFileName(file.FileName));
            request.Content = multipart;
            using var response = await _http.SendAsync(request);
            string text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return ApiResult<T>.Failure(ExtractError(text, response.StatusCode), response.StatusCode);
            return new(true, JsonSerializer.Deserialize<T>(text, _json), "", response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Failure("Cannot reach the API for image upload.");
        }
        catch (TaskCanceledException) { return ApiResult<T>.Failure("Image upload timed out."); }
        catch (JsonException) { return ApiResult<T>.Failure("The image endpoint returned an unexpected response."); }
    }

    public async Task<(byte[] Bytes, string Mime)?> ImageAsync(string name)
    {
        try
        {
            using var response = await _http.GetAsync("api/uploads/" + Uri.EscapeDataString(name));
            if (!response.IsSuccessStatusCode) return null;
            var mime = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            return (await response.Content.ReadAsByteArrayAsync(), mime);
        }
        catch (HttpRequestException) { return null; }
        catch (TaskCanceledException) { return null; }
    }

    private static string ExtractError(string content, HttpStatusCode code)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("message", out var message)) return message.GetString() ?? "Request failed.";
            if (doc.RootElement.TryGetProperty("title", out var title)) return title.GetString() ?? "Request failed.";
        }
        catch (JsonException) { }
        return code switch
        {
            HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
            HttpStatusCode.Forbidden => "You do not have permission to perform this action.",
            HttpStatusCode.Conflict => "This action conflicts with an existing record.",
            _ => "API request failed (" + (int)code + ")."
        };
    }
}
