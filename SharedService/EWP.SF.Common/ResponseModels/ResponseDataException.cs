

using EWP.SF.Common.Enumerators;
namespace EWP.SF.Common.ResponseModels;

public class ResponseDataException : Exception
{
	/// <summary>
	/// Gets or sets the action that triggered the exception.
	/// </summary>
	public ActionDB Action { get; set; }

	/// <summary>
	/// Gets or sets the error code.
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ResponseDataException" /> class.
	/// </summary>
	public ResponseDataException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="Exception" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error.</param>
	public ResponseDataException(string message) : base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="Exception" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
	public ResponseDataException(string message, Exception innerException) : base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="Exception" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="code">The error code.</param>
	/// <param name="action">The action that triggered the exception.</param>
	public ResponseDataException(string message, string code = "", ActionDB action = ActionDB.NA) : base(message)
	{
		Action = action;
		Code = code;
	}
}
