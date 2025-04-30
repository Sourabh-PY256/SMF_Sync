

using EWP.SF.Common.Enumerators;
namespace EWP.SF.Common.ResponseModels;

public class ResponseDataException : Exception
{
	public ActionDB Action { get; set; }
	public string Code { get; set; }

	public ResponseDataException(string message, string code = "", ActionDB action = ActionDB.NA) : base(message)
	{
		Action = action;
		Code = code;
	}

	public ResponseDataException()
	{
	}

	public ResponseDataException(string message) : base(message)
	{
	}

	public ResponseDataException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
