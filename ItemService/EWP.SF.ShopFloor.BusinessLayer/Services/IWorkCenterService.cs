
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.ShopFloor.BusinessEntities;

namespace EWP.SF.ShopFloor.BusinessLayer;

public interface IWorkCenterService
{
    Task<List<WorkCenter>> ListWorkCenter(DateTime? deltaDate = null);
    Task<WorkCenter> GetWorkCenter(string workCenterCode);
    //Task<ResponseData> CreateWorkCenter(WorkCenter workCenterInfo, User systemOperator, bool validation = false, string level = "Success");
    Task<bool> UpdateWorkCenter(WorkCenter workCenterInfo, User systemOperator);
    Task<bool> DeleteWorkCenter(WorkCenter workCenterInfo, User systemOperator);
}
