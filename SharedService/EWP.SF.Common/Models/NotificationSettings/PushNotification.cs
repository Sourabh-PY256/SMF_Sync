
public class NotificationRequest
{
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Url { get; set; }
}

public class TokenPushNotification
{
    public string Token { get; set; }
    public int UserId { get; set; }
    public string Endpoint { get; set; }
    public string Auth { get; set; }
    public string PKey { get; set; }
}
