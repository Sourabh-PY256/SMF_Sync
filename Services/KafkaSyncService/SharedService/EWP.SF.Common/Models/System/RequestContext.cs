namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class RequestContext
{
	/// <summary>
	///
	/// </summary>
	public string Token { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IP { get; set; }

	/// <summary>
	///
	/// </summary>
	public User User { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsPublic { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LanguageCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool MultiSession { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MainStartModule { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public EmployeeContext Employee { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpirationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Browser { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Avatar { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TimeZoneOffset { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Auth2FA { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UsedMultiFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool TemporalPassword { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DefaultMenu { get; set; }
}

/// <summary>
///
/// </summary>
public class EmployeeContext
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

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
	public string FullName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PositionCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PositionName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UserTypeCode { get; set; }
}
