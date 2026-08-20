namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class EmployeeContractsDetail
{
	/// <summary>
	///
	/// </summary>
	public string Employee_Contracts_Detail_Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DateStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? DateEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProfileId { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Salary { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal OvertimeCost { get; set; }

	/// <summary>
	///
	/// </summary>
	public int CreatedById { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal VacationsDays { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal HolidaysIncluded { get; set; }
}

/// <summary>
///
/// </summary>
public class EmployeeCurrentPosition
{
	/// <summary>
	///
	/// </summary>
	public string ProfileId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NameProfile { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DateStart { get; set; }
}
