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
public interface IWarehouseRepo
{
    Warehouse GetWarehouse(string Code);
    ResponseData MergeWarehouse(Warehouse WarehouseInfo, User systemOperator, bool Validation);

    List<Warehouse> ListWarehouse(string Code = "", DateTime? DeltaDate = null);
    
}
