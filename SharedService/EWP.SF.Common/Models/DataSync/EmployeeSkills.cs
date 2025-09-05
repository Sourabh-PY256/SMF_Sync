namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class EmployeeSkills
{
	/// <summary>
	///
	/// </summary>
	public string Employee_Skills_Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SkillId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NameSkill { get; set; }

	/// <summary>
	///
	/// </summary>
	public string QualificationObtained { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CertificationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachedDocument { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NameDocument { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentId { get; set; }
}

/// <summary>
///
/// </summary>
public class EmployeeSkillsProcessType
{
	/// <summary>
	///
	/// </summary>
	public string Employee_Skills_Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SkillId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }
}
