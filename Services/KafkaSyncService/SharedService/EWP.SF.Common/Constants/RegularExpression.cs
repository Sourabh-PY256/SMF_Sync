namespace EWP.SF.Common.Constants;

/// <summary>
///
/// </summary>
public static class RegularExpression
{
	#region Generic

	/// <summary>
	///
	/// </summary>
	public const string DigitGreaterThanZero = @"^(0*[1-9][0-9]*(\\.[0-9]+)?|0+\\.[0-9]*[1-9][0-9]*)$";

	/// <summary>
	///
	/// </summary>
	public const string EntityCode = "^[a-zA-Z0-9-|_. ]*$";

	/// <summary>
	///
	/// </summary>
	public const string UsageRegex = "IncrementStart|IncrementEnd|DecrementStart|DecrementEnd|IncrementforProcessTimeOnly|DecrementforProcessTimeOnly|NoChange|IncrementToEnd|DecrementToEnd|IncrementSetupTimeOnly|DecrementSetupTimeOnly|IncrementFromStartOfSetup|DecrementFromStartOfSetup|IncrementForEntireJob|DecrementForEntireJob";

	#endregion Generic

	#region Product

	/// <summary>
	///
	/// </summary>
	public const string ProductStatusIntegration = "Active|Draft";

	#endregion Product
}
