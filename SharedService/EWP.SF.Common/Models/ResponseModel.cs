namespace EWP.SF.Common.Models;

public class ResponseModel
{
	public bool IsSuccess { get; set; }
	public string ErrorCode { get; set; }
	public string Message { get; set; }
	public bool IsValidation { get; set; }
	public string Level { get; set; }
	public object Data { get; set; }
	public string Checksum { get; set; }
	public string RemoteHost { get; set; }
	public DateTime? RequestDate { get; set; }

	public ResponseModel()
	{
		IsSuccess = true;
	}
}
