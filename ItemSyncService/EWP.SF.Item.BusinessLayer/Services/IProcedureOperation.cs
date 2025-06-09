using EWP.SF.Common.Enumerators;
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


    Task<List<ResponseData>> ProcessMasterInsExternalSync(List<ProcedureExternalSync> listProcedures
    , List<ProcedureExternalSync> listProceduresOriginal, User systemOperator, bool Validate, LevelMessage Level = 0);
    Procedure GetProcessByProcessCodeVersion(string Code, int Version);
}