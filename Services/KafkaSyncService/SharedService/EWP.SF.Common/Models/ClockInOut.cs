namespace EWP.SF.Common.Models;

//public class ClockInOut
//{
//    public string ClockInOutId { get; set; }
//    public string EmployeeId { get; set; }
//    public int UserId { get; set; }
//    public DateTime Date { get; set; }
//    public DateTime Time { get; set; }
//    public int TypeClock { get; set; }
//    public int ValueReturn { get; set; }
//}

/// <summary>
///
/// </summary>
public class ClockInOutDetails
{
	/// <summary>
	///
	/// </summary>
	public string ClockInOutId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LastName { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime Date { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TypeClock { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Profile { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ValueReturn { get; set; }

	/// <summary>
	///
	/// </summary>
	public int CheckedSupervisor { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IsSupervisor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Reason { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Shift { get; set; }
}

/// <summary>
///
/// </summary>
public class EmployeeSupervisorDetails
{
	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LastName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Profile { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Shift { get; set; }
}

/// <summary>
///
/// </summary>
public class ClockInOutDetailsExternal
{
	/// <summary>
	///
	/// </summary>
	public string ClockInOutId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EventType { get; set; }
}
