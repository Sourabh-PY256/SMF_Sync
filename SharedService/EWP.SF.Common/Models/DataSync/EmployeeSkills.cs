namespace EWP.SF.Common.Models;

public class EmployeeSkills
{
	public string Employee_Skills_Id { get; set; }
	public string EmployeeId { get; set; }
	public string SkillId { get; set; }
	public string NameSkill { get; set; }
	public string QualificationObtained { get; set; }
	public DateTime CertificationDate { get; set; }
	public string AttachedDocument { get; set; }
	public string NameDocument { get; set; }
	public string AttachmentId { get; set; }
}

public class EmployeeSkillsProcessType
{
	public string Employee_Skills_Id { get; set; }
	public string EmployeeId { get; set; }
	public string SkillId { get; set; }
	public string ProcessType { get; set; }
	public string Code { get; set; }
	public string Name { get; set; }
}
