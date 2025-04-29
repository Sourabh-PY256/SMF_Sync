using System.Data;

using EWP.SF.ConnectionModule;
using EWP.SF.Helper;

using MySqlConnector;

namespace EWP.SF.ShopFloor.DataAccess;

public class UtilitiesRepository : IUtilitiesRepository
{
	private readonly string ConnectionString;

	public UtilitiesRepository(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
	}

	public object SingleResultByQuery(string cmd, string fieldname)
	{
		string response = string.Empty;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new(cmd, connection);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				using DataTable dt = new();
				dt.Load(rdr);

				if (dt.Rows.Count > 0)
				{
					response = dt.Rows[0].Field<string>(fieldname);
				}
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return response;
	}

	public async Task<DateTime> GetServerUTCTime(CancellationToken cancellationToken = default)
	{
		DateTime returnValue = DateTime.UtcNow;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SYS_SERVER_TIME", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				try
				{
					// Execute reader asynchronously
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// Read the first value (if exists)
						if (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
						{
							returnValue = rdr["now"].ToDate();
						}
					}
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					//logger?.Error(ex);
					throw;
				}
				finally
				{
					await connection.CloseAsync().ConfigureAwait(false);
				}

				return returnValue;
			}
		}
	}

	public async Task<string> GetChecksumTable(string tableName, CancellationToken cancellationToken = default)
	{
		string returnValue = "";

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SYS_CHECKSUM_TABLE", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				_ = command.Parameters.AddWithValue("_TableName", tableName);

				try
				{
					// Execute reader asynchronously
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// Read the first value (if exists)
						if (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
						{
							returnValue = rdr["Checksum"].ToStr();
						}
					}
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					//logger?.Error(ex);
					throw;
				}
				finally
				{
					await connection.CloseAsync().ConfigureAwait(false);
				}

				return returnValue;
			}
		}
	}
}
