using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Models;
using Microsoft.Extensions.Hosting;

namespace EWP.SF.Item.BusinessLayer;

public interface IItemService 
{
    /// <summary>
    /// Sets the service data for the item service
    /// </summary>
    /// <param name="Data">The data sync service configuration</param>
    //void SetServiceData(DataSyncService Data);

    /// <summary>
    /// Executes the service manually based on trigger type and execution origin
    /// </summary>
    /// <param name="Data">The data sync service configuration</param>
    /// <param name="Trigger">The trigger type (Erp, SmartFactory, etc.)</param>
    /// <param name="ExecOrigin">The execution origin (Event, Timer, etc.)</param>
    /// <param name="SystemOperator">The user initiating the execution</param>
    /// <param name="EntityCode">The entity code</param>
    /// <param name="BodyData">The request body data</param>
    /// <returns>HTTP response with execution status</returns>
    Task<DataSyncHttpResponse> ManualExecution(DataSyncService Data, TriggerType Trigger, ServiceExecOrigin ExecOrigin, User SystemOperator, string EntityCode, string BodyData);
}