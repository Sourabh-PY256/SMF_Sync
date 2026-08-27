using System.Collections.Generic;
using System.Threading.Tasks;
using EWP.SF.Common.Models;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
namespace EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

public interface IMoveEntryOperation
{
    Task<List<ResponseData>> ListUpdateMoveEntry(List<MoveEntryExternal> moveEntryList, User systemOperator, bool Validate, LevelMessage Level, string logId = null);
}
