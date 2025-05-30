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
public interface IProcedureRepo
{
    ResponseData ProcessMasterInsByXML(Procedure procesInfo
    , string xmlSections
    , string xmlInstructions
    , string xmlChoice
    , string xmlRange
    , string xmlActionCheckBoxs
    //  , string xmlMultipleChoiceCheckBox
    // , string xmlActionOperators
    , User systemOperator
    , string xmlComponents
    , string xmlAtachments
    , bool IsValidation = false);
    Procedure GetProcedure(string ProcedureId, string ActivityId = null, string Instance = null);

    ResponseData ProcessMasterIns(Procedure ProcessMaster, User User, bool IsValidation = false);




}
