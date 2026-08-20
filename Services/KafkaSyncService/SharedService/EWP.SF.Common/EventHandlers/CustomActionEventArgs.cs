namespace EWP.SF.Common.EventHandlers;

/// <summary>
///
/// </summary>
public delegate void CustomActionHandler<T, C>(object sender, CustomActionEventArgs<T, C> e);

/// <summary>
///
/// </summary>
public class CustomActionEventArgs<T, C> : EventArgs
{
	/// <summary>
	///
	/// </summary>
	public T Request { get; set; }

	/// <summary>
	///
	/// </summary>
	public Action<C> Callback { get; set; }

	/// <summary>
	///
	/// </summary>
	public CustomActionEventArgs()
	{
	}

	/// <summary>
	///
	/// </summary>
	public CustomActionEventArgs(T request, Action<C> callback = null)
	{
		Request = request;
		Callback = callback;
	}
}
