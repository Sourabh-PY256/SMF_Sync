using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IProcedureOperation
{

    Task<ResponseData> ProcessMasterInsByXML(
        Procedure processMasterinfo,
        User systemOperator,
        bool Validate = false,
        bool NotifyOnce = true);
    Task<ResponseData> ProcessMasterIns(
    Procedure processMasterinfo,
    User systemOperator,
    bool Validate = false,
    bool NotifyOnce = true);
    Task<Procedure> GetProcedure(string ProcedureId, string ActivityId = null, string Instance = null);
}