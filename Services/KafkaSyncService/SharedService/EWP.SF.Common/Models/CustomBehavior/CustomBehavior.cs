using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EWP.SF.Common.EventHandlers;
using EWP.SF.Common.Models;
using EWP.SF.Common.Models.Operations;
using EWP.SF.Common.Models.Sensors;
using EWP.SF.Helper;


// using EWP.SF.API.BusinessEntities.EventHandlers;
// using EWP.SF.API.BusinessEntities.MesModels;
// using EWP.SF.API.BusinessEntities.MesModels.Sensors;
// using EWP.SF.API.BusinessEntities.RequestModels.Operations;


namespace EWP.SF.Common.Models;

public static class CustomBehaviorHelper
{
	/// <summary>
	/// Gets the matching behavior for the specified machine and parameter.
	/// </summary>
	public static IBehaviorMatch GetMatch(this Machine machine, MachineParam parameter, string code)
	{
		IBehaviorMatch returnValue = null;
		if (machine is not null && parameter.CustomBehaviorMatch.TryGetValue(code, out BehaviorMatch value))
		{
			BehaviorMatch match = value;
			returnValue = match.SourceType switch
			{
				1 => machine.Sensors.Find(x => x.Status == Enumerators.Status.Active && x.Code.Equals(match.Code, StringComparison.Ordinal)),
				2 => machine.Parameters.Find(x => x.Status == Enumerators.Status.Active && x.Code.Equals(match.Code, StringComparison.Ordinal)),
				3 => new MachineParam { Formula = match.SourceValue, Value = match.SourceValue },
				4 => new MachineParam { Formula = machine.Environment.Values[match.SourceValue], Value = machine.Environment.Values[match.SourceValue] },
				_ => null
			};
		}
		return returnValue;
	}

	/// <summary>
	/// Gets the matching behavior for the specified machine and parameter.
	/// </summary>
	public static T GetMatch<T>(this Machine machine, MachineParam parameter, string code) where T : class
	{
		return machine.GetMatch(parameter, code) as T;
	}

	/// <summary>
	/// Validates the codes for the specified machine and parameter.
	/// </summary>
	public static bool ValidateCodes(this Machine machine, MachineParam currentSelf)
	{
		bool returnValue = true;
		if (machine is not null && currentSelf.CustomBehaviorMatch?.Any(x => machine.GetMatch(currentSelf, x.Key) is null) == true)
		{
			returnValue = false;
		}
		return returnValue;
	}
}

/// <summary>
/// Represents the custom behavior.
/// </summary>
public class CustomBehavior
{
	/// <summary>
	/// Gets or sets a value indicating whether the custom behavior is initialized.
	/// </summary>
	public bool isInitialized;

	/// <summary>
	/// Occurs when data is requested.
	/// </summary>
	public event CustomActionHandler<SensorDataRequest, List<SensorData>> OnRequestData;

	/// <summary>
	/// Occurs when summarized data is requested.
	/// </summary>
	public event CustomActionHandler<SummarizedSensorDataRequest, SummarizedSensorData> OnRequestSummary;

	/// <summary>
	/// Occurs when custom data is requested.
	/// </summary>
	public event EventHandler<NotifyParameterRequest> OnNotify;

	/// <summary>
	/// Occurs when custom data is requested.
	/// </summary>
	public event CustomActionHandler<CustomDataRequest, List<object>> OnRequestCustom;

	/// <summary>
	/// Occurs when a log message is generated.
	/// </summary>
	public event EventHandler<string> OnLog;

	/// <summary>
	/// Gets the data for the specified request.
	/// </summary>
	protected List<SensorData> GetData(SensorDataRequest request)
	{
		using ManualResetEvent resetEvent = new(false);
		List<SensorData> returnValue = null;
		OnRequestData(this, new CustomActionEventArgs<SensorDataRequest, List<SensorData>>
		{
			Request = request,
			Callback = (result) =>
			{
				returnValue = result;
				resetEvent.Set();
			}
		});
		_ = resetEvent.WaitOne(1000);
		return returnValue;
	}

