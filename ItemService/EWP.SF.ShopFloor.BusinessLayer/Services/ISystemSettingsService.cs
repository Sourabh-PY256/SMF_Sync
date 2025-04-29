namespace EWP.SF.ShopFloor.BusinessLayer;

/// <summary>
/// Interface for system settings service that provides system-level functionality
/// </summary>
public interface ISystemSettingsService
{
    /// <summary>
    /// Gets the checksum for a specified table
    /// </summary>
    /// <param name="tableName">The name of the table to get the checksum for</param>
    /// <returns>The checksum value as a string</returns>
    Task<string> GetTableChecksum(string tableName);

    /// <summary>
    /// Gets the current UTC time from the server
    /// </summary>
    /// <returns>The server's current UTC time</returns>
    Task<DateTime> GetServerTimeUTC();
}