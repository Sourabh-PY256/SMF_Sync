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
public interface IAssetRepo
{
    List<ProductionLine> ListProductionLines(DateTime? DeltaDate = null);
    ProductionLine GetProductionLine(string Code);

    ResponseData CreateProductionLine(ProductionLine productionLineInfo, User systemOperator, bool Validate = false, string Level = "Success");
    bool DeleteProductionLine(ProductionLine productionLineInfo, User systemOperator);
    //List<WorkCenter> ListWorkCenters(DateTime? DeltaDate = null);
    WorkCenter GetWorkCenter(string WorkCenterCode);

    ResponseData CreateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator, bool Validate = false, string Level = "Success");
    //bool DeleteWorkCenter(WorkCenter WorkCenterInfo, User systemOperator);
    //List<Floor> ListFloors(DateTime? DeltaDate = null);
    Floor GetFloor(string FloorCode);

    ResponseData CreateFloor(Floor FloorInfo, User systemOperator, bool Validate = false, string Level = "Success");
    //bool DeleteFloor(Floor FloorInfo, User systemOperator);
    //List<Facility> ListFacilities(DateTime? DeltaDate = null);
    Facility GetFacility(string Code);

    ResponseData CreateFacility(Facility FacilityInfo, User systemOperator, bool Validate = false, string Level = "Success");
    //bool DeleteFacility(Facility FacilityInfo, User systemOperator);



}
