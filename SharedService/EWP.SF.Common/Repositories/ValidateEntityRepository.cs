using System.Data;
using System.Globalization;
using System.Text;
using EWP.SF.ConnectionModule;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models; 
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;

using MySqlConnector;

namespace EWP.SF.Common.DataAccess;

public partial class ValidateEntityRepository : IValidateEntityRepository
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");


    public ValidateEntityRepository(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
    }


    /// <summary>
	///
	/// </summary>
	public void ValidateEntityCode(string key, string entity, User systemOperator)
	{
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_ValidateEntityCode", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddCondition("_EntityType", () => entity, !string.IsNullOrEmpty(entity), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Entity"));
			command.Parameters.AddCondition("_EntityCode", () => key, !string.IsNullOrEmpty(key), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Key"));
			command.Parameters.AddCondition("_UserId", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}
		catch
		{
			throw;
		}
		finally
		{
			connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}

}
