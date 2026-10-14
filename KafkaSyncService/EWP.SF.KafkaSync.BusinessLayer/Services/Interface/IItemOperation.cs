using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessEntities.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IItemOperation
{
    public  Task<List<ResponseData>> ListUpdateComponentBulk(List<ComponentExternal> itemList, List<ComponentExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    
    
}