namespace EWP.SF.Common.Models;

public class RequestContext
{
	public string Token { get; set; }
	public string IP { get; set; }
	public User User { get; set; }
	public bool IsPublic { get; set; }
	public string LanguageCode { get; set; }
	public bool MultiSession { get; set; }
	public string MainStartModule { get; set; }
	public string EmployeeCode { get; set; }
	public EmployeeContext Employee { get; set; }
	public DateTime ExpirationDate { get; set; }
	public string Browser { get; set; }
	public string OS { get; set; }
	public string Location { get; set; }
	public string Avatar { get; set; }
	public string AttachmentIds { get; set; }
	public double TimeZoneOffset { get; set; }
	public bool Auth2FA { get; set; }
	public bool UsedMultiFactor { get; set; }
	public bool TemporalPassword { get; set; }
	public string DefaultMenu { get; set; }
}

public class EmployeeContext
{
	public string Id { get; set; }
	public string Code { get; set; }
	public string Name { get; set; }
	public string LastName { get; set; }
	public string FullName { get; set; }
	public string Email { get; set; }
	public string PositionCode { get; set; }
	public string PositionName { get; set; }
	public string UserTypeCode { get; set; }
}
