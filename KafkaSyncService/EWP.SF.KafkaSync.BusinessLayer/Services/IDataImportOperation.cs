using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IDataImportOperation
{
    List<Activity> GetDataImportTasks(ProcessTypeExternal operationType);
    List<Activity> GetDataImportTasks(ProductOperation operationType, User systemOperator);
    Task<List<ProcessEntryComponent>> GetDataImportItems(ProductExternal item, ProcessEntry pe, User systemOperator);
    List<ProcessEntryTool> GetDataImportTooling(ProductExternal item, ProcessEntry pe, User systemOperator);
    List<DeviceSpeed> GetDataImportAvailableDevices(ProductOperation operationType, ProcessEntryProcess oldOperation = null);
    Task<List<Entity>> ListEntities();
    Task<List<SubProduct>> GetDataImportSubProducts(ProductOperation operationType);

    List<ProcessEntryLabor> GetDataImportLabor(ProductExternal item, ProcessEntry pe, User systemOperator);

    List<Activity> GetDataImportOerderTasks(Common.Models.WorkOrderOperation operationType, OrderProcess currentProcess);

}