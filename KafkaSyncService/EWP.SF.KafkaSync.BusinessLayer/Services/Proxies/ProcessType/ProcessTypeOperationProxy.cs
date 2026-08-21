using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Newtonsoft.Json;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class ProcessTypeOperationProxy : BaseHttpProxy, IProcessTypeOperation
{
    private readonly bool _use2503ForSync;

    public ProcessTypeOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
        _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
    }

    public async Task<List<ResponseData>> ListUpdateSuboperationTypes_Bulk(
        List<SubProcessTypeExternal> clockList, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        string endpoint = $"ProcessType/{Validate.ToString().ToLower()}/{Level}";
        
        ResponseModel response;
        if (_use2503ForSync)
        {
            response = await PostAsyncPO<ResponseModel>(endpoint, clockList).ConfigureAwait(false);
        }
        else
        {
            response = await PostAsync<ResponseModel>(endpoint, clockList).ConfigureAwait(false);
        }

        if (response.IsSuccess)
        {
            if (response.Data is IEnumerable<ResponseData> dataList)
            {
                return dataList.ToList();
            }
            else if (response.Data != null)
            {
                return JsonConvert.DeserializeObject<List<ResponseData>>(response.Data.ToString());
            }
        }
        else
        {
             return clockList.Select(x => new ResponseData
             {
                 IsSuccess = false,
                 Message = response.Message,
                 Code = x.OperationSubtypeCode
             }).ToList();
        }

        return new List<ResponseData>();
    }

    public List<ProcessType> GetProcessTypes(string processType, User systemOperator, bool WithTool = false, DateTime? DeltaDate = null)
        => throw new NotSupportedException("GetProcessTypes is not available through the HTTP proxy.");

    public Task<List<ProcessTypeDetail>> ListMachineProcessTypeDetails(string machineId, User systemOperator)
        => throw new NotSupportedException("ListMachineProcessTypeDetails is not available through the HTTP proxy.");
}
