
using EWP.SF.ShopFloor.BusinessLayer;   
using Microsoft.AspNetCore.Mvc;
using EWP.SF.ShopFloor.BusinessEntities;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;

namespace EWP.SF.ShopFloor.API;

[ApiController]
[Route("api/[controller]")]
public class WorkCenterController : BaseController
{
    private readonly IWorkCenterService _workCenterService;

    public WorkCenterController(IWorkCenterService workCenterService)
    {
        _workCenterService = workCenterService;
    }
    #region WorkCenter

	/// <summary>
	/// List WorkCenters in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("WorkCenter/List")]
	[ChecksumTable("sf_workcenter")]
	public async Task<ResponseModel> GetAllWorkCenters()
	{
		ResponseModel returnValue = new();
		_ = GetContext();

		returnValue.Data = _workCenterService.ListWorkCenter(DeltaDate);

		return returnValue;
	}

	/// <summary>
	/// Get a specific WorkCenter in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("WorkCenter/Get/{Code}")]
	public async Task<ResponseModel> GetWorkCenter(string Code = "")
	{
		ResponseModel returnValue = new();
		_ = GetContext();
		returnValue.Data = await _workCenterService.GetWorkCenter(Code);

		return returnValue;
	}

	/// <summary>
	/// Create a WorkCenter
	/// </summary>
	/// <returns></returns>
	// [HttpPost("WorkCenter/Create")]
	// public async Task<ResponseModel> CreateWorkCenter([FromBody] WorkCenter request)
	// {
	// 	ResponseModel returnValue = new();
	// 	RequestContext context = GetContext();

	// 	returnValue.Data = await _workCenterService.CreateWorkCenter(request, context.User).ConfigureAwait(false);

	// 	return returnValue;
	// }

	/// <summary>
	/// Updates a WorkCenter
	/// </summary>
	/// <returns></returns>
	[HttpPost("WorkCenter/Update")]
	public async Task<ResponseModel> UpdateWorkCenter([FromBody] WorkCenter request)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.IsSuccess = await _workCenterService.UpdateWorkCenter(request, context.User);

		return returnValue;
	}

	/// <summary>
	/// Deletes a WorkCenter
	/// </summary>
	/// <returns></returns>
	[HttpPost("WorkCenter/Delete")]
	public async Task<ResponseModel> DeleteWorkCenter(string Id)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.IsSuccess = await _workCenterService.DeleteWorkCenter(new WorkCenter(Id), context.User);

		return returnValue;
	}

	#endregion WorkCenter
}