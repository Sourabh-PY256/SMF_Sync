
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IDemandOperation
{
    Task<List<ResponseData>> ListUpdateDemandBulk(List<DemandExternal> demandList, List<DemandExternal> demandListOriginal, User systemOperator, bool Validate, LevelMessage Level);


}