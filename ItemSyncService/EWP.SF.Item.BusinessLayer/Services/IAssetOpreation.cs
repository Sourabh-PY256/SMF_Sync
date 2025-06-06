using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

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
        Task<List<ResponseData>> CreateAssetsExternal(List<AssetExternal> AssetsList, List<AssetExternal> AssetListOriginal, User user, bool Validate, string Level);
}
