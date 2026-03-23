using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.API;

[ApiController]
public class ComponentController : BaseController
{
    private readonly IComponentOperation _componentOperation;
    private readonly User _systemOperator;

    public ComponentController(IComponentOperation componentOperation, IDataSyncServiceOperation dataSyncOperation)
    {
        _componentOperation = componentOperation;
        _systemOperator = dataSyncOperation.GetUserWithoutValidations(new User(-1)).Result;
    }

    // ─── UI Endpoint ──────────────────────────────────────────────────────────
    /// <summary>
    /// Called by the UI. Accepts a <see cref="Component"/> model.
    /// NormalizeComponent rules run automatically inside ProcessProduct.
    /// </summary>
    [HttpPost("Product/Merge/{Mode?}")]
    // [RequestValidator]
    // [RequiresToken]
    public async Task<ResponseModel> MergeProduct(
        [FromBody] Component request,
        [FromRoute] string Mode = "Create")
    {
        ResponseModel response = new();
        try
        {
            ResponseData result = await _componentOperation
                .ProcessProduct(ParseMode(Mode), request, _systemOperator)
                .ConfigureAwait(false);

            response.IsSuccess = result.IsSuccess;
            response.Message   = result.Message;
            response.Data      = result.Entity;
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message   = ex.Message;
        }
        return response;
    }

    // ─── ERP / DataSync Endpoint ──────────────────────────────────────────────
    /// <summary>
    /// Called by DataSync or another Microservice. Accepts a <see cref="ProductExternal"/> model.
    /// Internally converts → normalizes → merges using the same shared rules as the UI path.
    /// </summary>
    [HttpPost("Product/Sync/{Mode?}")]
    // [RequestValidator]
    // [RequiresToken]
    public async Task<ResponseModel> SyncProduct(
        [FromBody] ProductExternal request,
        [FromRoute] string Mode = "Create")
    {
        ResponseModel response = new();
        try
        {
            ResponseData result = await _componentOperation
                .ProcessProduct(ParseMode(Mode), request, _systemOperator)
                .ConfigureAwait(false);

            response.IsSuccess = result.IsSuccess;
            response.Message   = result.Message;
            response.Data      = result.Entity;
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message   = ex.Message;
        }
        return response;
    }

    // ─── Helper ───────────────────────────────────────────────────────────────
    private static ActionDB ParseMode(string mode) => mode?.ToLower() switch
    {
        "update" => ActionDB.Update,
        "delete" => ActionDB.Delete,
        _        => ActionDB.Create
    };
}
