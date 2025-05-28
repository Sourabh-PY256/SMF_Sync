using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IDataSyncRepository
{

    Task<DataSyncServiceLog> GetDataSyncServiceLogs(string LogId, int logType = 0, CancellationToken cancellationToken = default);
    string GetDatasyncDynamicBody(string EntityCode);
    List<TimeZoneCatalog> GetTimezones(bool currentValues);
    Task<List<DataSyncService>> GetBackgroundService(string backgroundService, string httpMethod, CancellationToken cancellationToken = default);
    bool UpdateDataSyncServiceExecution(string Id, DateTime ExecutionDate);
    DataSyncErpAuth GetDataSyncServiceErpToken(string ErpCode);

    bool InsertDataSyncServiceErpToken(DataSyncErpAuth TokenInfo);

    ResponseData MergeComponentBulk(string Json, User systemOperator, bool Validation);

    List<Inventory> ListInventory(string Code = "", DateTime? DeltaDate = null);

    List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null);
    Task<List<Component>> ListComponents(string componentId, bool ignoreImages = false, string filter = "", DateTime? DeltaDate = null, CancellationToken cancellationToken = default);
    Warehouse GetWarehouse(string Code);
    ResponseData MergeWarehouse(Warehouse WarehouseInfo, User systemOperator, bool Validation);
    Task<User> GetUser(int userId, string userHash, User systemOperator, CancellationToken cancellationToken = default);
    List<ProductionLine> ListProductionLines(DateTime? DeltaDate = null);

    ProductionLine GetProductionLine(string Code);

    ResponseData CreateProductionLine(ProductionLine productionLineInfo, User systemOperator, bool Validation, string Level);

    bool DeleteProductionLine(ProductionLine productionLineInfo, User systemOperator);

    WorkCenter GetWorkCenter(string WorkCenterCode);

    ResponseData CreateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator, bool Validation, string Level);

    ResponseData CreateFloor(Floor FloorInfo, User systemOperator, bool Validation, string Level);

    Floor GetFloor(string FloorCode);

    ResponseData CreateFacility(Facility FacilityInfo, User systemOperator, bool Validation, string Level);

    Facility GetFacility(string Code);

    List<SchedulingCalendarShifts> GetSchedulingCalendarShifts(string Id, string AssetCode, string IdParent, int AssetLevel, string AssetLevelCode, string Origin = null);

    List<SchedulingShiftStatus> GetSchedulingShiftStatus(string Code, string Type, DateTime? DeltaDate = null);

    public ResponseData ProcessMasterInsByXML(Procedure procesInfo
    , string xmlSections
    , string xmlInstructions
    , string xmlChoice
    , string xmlRange
    , string xmlActionCheckBoxs
    //  , string xmlMultipleChoiceCheckBox
    // , string xmlActionOperators
    , User systemOperator
    , string xmlComponents
    , string xmlAtachments
    , bool IsValidation = false);
    Procedure GetProcedure(string ProcedureId, string ActivityId = null, string Instance = null);

    ResponseData ProcessMasterIns(Procedure ProcessMaster, User User, bool IsValidation = false);

    MessageBroker ActivityMergeSchedule(Activity activityInfo, User systemOperator);

    bool DeleteActivity(Activity activityInfo, User systemOperator);

    Activity GetActivity(Activity activityInfo);

    Task<AttachmentResponse> AttachmentPut(AttachmentLocal attachment, User systemOperator);

    Task<string> AttachmentSync(string AttachmentId, string AuxId, User systemOperator);

    List<ActivityInstanceCalculateResponse> ActivityInstanceCalculate(ActivityInstanceCalculateRequest activityInfo, User systemOperator);

    bool ActivityItemInsByXML(User systemOperator, string xmlComponents);

    SchedulingCalendarShifts PutSchedulingCalendarShifts(SchedulingCalendarShifts request, User systemOperator);

    bool DeleteSchedulingCalendarShifts(SchedulingCalendarShifts request);
    Inventory GetInventory(string Code);
    ResponseData MergeInventory(Inventory InventoryInfo, User systemOperator, bool Validation);

    bool UpdateActivity(Activity activityInfo, User systemOperator);

    ResponseData MergeBinLocation(BinLocation BinLocationInfo, User systemOperator, bool Validation);

    List<BinLocation> ListBinLocation(string Code = "", DateTime? DeltaDate = null);

    ResponseData PutSchedulingShiftStatus(SchedulingShiftStatus request, User systemOperator, bool Validation);

    List<EmployeeSkills> EmployeeSkillsList(string id);

    List<EmployeeContractsDetail> ContractsDetailList(string id);

    List<EmployeeSkills> CreateEmployeeSkills(string employeeId, string XML, User systemOperator);

    List<EmployeeContractsDetail> CreateEmployeeContractsDetail(string employeeId, string XML, User systemOperator);

    void GeneratePositionShiftExplosion(string EmployeeCode);

    ResponseData MRGEmployee(Employee employee, bool Validation, User systemOperator);








}
