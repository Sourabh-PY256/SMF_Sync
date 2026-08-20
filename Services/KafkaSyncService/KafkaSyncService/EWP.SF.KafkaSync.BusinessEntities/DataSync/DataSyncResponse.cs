using System.Net;
using System.Net.Http.Headers;

namespace EWP.SF.KafkaSync.BusinessEntities;
public class DataSyncResponse
{
	public HttpStatusCode StatusCode { get; set; }

	public string StatusMessage { get; set; }

	public HttpResponseHeaders ReturnHeaders { get; set; }

	public string Response { get; set; }
}

public class DataSyncExecuteResponse
{
	public string Service { get; set; }

	public bool IsSuccess { get; set; } = true;

	public string Response { get; set; }

	public DataSyncHttpResponse ResponseHttp { get; set; }
}

public class DataSyncHttpResponse
{
	public HttpStatusCode StatusCode { get; set; }

	public string Message { get; set; }

	public string LogId { get; set; }

	public int FailRecords { get; set; }

	public int SuccessRecords { get; set; }
}
