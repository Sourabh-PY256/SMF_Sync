using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class MoveEntryOperationProxy : BaseHttpProxy, IMoveEntryOperation
{
    private readonly ILogger<MoveEntryOperationProxy> _logger;

    public MoveEntryOperationProxy(
        HttpClient httpClient, 
        IConfiguration configuration, 
        IAuthenticationService authService, 
        ILogger<MoveEntryOperationProxy> logger)
        : base(httpClient, configuration, authService)
    {
        _logger = logger;
    }

    public async Task<List<ResponseData>> ListUpdateMoveEntry(List<MoveEntryExternal> moveEntryList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        var request = new
        {
            MoveEntryList = moveEntryList,
            SystemOperator = systemOperator,
            Validate = Validate,
            Level = Level,
            LogId = logId
        };

        var response = await PostAsync<List<ResponseData>>("MoveEntry/Bulk", request).ConfigureAwait(false);
        return response ?? new List<ResponseData>();
    }
}
