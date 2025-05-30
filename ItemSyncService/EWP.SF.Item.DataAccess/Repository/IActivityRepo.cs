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
}
