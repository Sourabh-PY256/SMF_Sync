namespace EWP.SF.Common.Models;

public class UserPermissions
{
	public int ID { get; set; }
	public string Name { get; set; }
	public int FatherId { get; set; }
	public string Url { get; set; }
	public int Level { get; set; }
	public int SortId { get; set; }
	public string PermissionCode { get; set; }
	public string Permission { get; set; }
	public bool IsCustom { get; set; }
}
public class ReqPermissions
{
	public int ID { get; set; }
	public string PermissionCode { get; set; }
}
public class ResPermissions
{
	public string EntityCode { get; set; }
	public string PermissionCode { get; set; }
}
