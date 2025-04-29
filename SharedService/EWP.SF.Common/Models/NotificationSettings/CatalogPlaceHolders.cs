namespace EWP.SF.Common.Models.NotificationSettings;

public class CatalogPlaceHolders
{
	public string sensor_name { get; set; }
	public string sensor_reading { get; set; }
	public string user_name { get; set; }
	public string machine_name { get; set; }
	public DateTime? event_date { get; set; }
	public int? event_hour { get; set; }
	public string event_type { get; set; }
	public string name_explode { get; set; }

	public string campo_extra { get; set; }

	/*
    Agregar las propiedades necesarias a utilizar en la plantilla dinámica de mensajes
    Estas propiedades serán serializadas y guardadas en el campo DataValues para posteriormente leerse como Dic<string, string> y reemplazarse
    por los valores guardados.
    */

}
