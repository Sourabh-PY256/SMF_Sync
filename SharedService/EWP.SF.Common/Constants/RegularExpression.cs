namespace EWP.SF.Common.Constants;

public static class RegularExpression
{
	#region Generic

	public const string DigitGreaterThanZero = @"^(0*[1-9][0-9]*(\\.[0-9]+)?|0+\\.[0-9]*[1-9][0-9]*)$";
	public const string EntityCode = "^[a-zA-Z0-9-|_. ]*$";
	public const string UsageRegex = "IncrementStart|IncrementEnd|DecrementStart|DecrementEnd|IncrementforProcessTimeOnly|DecrementforProcessTimeOnly|NoChange|IncrementToEnd|DecrementToEnd|IncrementSetupTimeOnly|DecrementSetupTimeOnly|IncrementFromStartOfSetup|DecrementFromStartOfSetup|IncrementForEntireJob|DecrementForEntireJob";

	#endregion Generic

	#region Product

	public const string ProductStatusIntegration = "Active|Draft";

	#endregion Product
}
