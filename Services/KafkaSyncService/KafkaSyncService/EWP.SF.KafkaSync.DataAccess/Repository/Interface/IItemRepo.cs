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
public interface IItemRepo
{
    List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null);
    Task<List<Component>> ListComponents(string componentId, bool ignoreImages = false, string filter = "", DateTime? DeltaDate = null, CancellationToken cancellationToken = default);
    ResponseData MergeComponentBulk(string Json, User systemOperator, bool Validation);
}