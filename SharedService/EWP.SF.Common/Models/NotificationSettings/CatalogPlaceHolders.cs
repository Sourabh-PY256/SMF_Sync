namespace EWP.SF.Common.Models.NotificationSettings;

/// <summary>
///
/// </summary>
public class CatalogPlaceHolders
{
	/// <summary>
	///
	/// </summary>
	public string sensor_name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string sensor_reading { get; set; }

	/// <summary>
	///
	/// </summary>
	public string user_name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string machine_name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? event_date { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? event_hour { get; set; }

	/// <summary>
	///
	/// </summary>
	public string event_type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string name_explode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string campo_extra { get; set; }

	/*
    Agregar las propiedades necesarias a utilizar en la plantilla dinámica de mensajes
    Estas propiedades serán serializadas y guardadas en el campo DataValues para posteriormente leerse como Dic<string, string> y reemplazarse
    por los valores guardados.
 */
 
}
