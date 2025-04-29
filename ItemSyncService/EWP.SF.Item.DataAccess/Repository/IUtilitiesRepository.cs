using System.Data;

namespace EWP.SF.ShopFloor.DataAccess;

/// <summary>
/// Interface for utilities repository that provides common database utility functions
/// </summary>
public interface IUtilitiesRepository
{
    /// <summary>
    /// Executes a query and returns a single result from the specified field
    /// </summary>
    /// <param name="cmd">The SQL command to execute</param>
    /// <param name="fieldname">The field name to retrieve from the result</param>
    /// <returns>The value of the specified field</returns>
    object SingleResultByQuery(string cmd, string fieldname);

    /// <summary>
    /// Gets the current UTC time from the server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The server's current UTC time</returns>
    Task<DateTime> GetServerUTCTime(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the checksum for a specified table
    /// </summary>
    /// <param name="tableName">The name of the table to get the checksum for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The checksum value as a string</returns>
    Task<string> GetChecksumTable(string tableName, CancellationToken cancellationToken = default);
}
