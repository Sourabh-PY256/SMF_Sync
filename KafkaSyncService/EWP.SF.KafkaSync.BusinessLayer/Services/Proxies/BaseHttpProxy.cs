using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public abstract class BaseHttpProxy
{
    protected readonly HttpClient _httpClient;
    protected readonly string _baseUrl;
    private readonly IConfiguration _configuration;
    
    private string _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    protected BaseHttpProxy(HttpClient httpClient, IConfiguration configuration, string baseUrlKey = "ExternalSyncApiBaseUrl")
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = configuration[$"AppSettings:{baseUrlKey}"] ?? configuration[baseUrlKey] ?? "http://localhost:5000";
    }

    private async Task<string> GetAccessTokenAsync()
    {
        // Check if token is still valid (with 1 minute buffer)
        if (!string.IsNullOrEmpty(_cachedToken) && _tokenExpiry > DateTime.UtcNow.AddMinutes(1))
        {
            return _cachedToken;
        }

        var oauthSettings = _configuration.GetSection("AppSettings:OAuth2Settings");
        var tokenUrl = oauthSettings["TokenUrl"];
        var clientId = oauthSettings["ClientId"];
        var clientSecret = oauthSettings["ClientSecret"];
        var scope = oauthSettings["Scope"];

        if (string.IsNullOrEmpty(tokenUrl) || string.IsNullOrEmpty(clientId))
        {
            return null;
        }

        var dict = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "scope", scope }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(dict)
        };

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            
            _cachedToken = json["access_token"]?.ToString();
            int expiresIn = json["expires_in"]?.ToObject<int>() ?? 3600;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
            
            return _cachedToken;
        }

        return null;
    }

    protected async Task<T> PostAsync<T>(string endpoint, object data, bool authorize = false)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        
        if (authorize)
        {
            var token = await GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }

    protected async Task<T> GetAsync<T>(string endpoint, bool authorize = true)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        if (authorize)
        {
            var token = await GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
}
