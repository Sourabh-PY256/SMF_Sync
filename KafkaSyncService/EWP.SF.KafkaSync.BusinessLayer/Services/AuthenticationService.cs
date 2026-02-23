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

        _logger.LogInformation("Token expired or missing. Attempting login to Smart Factory API...");

        // Perform login
        var oauthSettings = _configuration.GetSection("AppSettings:SmartFactoryAuth");
        var loginUrl = oauthSettings["LoginUrl"] ?? "https://localhost:7307/API/V1/User/Login/Angular";
        var clientId = oauthSettings["ClientId"];
        var clientSecret = oauthSettings["ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogError("Authentication failed: ClientId or ClientSecret is not configured.");
            return null;
        }

        var payload = new
        {
            GrantType = "client_credentials",
            ClientId = clientId,
            ClientSecret = clientSecret
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
                string accessToken = json["Token"]?.ToString() ?? json["access_token"]?.ToString();
                
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
