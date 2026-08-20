using EWP.SF.Helper;

namespace EWP.SF.KafkaSync.BusinessLayer;

/// <summary>
///
/// </summary>
public partial class ExpressionResolver
{
	/// <summary>
	///
	/// </summary>
	public static int Cast_random(object value) => new Random().Next(0, value.ToInt32() + 1);

	/// <summary>
	///
	/// </summary>
	public static double Cast_abs(object value) => Math.Abs(value.ToDouble());

	/// <summary>
	///
	/// </summary>
	public static double Cast_float(object value) => value.ToDouble();

	/// <summary>
	///
	/// </summary>
	public static int Cast_integer(object value) => value.ToInt32();

	/// <summary>
	///
	/// </summary>
	public static string Cast_text(object value) => value.ToStr();

	/// <summary>
	///
	/// </summary>
	public static double Cast_round(object value, int decimals) => Math.Round(value.ToDouble(), decimals);

	/// <summary>
	///
	/// </summary>
	public static object Cast_iif(bool condition, object ifTrue, object ifFalse) => condition ? ifTrue : ifFalse;
}
