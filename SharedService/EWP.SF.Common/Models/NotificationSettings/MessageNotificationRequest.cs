using System.ComponentModel.DataAnnotations;

namespace EWP.SF.Common.Models.NotificationSettings;

public class MessageNotificationRequest
{
	/// <summary>
	/// Valores de placeholder a sustituir dentro del Mensaje
	/// Opcional si el mensaje no utiliza placeholder y/o se obtendran por query
	/// </summary>
	public List<DataValue> Placeholders { get; set; }

	/// <summary>
	/// LLave primary a utilizar en query para obtener los valores de placeholder
	/// Opcional si el mensaje no utiliza placeholder y/o no se obtendran valores por query
	/// </summary>
	[StringLength(36)]
	public string AuxKeyId { get; set; }

	/// <summary>
	/// Mensaje del Email a enviar
	/// Opcional si se obtendra por Template
	/// </summary>
	public string Body { get; set; }

	/// <summary>
	/// Mensaje del InApp y MsTeam a enviar
	/// Opcional si se obtendra por Template
	/// </summary>
	[StringLength(500)]
	public string Message { get; set; }

	/// <summary>
	/// Tipo de Notificación
	/// InApp, Email, MsTeams,
	/// </summary>
	[Required]
	public string TargetType { get; set; }

	/// <summary>
	/// Id del empleado la que se enviará el mensaje
	/// </summary>
	[Required]
	public List<string> ToEmp { get; set; }

	/// <summary>
	/// Opcional si se obtendra por Template
	/// </summary>
	[StringLength(150)]
	public string Subject { get; set; }

	/// <summary>
	/// Prioridad del mensaje
	/// Opcional si se obtendra por Template
	/// 1 = Baja, 2 = Media, 3 = Alta
	/// </summary>
	[Range(0, 3)]
	public string Priority { get; set; }

	/// <summary>
	/// Opcional en caso de requerir confirmar el mensaje de atendido
	/// </summary>
	public bool RequiresConfirm { get; set; }

	/// <summary>
	/// Opcional en caso de confirmar la notificación
	/// </summary>
	public string KeyConfirm { get; set; }

	/// <summary>
	/// Opcional Codigo del template a utilizar para el mensaje
	/// </summary>
	[StringLength(150)]
	public string TemplateCode { get; set; }
	public string EntityCode { get; set; }

	public MessageNotificationRequest()
	{
		ToEmp = [];
		Placeholders = [];
	}
}

public class DataValue
{
	public string Code { get; set; }
	public string Value { get; set; }
}
