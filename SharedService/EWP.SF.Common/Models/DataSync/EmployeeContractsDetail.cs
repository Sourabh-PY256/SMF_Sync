namespace EWP.SF.Common.Models;

public class EmployeeContractsDetail
{
	public string Employee_Contracts_Detail_Id { get; set; }
	public string EmployeeId { get; set; }
	public DateTime DateStart { get; set; }
	public DateTime? DateEnd { get; set; }
	public string ProfileId { get; set; }
	public decimal Salary { get; set; }
	public decimal OvertimeCost { get; set; }
	public int CreatedById { get; set; }
	public DateTime CreationDate { get; set; }
	public decimal VacationsDays { get; set; }
	public decimal HolidaysIncluded { get; set; }
}

public class EmployeeCurrentPosition
{
	public string ProfileId { get; set; }
	public string NameProfile { get; set; }
	public string Code { get; set; }
	public DateTime DateStart { get; set; }
}
