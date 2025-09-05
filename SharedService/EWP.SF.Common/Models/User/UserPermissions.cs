namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class UserPermissions
{
	/// <summary>
	///
	/// </summary>
	public int ID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public int FatherId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Url { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Level { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PermissionCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Permission { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsCustom { get; set; }
}
/// <summary>
///
/// </summary>
public class ReqPermissions
{
	/// <summary>
	///
	/// </summary>
	public int ID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PermissionCode { get; set; }
}
/// <summary>
///
/// </summary>
public class ResPermissions
{
	/// <summary>
	///
	/// </summary>
	public string EntityCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PermissionCode { get; set; }
}
