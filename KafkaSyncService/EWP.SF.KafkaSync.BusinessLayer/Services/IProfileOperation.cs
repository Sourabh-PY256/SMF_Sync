using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IProfileOperation
{
    Task<List<ResponseData>> ListUpdateProfile(List<PositionExternal> profileInfoList, List<PositionExternal> profileInfoListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    Task<ResponseData> MergeProfile(CatProfile ProfileInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true);
}