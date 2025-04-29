using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.ShopFloor.BusinessEntities;

public class SchedulingCalendarShifts
{
	public string Id { get; set; }
	public string CodeShift { get; set; }
	public string CodeOrigin { get; set; }
	public string IdAsset { get; set; }
	public int AssetLevel { get; set; }
	public string AssetLevelCode { get; set; }
	public string IdParent { get; set; }
	public bool IsParent { get; set; }
	public string Name { get; set; }
	public DateTime FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public string Color { get; set; }
	public int Status { get; set; }
	public int UserId { get; set; }
	public bool isEmployee { get; set; }
	public string TypeClock { get; set; }
	public string StatusName { get; set; }
	public string Origin { get; set; }
	public bool Validation { get; set; }
	public List<SchedulingCalendarShifts> listChildren { get; set; }
}
