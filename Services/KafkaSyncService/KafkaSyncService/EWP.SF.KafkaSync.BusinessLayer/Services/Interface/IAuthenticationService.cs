using System.Threading.Tasks;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

public interface IAuthenticationService
{
    /// <summary>
    /// Gets a valid access token. Retrieves from cache if available and not expired, 
    /// otherwise performs a login request.
    /// </summary>
    /// <returns>The access token or null if authentication fails.</returns>
    Task<string> GetAccessTokenAsync();

    /// <summary>
    /// Clears the cached token, forcing a fresh login on the next request.
    /// </summary>
    void ClearToken();
}
