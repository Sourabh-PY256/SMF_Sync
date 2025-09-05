
/// <summary>
///
/// </summary>
public class NotificationRequest
{
	/// <summary>
	///
	/// </summary>
	public string UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Url { get; set; }
}

/// <summary>
///
/// </summary>
public class TokenPushNotification
{
	/// <summary>
	///
	/// </summary>
	public string Token { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Endpoint { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Auth { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PKey { get; set; }
}
