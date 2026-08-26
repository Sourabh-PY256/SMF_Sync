using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessEntities;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class StockAllocationProxy : BaseHttpProxy, IStockAllocationOperation
{
    private readonly bool _use2503ForSync;
    public StockAllocationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
        _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
    }

    public ResponseData ListUpdateAllocationBulk(StockAllocationExternal[] stockList, User systemOperator, bool Validate, LevelMessage Level, bool nodelete = false)
    {
        string endpoint = $"StockAllocation/Bulk/{Validate.ToString().ToLower()}/{Level}";
        return PostAsync<ResponseData>(endpoint, stockList).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
