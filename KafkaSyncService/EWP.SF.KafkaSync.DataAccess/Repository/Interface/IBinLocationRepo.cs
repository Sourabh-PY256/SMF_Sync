using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IBinLocationRepo
{
    List<BinLocation> ListBinLocation(string Code = "", DateTime? DeltaDate = null);
    ResponseData MergeBinLocation(BinLocation BinLocationInfo, User systemOperator, bool Validation);
}