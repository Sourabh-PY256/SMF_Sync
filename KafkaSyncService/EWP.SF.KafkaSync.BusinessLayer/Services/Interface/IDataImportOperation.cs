using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IDataImportOperation
{
    List<Activity> GetDataImportTasks(ProcessTypeExternal operationType);
    List<Activity> GetDataImportTasks(ProductOperationExternal operationType, User systemOperator);
    Task<List<ProcessEntryComponent>> GetDataImportItems(ProductExternal item, ProcessEntry pe, User systemOperator);
    Task<List<ProcessEntryTool>> GetDataImportTooling(ProductExternal item, ProcessEntry pe, User systemOperator);
    List<DeviceSpeed> GetDataImportAvailableDevices(ProductOperationExternal operationType, ProcessEntryProcess oldOperation = null);
    Task<List<Entity>> ListEntities();
    Task<List<SubProduct>> GetDataImportSubProducts(ProductOperationExternal operationType);

    Task<List<ProcessEntryLabor>> GetDataImportLabor(ProductExternal item, ProcessEntry pe, User systemOperator);

    List<Activity> GetDataImportOrderTasks(Common.Models.WorkOrderOperation operationType, OrderProcess currentProcess);
   List<Activity> GetDataImportProductionOrderTasks(Common.Models.WorkOrderOperation operationType, ProductionOrderOperation currentProcess);

}