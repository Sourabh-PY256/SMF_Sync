using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.DataAccess;

public class DataImportRepo : IDataImportRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public DataImportRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    /// <summary>
	///
	/// </summary>
	public async Task<List<Entity>> ListEntities()
	{
		List<Entity> returnValue = [];
		List<EntityDocImport> listDocs = [];
		await using EWP_Connection connection = new(ConnectionString);
		try
		{
			await using EWP_Command command = new("SP_SF_ENTITIES_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			await connection.OpenAsync().ConfigureAwait(false);
			MySqlDataReader rdr = await command.ExecuteReaderAsync().ConfigureAwait(false);
			while (await rdr.ReadAsync().ConfigureAwait(false))
			{
				Entity element = new()
				{
					Id = rdr["Code"].ToStr(),
					Icon = rdr["Icon"].ToStr(),
					Description = rdr["Description"].ToStr(),
					Name = rdr["Name"].ToStr(),
					Module = rdr["Module"].ToStr(),
					ModuleIcon = rdr["ModuleIcon"].ToStr(),
					ModuleDescription = rdr["ModuleDescription"].ToStr(),
					isVisualHelp = rdr["isVisualHelp"].ToBool(),
					NameEntityExternal = rdr["NameEntityExternal"].ToStr(),
					ListDocsImport = [],
				};

				if (returnValue.IsNull())
				{
					returnValue = [];
				}

				returnValue.Add(element);
			}
			await rdr.NextResultAsync().ConfigureAwait(false);
			while (await rdr.ReadAsync().ConfigureAwait(false))
			{
				EntityDocImport element = new()
				{
					IdEntity = rdr["EntityCode"].ToStr(),
					Name = rdr["Name"].ToStr(),
					Description = rdr["Description"].ToStr(),
					IsMandatory = rdr["IsMandatory"].ToInt32().ToBool(),
					NameEntityExternal = rdr["NameEntityExternal"].ToStr(),
					NamePropertyExternal = rdr["NamePropertyExternal"].ToStr(),
					ParentEntityExternal = DBNull.Value == rdr["ParentEntityExternal"] ? null : rdr["ParentEntityExternal"].ToString(),
					EntityExternalPath = rdr["EntityExternalPath"].ToStr(),
				};
				returnValue.Find(x => x.Id == element.IdEntity).ListDocsImport.Add(element);
			}
		}
		catch (Exception ex)
		{
			//logger.Error(ex);
			throw;
		}
		finally
		{
			await connection.CloseAsync().ConfigureAwait(false);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public ResponseAttachment GetAttachment(string IdAttachment, string AuxId)
	{
		ResponseAttachment returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Attachment_Local_Storage_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_idAttachment", IdAttachment);
				command.Parameters.AddWithValue("_AuxId", AuxId);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ResponseAttachment
					{
						Id = rdr["Id"].ToStr(),
						Type = rdr["TypeCode"].ToString(),
						Name = rdr["Name"].ToStr(),
						URL = rdr["Name"].ToStr(),
						Ext = rdr["Extension"].ToStr(),
						Size = rdr["Size"].ToStr(),
						File = new ResponseFileAttachment()
					};
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return returnValue;
	}
}