	/// <summary>
	/// Gets the summary for the specified request.
	/// </summary>
	protected SummarizedSensorData GetSummary(SummarizedSensorDataRequest request)
	{
		using ManualResetEvent resetEvent = new(false);
		SummarizedSensorData returnValue = null;
		OnRequestSummary(this, new CustomActionEventArgs<SummarizedSensorDataRequest, SummarizedSensorData>
		{
			Request = request,
			Callback = (result) =>
			{
				returnValue = result;
				resetEvent.Set();
			}
		});
		resetEvent.WaitOne(1000);
		return returnValue;
	}

	/// <summary>
	/// Logs the specified message.
	/// </summary>
	protected void Log(string message)
	{
		if (OnLog is null)
		{
			return;
		}

		OnLog(this, message);
	}

	/// <summary>
	/// Notifies the specified request.
	/// </summary>
	protected void Notify(NotifyParameterRequest request)
	{
		if (OnNotify is null)
		{
			return;
		}

		OnNotify(this, request);
	}

	/// <summary>
	/// Gets the custom data for the specified request.
	/// </summary>
	protected List<object> GetCustom(CustomDataRequest request)
	{
		using ManualResetEvent resetEvent = new(false);
		List<object> returnValue = null;
		OnRequestCustom(this, new CustomActionEventArgs<CustomDataRequest, List<object>>
		{
			Request = request,
			Callback = (result) =>
			{
				returnValue = result;
				_ = resetEvent.Set();
			}
		});
		resetEvent.WaitOne(1000);
		return returnValue;
	}

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the ID.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Gets or sets the author.
	/// </summary>
	public string Author { get; }

	/// <summary>
	/// Gets the library.
	/// </summary>
	public string Library { get; }

	/// <summary>
	/// Gets or sets the required codes.
	/// </summary>
	public string[] RequiredCodes { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CustomBehavior"/> class.
	/// </summary>
	public CustomBehavior()
	{
		Library = GetLibraryName();
		Author = GetAssemblyAuthor();
		Description = GetConstructorDescription();
	}

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	private string GetLibraryName() => GetType().Assembly.ManifestModule.ToStr().Replace(".dll", "");

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	private string GetAssemblyAuthor()
	{
		object[] attribs = GetType().Assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
		return attribs.Length > 0 ? ((AssemblyCompanyAttribute)attribs[0]).Company : string.Empty;
	}

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	private string GetConstructorDescription()
	{
		Attribute attr = GetType().GetConstructors().FirstOrDefault()?.GetCustomAttribute(typeof(DescriptionAttribute));
		return attr is not null ? ((DescriptionAttribute)attr).Description : string.Empty;
	}

	/// <summary>
	/// <para>Gets the self parameter.</para>
	/// </summary>
	protected static MachineParam GetSelf(Machine machine, string paramCode) => machine?.Parameters is not null ? machine.Parameters.Find(x => x.Code == paramCode) : null;

	/// <summary>
	/// Validates the nulls of the required codes in the machine.
	/// </summary>
	protected bool ValidateNulls(Machine machine)
	{
		if (machine is not null)
		{
			foreach (string code in RequiredCodes)
			{
				Sensor sensor = machine.Sensors.Find(x => x.Code == code);
				if (!sensor.IsNull())
				{
				}
			}
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Resolves the custom behavior for the specified machine.
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public virtual CustomBehaviorResult Resolve(Machine machine)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Resolves the custom behavior for the specified machine with a fallback value.
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public virtual CustomBehaviorResult Resolve(Machine machine, string paramCode, string fallBackValue = null)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Validates the codes for the specified machine and parameter.
	/// </summary>
	protected bool ValidateCodes(Machine machine, MachineParam current) => machine.ValidateCodes(current) && RequiredCodes.All(current.CustomBehaviorMatch.ContainsKey);
}
