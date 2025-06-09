using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Item.BusinessEntities;

namespace EWP.SF.Item.BusinessLayer;

public interface IDataImportOperation
{
    Task<List<Activity>> GetDataImportTasks(ProductOperation operationType, User systemOperator);

    Task<List<ProcessEntryComponent>> GetDataImportItems(ProductExternal item, ProcessEntry pe, User systemOperator);
    Task<List<ProcessEntryTool>> GetDataImportTooling(ProductExternal item, ProcessEntry pe, User systemOperator);
    List<DeviceSpeed> GetDataImportAvailableDevices(ProductOperation operationType, ProcessEntryProcess oldOperation = null);
    Task<List<Entity>> ListEntities();
    Task<List<SubProduct>> GetDataImportSubProducts(ProductOperation operationType);

    List<ProcessEntryLabor> GetDataImportLabor(ProductExternal item, ProcessEntry pe, User systemOperator);

    List<Activity> GetDataImportOerderTasks(Common.Models.WorkOrderOperation operationType, OrderProcess currentProcess);

}