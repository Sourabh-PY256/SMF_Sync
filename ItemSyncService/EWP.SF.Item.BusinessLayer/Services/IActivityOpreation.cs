using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.Text;
using Range = EWP.SF.Common.Models.Range;
using SixLabors.ImageSharp;
using EWP.SF.Common.Models.Catalogs;

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
