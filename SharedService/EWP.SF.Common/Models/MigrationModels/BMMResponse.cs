using Newtonsoft.Json;


namespace EWP.SF.Common.Models.MigrationModels;

/// <summary>
///
/// </summary>
public class ApiDocumentation
{
	/// <summary>
	///
	/// </summary>
	public List<APIDocumentationChangelog> Changelog { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<APIDocumentationEndpoint> Endpoints { get; set; }
}

/// <summary>
///
/// </summary>
public class APIDocumentationChangelog
{
	/// <summary>
	///
	/// </summary>
	public DateTime LogDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EndpointId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Endpoint { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }
}

/// <summary>
///
/// </summary>
public class APIDocumentationEndpoint
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Method { get; set; }

	/// <summary>
	///
	/// </summary>
	public string URL { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Brief { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SampleRequest { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SampleResponse { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<APIDocumentationAttribute> Attributes { get; set; }
}

/// <summary>
///
/// </summary>
public class APIDocumentationAttribute
{
	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Group { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMReceiptResponse
{
	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LotNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime Date { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMIssueMaterialResponse
{
	/// <summary>
	///
	/// </summary>
	public BMMIssueMaterialResponse()
	{
	}

	/// <summary>
	///
	/// </summary>
	public BMMIssueMaterialResponse(string transactionId)
	{
		TransactionId = transactionId;
	}

	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderLotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime Date { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Process { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMIssuedMaterial> Materials { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMIssuedMaterial
{
	/// <summary>
	///
	/// </summary>
	public string MaterialId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MaterialName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InputLotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }
}

/// <summary>
///
/// </summary>
public class IntegrationResult
{
	/// <summary>
	///
	/// </summary>
	public bool IsSuccess
	{
		get
		{
			return !Messages.Any(static x => x.Code != "100");
		}
	}

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<IntegrationResponse> Messages { get; set; }

	/// <summary>
	///
	/// </summary>
	public object Data { get; set; }

	/// <summary>
	///
	/// </summary>
	public IntegrationResult()
	{
		Messages = [];
	}
}

/// <summary>
///
/// </summary>
public class IntegrationResponse
{
	private readonly Dictionary<string, string> codeDefs = new()
		{
			 { "100","Ok" }
			,{ "400","Bad or invalid request" }
			,{ "300","External id cannot be empty" }
			,{ "301","Code cannot be empty" }
			,{ "302","Name cannot be empty" }
			,{ "303","Unit Type cannot be empty" }
			,{ "304","Quantity must be greater than zero" }
			,{ "305","Unit cannot be empty" }
			,{ "306","Presentation Unit Id cannot be empty" }
			,{ "307","Presentation Factor cannot be zero nor empty" }
			,{ "308","Product must have at least one process" }
			,{ "309","Process Step cannot be empty" }
			,{ "310","Process Type cannot be empty" }
			,{ "311","Process Type is not valid" }
			,{ "312","Process Output unit cannot be empty" }
			,{ "313","Process Time must be greater than zero" }
			,{ "314","Process Transform Factor cannot be empty when the process output unit is different than the product" }
			,{ "315","Component External id cannot be empty" }
			,{ "316","Component Id is not valid" }
			,{ "317","Component Quantity cannot be empty" }
			,{ "318","Component Unit Id cannot be empty" }
			,{ "319","Component Unit is invalid" }
			,{ "320","Unit is invalid" }
			,{ "321","Presentation unit must be a discrete unit" }
			,{ "322","Process Output unit is invalid" }
			,{ "323","Component External id does not exist" }
			,{ "324","Only one process on the last step is allowed" }
			,{ "325","Cannot convert type from Product to Raw Material" }
			,{ "326","Material for product cannot be the same as the product identifier" }
			,{ "327","The Process requires materials" }
			,{ "328","Product Id is empty or invalid" }
			,{ "329","Planned Start date is empty or invalid" }
			,{ "330","Planned End date is empty or invalid" }
			,{ "331","Order Lot  numbers must be unique" }
			,{ "332","Order must have at least one process" }
			,{ "333","Lot totals doesnt macth with order total" }
			,{ "334","Invalid product" }
			,{ "335","Process not found on the Product" }
			,{ "336","This order cannot be edited" }
			,{ "337","The Process requires at least one resource" }
			,{ "338","Invalid resource Identifier" }
			,{ "339","Lot number is required" }
			,{ "340","Lot quantity is required" }
			,{ "341","a valid lot status is required" }
			,{ "342","External id is not valid" }
			,{ "343","Quality result is required" }
			,{ "344","Quality result must be between 0 and 100" }
			,{ "345","Lot location is required" }
			,{ "346","External Order is required for allocated lots" }
			,{ "347","Factor must be a decimal" }
			,{ "348","Process Type Identifier does not exist" }
			,{ "349","This component does not belong to the product materials" }
			,{ "350","Output process unit type must be the same of the final product ({0})" }
			,{ "351","Process type unit ({0}) does not allow '{1}' as output unit" }
			,{ "352","Output unit must be equal as the products output unit for this this process ({0})" }
			,{ "353","Status is required" }
			,{ "354","Order status is not valid. Please refer to Status API for valid statuses" }
			,{ "355","Order material must be returned before remove." }
			,{ "356","Migration is already processing this entity" }
			,{ "357","Resource cannot be changed on a running order" }
			,{ "358","Subproduct has product receipt and cannot be removed" }
			,{ "359","This is an invalid order" }
			,{ "360","This property is required" }
			,{ "361","Only one lot per product/sub-product is allowed" }
			,{ "362","Order must be running before inserting transactions" }
			,{ "363","Warehouse Code is not valid" }
			,{ "364","Invalid value for this field" }
			,{ "365","This resource does not exist or is disabled" }
			,{ "366","Time must be in the following format DD:HH:MM" }
			,{ "367","Error parsing time value" }
			,{ "368","This resource is Auxiliar or is not yet configured" }
			,{ "369","Product versioning is disabled on this company" }
			,{ "370","Previous version for this component not found" }
			,{ "371","The Process requires at least one machine" }
			,{ "372","The Process must have one primary machine " }
			,{ "373","Machine code is invalid " }
			,{ "374","Either product/warehouse/version was not found" }
		};

	/// <summary>
	///
	/// </summary>
	public IntegrationResponse(string code)
	{
		Code = code;

		if (codeDefs.TryGetValue(code, out string value))
		{
			Message = value;
		}
	}

	/// <summary>
	///
	/// </summary>
	public IntegrationResponse(string code, KeyValuePair<string, string> data)
	{
		Code = code;

		if (codeDefs.TryGetValue(code, out string value))
		{
			Message = value;
		}
		if (!data.Equals(default(KeyValuePair<string, string>)))
		{
			Property = data.Key;
			Value = data.Value;
		}
	}

	/// <summary>
	///
	/// </summary>
	public IntegrationResponse(string code, KeyValuePair<string, string> data, object obj)
	{
		Code = code;

		if (codeDefs.TryGetValue(code, out string value))
		{
			Message = value;
		}
		if (!data.Equals(default(KeyValuePair<string, string>)))
		{
			Property = data.Key;
			Value = data.Value;
		}
		if (obj is not null)
		{
			AuxData = obj;
		}
	}

	/// <summary>
	///
	/// </summary>
	public IntegrationResponse(string code, string data)
	{
		Code = code;

		if (codeDefs.TryGetValue(code, out string value))
		{
			Message = value;
		}
		if (!string.IsNullOrEmpty(data))
		{
			Property = data;
		}
	}

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Property { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	public object AuxData { get; set; }
}
