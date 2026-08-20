namespace EWP.SF.Common.ResponseModels;

/// <summary>
///
/// </summary>
public class ResponseAttachment
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string URL { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Ext { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsFileOffice { get; set; }

	/// <summary>
	///
	/// </summary>
	public ResponseFileAttachment File { get; set; }
}

/// <summary>
///
/// </summary>
public class ResponseFileAttachment
{
	/// <summary>
	///
	/// </summary>
	public string name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FileBase64 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string lastModified { get; set; }

	/// <summary>
	///
	/// </summary>
	public string lastModifiedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string type { get; set; }
}
