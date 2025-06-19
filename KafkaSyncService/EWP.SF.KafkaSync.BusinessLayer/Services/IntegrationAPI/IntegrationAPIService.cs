using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class APIWebClient : HttpClient
{
	public int TimeoutSeconds
	{
		get => Timeout.Seconds;
		set => Timeout = TimeSpan.FromSeconds(value);
	}

	public HttpHeaders Headers
	{
		get
		{
			return DefaultRequestHeaders;
		}
	}

	public async Task<string> DownloadString(string uri)
	{
		string response = string.Empty;
		using (HttpResponseMessage httpResponse = await GetAsync(new Uri(uri)).ConfigureAwait(false))
		{
			byte[] byteArray = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
			response = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
		}
		return response;
	}

	public async Task<string> UploadString(string uri, string httpMethod, string body)
	{
		_ = HttpMethod.Post;
		if (httpMethod.Equals("delete", StringComparison.OrdinalIgnoreCase))
		{
			_ = HttpMethod.Delete;
		}
		else if (httpMethod.Equals("put", StringComparison.OrdinalIgnoreCase))
		{
			_ = HttpMethod.Put;
		}
		else if (httpMethod.Equals("patch", StringComparison.OrdinalIgnoreCase))
		{
			_ = HttpMethod.Patch;
		}
		else if (httpMethod.Equals("head", StringComparison.OrdinalIgnoreCase))
		{
			_ = HttpMethod.Head;
		}
		else if (httpMethod.Equals("trace", StringComparison.OrdinalIgnoreCase))
		{
			_ = HttpMethod.Trace;
		}

		using StringContent httpContent = new(body, Encoding.UTF8, "application/json");
		HttpResponseMessage result = await PostAsync(new Uri(uri), httpContent).ConfigureAwait(false);
		byte[] byteArray = await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
		return Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
	}

	public async Task<DataSyncResponse> DataSyncDownload(string uri, string body = "")
	{
		DataSyncResponse response = new();
		
		// // Parse the original URI
		// Uri originalUri = new Uri(uri);
		
		// // Construct new URI with host.docker.internal
		// UriBuilder uriBuilder = new UriBuilder
		// {
		// 	Scheme = "http",
		// 	Host = "host.docker.internal",
		// 	Port = 8030,
		// 	Path = originalUri.AbsolutePath,
		// 	Query = originalUri.Query
		// };
		
		using HttpRequestMessage requestGet = new()
		{
			Method = HttpMethod.Get,
			//RequestUri = uriBuilder.Uri,
			RequestUri = new Uri(uri),
			Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json),
		};
		using (HttpResponseMessage httpResponse = await SendAsync(requestGet).ConfigureAwait(false))
		{
			byte[] byteArray = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
			string contentType = httpResponse.Content.Headers.ContentType?.MediaType;
			response = new DataSyncResponse
			{
				StatusCode = httpResponse.StatusCode,
				StatusMessage = httpResponse.ReasonPhrase,
				ReturnHeaders = httpResponse.Headers,
				Response = contentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) != false
						? Encoding.UTF8.GetString(byteArray) // Decodifica si es texto.

						: Convert.ToBase64String(byteArray)  // Convierte a Base64 si es binario.
			};
		}
		return response;
	}

	public async Task<DataSyncResponse> DataSyncUpload(string uri, string httpMethod, string body)
	{
		_ = new DataSyncResponse();
		using StringContent httpContent = new(body, Encoding.UTF8, "application/json");
		DataSyncResponse response;
		HttpResponseMessage result;
		if (httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
		{
			result = await PostAsync(new Uri(uri), httpContent).ConfigureAwait(false);
		}
		else if (httpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase))
		{
			result = await PutAsync(new Uri(uri), httpContent).ConfigureAwait(false);
		}
		else if (httpMethod.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
		{
			result = await PatchAsync(new Uri(uri), httpContent).ConfigureAwait(false);
		}
		else
		{
			throw new NotSupportedException($"HTTP method {httpMethod} is not supported.");
		}
		using (result)
		{
			byte[] byteArray = await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
			response = new()
			{
				StatusCode = result.StatusCode,
				StatusMessage = result.ReasonPhrase,
				ReturnHeaders = result.Headers,
				Response = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length)
			};
		}

		return response;
	}
}
