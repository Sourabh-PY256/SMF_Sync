using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IMeasureUnitOperation
{
    List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null);
    Task<List<ResponseData>> ListUpdateUnitMeasure(List<MeasureUnitExternal> measureUnitInfoList, List<MeasureUnitExternal> measureUnitInfoListOriginal, User systemOperator, bool Validate, LevelMessage Level);

}