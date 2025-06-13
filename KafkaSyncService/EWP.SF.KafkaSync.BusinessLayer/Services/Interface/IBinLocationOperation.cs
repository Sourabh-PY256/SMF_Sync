
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IBinLocationOperation
{
    Task<List<ResponseData>> ListUpdateBinLocation(List<BinLocationExternal> binLocationList, List<BinLocationExternal> binLocationListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    Task<ResponseData> MergeBinLocation(BinLocation BinLocationInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true);
}