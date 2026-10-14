using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessEntities.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IMeasureUnitRepo
{
    ResponseData MergeUnitMeasure(MeasureUnit measureUnitInfo, User systemOperator, bool Validation);
    List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null);
}

