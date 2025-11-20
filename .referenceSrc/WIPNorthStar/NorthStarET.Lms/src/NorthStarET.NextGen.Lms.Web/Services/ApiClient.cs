using System.Net.Http.Json;
using System.Text.Json;

namespace NorthStarET.NextGen.Lms.Web.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest content, CancellationToken cancellationToken = default);
    Task<bool> PostAsync<TRequest>(string requestUri, TRequest? content, CancellationToken cancellationToken = default);
    Task<bool> PutAsync<TRequest>(string requestUri, TRequest content, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(requestUri, _jsonOptions, cancellationToken);
        }
        catch
        {
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, content, _jsonOptions, cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken)
                : default;
        }
        catch
        {
            return default;
        }
    }

    public async Task<bool> PostAsync<TRequest>(string requestUri, TRequest? content, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = content != null
                ? await _httpClient.PostAsJsonAsync(requestUri, content, _jsonOptions, cancellationToken)
                : await _httpClient.PostAsync(requestUri, new ByteArrayContent(Array.Empty<byte>()), cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> PutAsync<TRequest>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(requestUri, content, _jsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
