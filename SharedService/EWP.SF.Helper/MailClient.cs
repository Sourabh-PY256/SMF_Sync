using System.Net;
using System.Net.Mail;
using System.Text;

namespace EWP.SF.Helper;

#nullable disable
/// <summary>
/// MailClient is a class that provides functionality to send emails using SMTP.
/// </summary>
public sealed class MailClient : IDisposable
{
	private readonly SmtpClient client;

	/// <summary>
	/// Gets or sets the SMTP server address.
	/// </summary>
	public string SMTPServer { get; set; }
	/// <summary>
	/// Gets or sets the SMTP port number.
	/// </summary>
	public int SMTPPort { get; set; }
	/// <summary>
	/// Gets or sets the SMTP username.
	/// </summary>
	public string SMTPusername { get; set; }
	/// <summary>
	/// Gets or sets the SMTP password.
	/// </summary>
	public string SMTPpassword { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether SSL is enabled.
	/// </summary>
	public bool SSL { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether authentication is required.
	/// </summary>
	public bool RequiresAuth { get; set; }

#nullable disable

	/// <summary>
	/// Initializes a new instance of the <see cref="MailClient"/> class.
	/// </summary>
	public MailClient()
	{
		client = new SmtpClient();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MailClient"/> class with specified parameters.
	/// </summary>
	/// <param name="server"></param>
	/// <param name="port"></param>
	/// <param name="user"></param>
	/// <param name="password"></param>
	/// <param name="requiresAuth"></param>
	/// <param name="ssl"></param>
	public MailClient(string server, int port, string user, string password, bool requiresAuth = true, bool ssl = true)
	{
		client = new SmtpClient();
		SMTPServer = server;
		SMTPPort = port;
		SMTPpassword = password;
		SMTPusername = user;
		RequiresAuth = requiresAuth;
		SSL = ssl;
		InitializeClient();
	}

	/// <summary>
	/// Initializes the SMTP client with the specified settings.
	/// </summary>
	public void InitializeClient()
	{
		if (!RequiresAuth || !(string.IsNullOrEmpty(SMTPusername) || string.IsNullOrEmpty(SMTPpassword)))
		{
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential(SMTPusername, SMTPpassword);
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.Host = SMTPServer;
			client.Port = SMTPPort;
			client.EnableSsl = SSL;
			client.Timeout = 30000;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MailClient"/> class.
	/// </summary>
	/// <param name="email"></param>
	/// <param name="title"></param>
	/// <param name="body"></param>
	/// <param name="attachments"></param>
	/// <param name="fromEmail"></param>
	/// <param name="fromName"></param>
	/// <param name="cc"></param>
	/// <param name="bcc"></param>
	/// <returns></returns>
	public async Task<bool> SendMessage(string email, string title, string body, List<KeyValuePair<string, byte[]>> attachments, string fromEmail = "", string fromName = "", string cc = "", string bcc = "")
	{
		using MailMessage mail = new(fromEmail, email);
		mail.SubjectEncoding = Encoding.UTF8;
		mail.BodyEncoding = Encoding.UTF8;
		mail.From = new MailAddress(fromEmail, fromName);
		mail.Sender = new MailAddress(fromEmail, fromName);
		mail.Subject = title;
		mail.Body = body;
		mail.IsBodyHtml = true;

		if (attachments is not null)
		{
			foreach (KeyValuePair<string, byte[]> att in attachments)
			{
				mail.Attachments.Add(new Attachment(new MemoryStream(att.Value), att.Key));
			}
		}

		if (!string.IsNullOrEmpty(cc))
		{
			cc = cc.Replace(';', ',');
			mail.CC.Add(cc);
		}
		if (!string.IsNullOrEmpty(bcc))
		{
			bcc = bcc.Replace(';', ',');
			mail.Bcc.Add(bcc);
		}
		await client.SendMailAsync(mail).ConfigureAwait(false);
		return true;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MailClient"/> class.
	/// </summary>
	/// <param name="email"></param>
	/// <param name="title"></param>
	/// <param name="body"></param>
	/// <param name="attachments"></param>
	/// <param name="fromEmail"></param>
	/// <param name="fromName"></param>
	/// <param name="cc"></param>
	/// <param name="bcc"></param>
	/// <returns></returns>
	public async Task<bool> SendMessageAsync(string email, string title, string body, List<KeyValuePair<string, byte[]>> attachments, string fromEmail = "", string fromName = "", string cc = "", string bcc = "")
	{
		using MailMessage mail = PrepareMailMessage(email, title, body, attachments, fromEmail, fromName, cc, bcc);
		await client.SendMailAsync(mail).ConfigureAwait(false);
		return true;
	}

	/// <summary>
	/// Disposes the SMTP client and releases any resources used by it.
	/// </summary>
	public void Dispose()
	{
		client.Dispose();

		GC.WaitForPendingFinalizers();
	}

	private static MailMessage PrepareMailMessage(string email, string title, string body, List<KeyValuePair<string, byte[]>> attachments, string fromEmail, string fromName, string cc, string bcc)
	{
		MailMessage mail = new(fromEmail, email)
		{
			SubjectEncoding = Encoding.UTF8,
			BodyEncoding = Encoding.UTF8,
			From = new MailAddress(fromEmail, fromName),
			Sender = new MailAddress(fromEmail, fromName),
			Subject = title,
			Body = body,
			IsBodyHtml = true
		};

		if (attachments is not null)
		{
			foreach (KeyValuePair<string, byte[]> att in attachments)
			{
				mail.Attachments.Add(new Attachment(new MemoryStream(att.Value), att.Key));
			}
		}
		if (!string.IsNullOrEmpty(cc))
		{
			cc = cc.Replace(';', ',');
			mail.CC.Add(cc);
		}
		if (!string.IsNullOrEmpty(bcc))
		{
			bcc = bcc.Replace(';', ',');
			mail.Bcc.Add(bcc);
		}

		return mail;
	}
}
