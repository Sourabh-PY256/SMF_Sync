using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface ISupplyOperation
{
    Task<List<ResponseData>> ListUpdateSupply(List<SupplyExternal> SupplyList, List<SupplyExternal> SupplyListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    

}