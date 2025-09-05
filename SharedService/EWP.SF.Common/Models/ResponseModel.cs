namespace EWP.SF.Common.Models;

/// <summary>
/// Response model for API responses.
/// </summary>
public class ResponseModel
{
	/// <summary>
	/// Indicates whether the API request was successful.
	/// </summary>
	public bool IsSuccess { get; set; }

	/// <summary>
	/// Indicates the error code if the API request failed.
	/// </summary>
	public string ErrorCode { get; set; }

	/// <summary>
	/// Indicates the message associated with the API response.
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// Indicates if the request was just fo validation.
	/// </summary>
	public bool IsValidation { get; set; }

	/// <summary>
	/// Indicates the level of the API response.
	/// </summary>
	public string Level { get; set; }

	/// <summary>
	/// Indicates the Data of the API response.
	/// </summary>
	public object Data { get; set; }

	/// <summary>
	/// Indicates the checksum of the API response.
	/// </summary>
	public string Checksum { get; set; }

	/// <summary>
	/// Indicates the remote host of the API request.
	/// </summary>
	public string RemoteHost { get; set; }

	/// <summary>
	/// Indicates the date and time when the API request was made.
	/// </summary>
	public DateTime? RequestDate { get; set; }

	/// <summary>
	/// Constructor for the ResponseModel class.
	/// </summary>
	public ResponseModel()
	{
		IsSuccess = true;
	}

	/// <summary>
	/// Constructor for the ResponseModel class with data.
	/// </summary>
	public ResponseModel(object data) : this()
	{
		Data = data;
	}
}
