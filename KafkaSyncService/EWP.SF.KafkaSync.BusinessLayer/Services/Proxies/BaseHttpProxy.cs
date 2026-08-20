using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public abstract class BaseHttpProxy
{
    protected readonly HttpClient _httpClient;
    protected readonly string _baseUrl;

    protected readonly string _baseUrl2503;
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
    protected async Task<T> PostAsyncPO<T>(string endpoint, object data, bool authorize = true)
    {
        try
        {
            var baseUrl = _baseUrl;
            var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (authorize)
            {
                var token = await GetAccessTokenAsync();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new Exception("Authorization token is null or empty");
                }
                request.Headers.TryAddWithoutValidation("Authorization", token);
                request.Headers.TryAddWithoutValidation("CryptoKey", AuthenticationService.CryptoKey);
            }

            if (data is string jsonString)
            {
                request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }
            else
            {
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
            }

            var response = await _httpClient.SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP {(int)response.StatusCode} - URL: {url} - Body: {responseContent}");
            }

            return DeserializeResponse<T>(responseContent);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"HTTP Request failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new Exception("Request timed out", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error: {ex.Message}", ex);
        }
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
                request.Headers.TryAddWithoutValidation("CryptoKey", AuthenticationService.CryptoKey);
            }


        }

        if (data is string jsonString)
        {
            request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        }
        else
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception($"HTTP {(int)response.StatusCode} - URL: {url} - Body: {errorBody}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return DeserializeResponse<T>(responseContent);
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
                request.Headers.TryAddWithoutValidation("CryptoKey", AuthenticationService.CryptoKey);
            }
        }

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception($"HTTP {(int)response.StatusCode} - URL: {url} - Body: {errorBody}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return DeserializeResponse<T>(responseContent);
    }

    protected T DeserializeResponse<T>(string responseContent)
    {
        if (typeof(T) == typeof(ResponseModel))
        {
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        try
        {
            var jToken = JToken.Parse(responseContent);
            if (jToken is JObject jObject && (jObject.ContainsKey("IsSuccess") || jObject.ContainsKey("isSuccess")) && (jObject.ContainsKey("Data") || jObject.ContainsKey("data")))
            {
                var responseModel = jObject.ToObject<ResponseModel>();
                if (responseModel.IsSuccess)
                {
                    if (responseModel.Data != null)
                    {
                        var dataJson = JsonConvert.SerializeObject(responseModel.Data);
                        return JsonConvert.DeserializeObject<T>(dataJson);
                    }
                    return default(T);
                }
                else
                {
                    throw new Exception(responseModel.Message ?? "Microservice returned an error");
                }
            }
        }
        catch (JsonReaderException)
        {
            // Fallback for non-JSON or array strings
        }

        return JsonConvert.DeserializeObject<T>(responseContent);
    }
}
