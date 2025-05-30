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