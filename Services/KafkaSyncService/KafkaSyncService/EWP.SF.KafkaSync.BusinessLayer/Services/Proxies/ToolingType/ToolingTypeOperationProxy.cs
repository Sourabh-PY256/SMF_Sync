using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class ToolingTypeOperationProxy : BaseHttpProxy, IToolOperation
{
    private readonly ILogger<ToolingTypeOperationProxy> _logger;

    public ToolingTypeOperationProxy(
        HttpClient httpClient, 
        IConfiguration configuration, 
        IAuthenticationService authService, 
        ILogger<ToolingTypeOperationProxy> logger)
        : base(httpClient, configuration, authService)
    {
        _logger = logger;
    }

    public async Task<List<ResponseData>> ListUpdateToolType(List<ToolTypeExternal> toolTypeList, List<ToolTypeExternal> toolTypeListOriginal, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
         string endpoint = $"ToolType/{Validate.ToString().ToLower()}/{Level}";

        var response = await PostAsync<List<ResponseData>>(endpoint, toolTypeList).ConfigureAwait(false);
        return response ?? new List<ResponseData>();
    }

    public List<ToolType> ListToolTypes(string ToolTypeCode, DateTime? DeltaDate = null)
    {
        throw new NotSupportedException("ListToolTypes is not supported through proxy.");
    }

    public List<Tool> ListTools(string ToolCode, DateTime? DeltaDate = null)
    {
        throw new NotSupportedException("ListTools is not supported through proxy.");
    }
}
