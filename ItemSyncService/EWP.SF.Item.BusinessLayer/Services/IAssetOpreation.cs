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
public interface IAssetOperation
{
    Task<ResponseData> CreateFacility(Facility FacilityInfo, User systemOperator
        , bool Validate = false
        , string Level = "Success"
        , bool NotifyOnce = true
        );
    Task<ResponseData> CreateFloor(Floor FloorInfo, User systemOperator
        , bool Validate = false
        , string Level = "Success"
        , bool NotifyOnce = true
        );
    Task<ResponseData> CreateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator
        , bool Validate = false
        , string Level = "Success"
        , bool NotifyOnce = true
        );
    Task<ResponseData> CreateProductionLine(ProductionLine productionLineInfo, User systemOperator
        , bool Validate = false
        , string Level = "Success"
        , bool NotifyOnce = true
        );
}
