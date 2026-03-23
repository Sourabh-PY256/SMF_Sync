using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public abstract class BaseHttpProxy
{
    protected readonly HttpClient _httpClient;
    protected readonly string _baseUrl;
    private readonly IConfiguration _configuration;
    private readonly IAuthenticationService _authService;

    protected BaseHttpProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService, string baseUrlKey = "ExternalSyncApiBaseUrl")
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _authService = authService;
        _baseUrl = configuration[$"AppSettings:{baseUrlKey}"] 
                  ?? configuration[baseUrlKey] 
                  ?? configuration["AppSettings:ExternalServiceUrl"] 
                  ?? configuration["ExternalServiceUrl"] 
                  ?? "http://localhost:5000";
    }

    private async Task<string> GetAccessTokenAsync()
    {
        return await _authService.GetAccessTokenAsync();
    }

    protected async Task<T> PostAsync<T>(string endpoint, object data, bool authorize = true)
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
