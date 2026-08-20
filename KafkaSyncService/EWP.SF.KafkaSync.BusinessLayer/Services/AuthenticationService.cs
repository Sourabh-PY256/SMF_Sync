using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private const string CACHE_KEY = "SF_Access_Token";

    // Guards the cache-miss login call so concurrent callers await one in-flight login
    // instead of each firing a separate request at the ERP/SF login endpoint (cache stampede).
    private static readonly SemaphoreSlim _loginLock = new(1, 1);

    public static readonly string CryptoKey = Guid.NewGuid().ToString();

    public AuthenticationService(HttpClient httpClient, IConfiguration configuration, ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // Try to get from cache
        var token = MemoryCache.Default.Get(CACHE_KEY) as string;
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }

        await _loginLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Re-check: another caller may have already logged in while we were waiting for the lock.
            token = MemoryCache.Default.Get(CACHE_KEY) as string;
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            return await LoginAndCacheTokenAsync().ConfigureAwait(false);
        }
        finally
        {
            _loginLock.Release();
        }
    }

    private async Task<string> LoginAndCacheTokenAsync()
    {
        _logger.LogInformation("Token expired or missing. Attempting login to Smart Factory API...");

        // Perform login
        var authSettings = _configuration.GetSection("AppSettings:SmartFactoryAuth");
        if (!authSettings.Exists())
        {
            _logger.LogWarning("SmartFactoryAuth settings not found. Falling back to OAuth2Settings.");
            authSettings = _configuration.GetSection("AppSettings:OAuth2Settings");
        }

        var loginUrl = authSettings["LoginUrl"] ?? authSettings["TokenUrl"] ?? "https://localhost:7307/API/V1/User/Login/Angular";
        _logger.LogInformation("Resolved Login URL: {Url}", loginUrl);
        var clientId = authSettings["ClientId"];
        var clientSecret = authSettings["ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogError("Authentication failed: ClientId or ClientSecret is not configured in SmartFactoryAuth or OAuth2Settings.");
            return null;
        }

        var payload = new
        {
            GrantType = "client_credentials",
            ClientId = clientId,
            ClientSecret = clientSecret,
            CryptoKey = CryptoKey
        };

        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(loginUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);
                
                // Adjust property names based on the actual response structure of your Login API
                // Usually it returns 'token', 'access_token', or a nested object
                // Extract token from 'Authorization' or 'Data["Authorization"]' based on the provided response structure
                string accessToken = json["Authorization"]?.ToString() ?? 
                                     json["Data"]?["Authorization"]?.ToString() ??
                                     json["Token"]?.ToString() ?? 
                                     json["access_token"]?.ToString();
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Login successful but no token found in response. Body: {Body}", responseBody);
                    return null;
                }

                // Cache the token. Default expiration to 55 minutes if not specified (assuming 1h life)
                int expiresInSeconds = json["expires_in"]?.ToObject<int>() ?? 3300; 
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds)
                };

                MemoryCache.Default.Set(CACHE_KEY, accessToken, policy);
                _logger.LogInformation("Successfully authenticated. Token cached for {ExpiresIn} seconds.", expiresInSeconds);
                
                return accessToken;
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Authentication failed with status {Status}. Response: {Body}", response.StatusCode, errorBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication call to {Url}", loginUrl);
        }

        return null;
    }

    public void ClearToken()
    {
        MemoryCache.Default.Remove(CACHE_KEY);
        _logger.LogInformation("Authentication token cleared from cache.");
    }
}
