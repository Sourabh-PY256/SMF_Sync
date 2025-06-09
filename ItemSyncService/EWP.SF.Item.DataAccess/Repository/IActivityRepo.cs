using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IActivityRepo

{
    Activity CreateActivity(Activity activityInfo, User systemOperator);
    bool UpdateActivity(Activity activityInfo, User systemOperator);

    List<ActivityInstanceCalculateResponse> ActivityInstanceCalculate(ActivityInstanceCalculateRequest activityInfo, User systemOperator);

    Activity GetActivity(Activity activityInfo);

    bool DeleteActivity(Activity activityInfo, User systemOperator);
    MessageBroker ActivityMergeSchedule(Activity activityInfo, User systemOperator);
    bool ActivityItemInsByXML(User systemOperator, string xmlComponents);
    string CloneActivityProcessMaster(string ActivityId, string ActivityIdNew, string Origin);

    List<ActivityType> ListActivityTypes();
    List<ActivityClass> ListActivityClasses();
    List<ActivitySource> ListActivitySources();
    List<Intervention> ListActivityInterventions();
    bool AssociateActivityProcessEntry(string ProcessEntryId, string ProcessId, string ActivityId, int TriggerId, int SortId, bool isMandatory, string RawMaterials, User systemOperator);
    bool RemoveActivityProcessEntryAssociation(string ProcessEntryId, string ProcessId, string ActivityId, User systemOperator);

    bool AssociateActivityWorkOrder(string WorkOrderId, string ProcessId, string MachineId, string ActivityId, int TriggerId, int SortId, bool isMandatory, string RawMaterials, User systemOperator);
    bool RemoveActivityWorkOrderAssociation(string WorkOrderId, string ProcessId, string MachineId, string ActivityId, User systemOperator);
}
