using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Maraton.mvc.Services
{
    /// <summary>
    /// Result of an API call. Never throws for HTTP failures so controllers
    /// can turn them into friendly validation messages.
    /// </summary>
    public record ApiResult<T>(bool Success, int StatusCode, T? Data, string? Error)
    {
        public bool NotFound => StatusCode == 404;
        public bool Forbidden => StatusCode == 403;
    }

    /// <summary>
    /// Thin wrapper around the RaceDay API. Forwards the signed-in user's JWT
    /// (kept in session at login) as a bearer token and normalises JSON handling.
    /// </summary>
    public class RaceDayApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RaceDayApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<ApiResult<T>> GetAsync<T>(string path) =>
            SendAsync<T>(HttpMethod.Get, path, null);

        public Task<ApiResult<T>> PostAsync<T>(string path, object? body) =>
            SendAsync<T>(HttpMethod.Post, path, body);

        public Task<ApiResult<T>> PutAsync<T>(string path, object? body) =>
            SendAsync<T>(HttpMethod.Put, path, body);

        public Task<ApiResult<T>> PatchAsync<T>(string path, object? body) =>
            SendAsync<T>(HttpMethod.Patch, path, body);

        public Task<ApiResult<T>> DeleteAsync<T>(string path) =>
            SendAsync<T>(HttpMethod.Delete, path, null);

        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body)
        {
            AttachToken();

            try
            {
                using var request = new HttpRequestMessage(method, path);

                if (body is not null)
                {
                    request.Content = new StringContent(
                        JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
                }

                using var response = await _http.SendAsync(request);
                var payload = await response.Content.ReadAsStringAsync();
                var status = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                    return new ApiResult<T>(false, status, default, DescribeError(payload, status));

                if (typeof(T) == typeof(string))
                    return new ApiResult<T>(true, status, (T)(object)payload, null);

                var data = string.IsNullOrWhiteSpace(payload)
                    ? default
                    : JsonSerializer.Deserialize<T>(payload, JsonOptions);

                return new ApiResult<T>(true, status, data, null);
            }
            catch (Exception ex)
            {
                return new ApiResult<T>(false, 0, default, $"Could not reach the RaceDay API: {ex.Message}");
            }
        }

        /// <summary>
        /// The API answers with either plain text (explicit BadRequest/Conflict strings)
        /// or a ProblemDetails document (automatic model-validation failures).
        /// </summary>
        private static string DescribeError(string payload, int status)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return $"The request failed with status {status}.";

            if (!payload.TrimStart().StartsWith('{'))
                return payload.Trim();

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.EnumerateArray())
                        .Select(v => v.GetString())
                        .Where(m => !string.IsNullOrWhiteSpace(m))
                        .ToList();

                    if (messages.Count > 0)
                        return string.Join(" ", messages);
                }

                if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
                    return title.GetString()!;
            }
            catch (JsonException)
            {
                // Fall through and return the raw payload.
            }

            return payload.Trim();
        }
    }
}
