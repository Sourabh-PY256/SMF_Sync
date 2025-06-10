
using EWP.SF.Common.Models;

namespace EWP.SF.Item.BusinessLayer;

public interface IActivityOperation
{
    Task<Activity> CloneActivity(Activity request, User systemOperator, string Origin);
    Task<Activity> CreateActivity(Activity activityInfo, User systemOperator);
    Task<bool> DeleteActivity(Activity activityInfo, User systemOperator);
    Task<Activity> UpdateActivity(Activity activityInfo, User systemOperator);

    bool AssociateActivityProcessEntry(string ProcessEntryId, string ProcessId, string ActivityId, int triggerId, int sortId, bool isMandatory, string rawMaterials, User systemOperator);
    bool RemoveActivityProcessEntryAssociation(string ProcessEntryId, string ProcessId, string ActivityId, User systemOperator);
    bool AssociateActivityWorkOrder(string WorkOrderId, string ProcessId, string MachineId, string ActivityId, int triggerId, int sortId, bool isMandatory, string rawMaterials, User systemOperator);
    bool RemoveActivityWorkOrderAssociation(string WorkOrderId, string ProcessId, string MachineId, string ActivityId, User systemOperator);
}
