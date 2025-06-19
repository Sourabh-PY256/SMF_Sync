using System.Data;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.ConnectionModule;
using System.Text;
using System.Text.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using NLog;

namespace EWP.SF.KafkaSync.DataAccess;

public class AttachmentRepo : IAttachmentRepo
{
    private readonly string ConnectionString;
   
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public AttachmentRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region Attachment
    public async Task<string> AttachmentSync(string AttachmentId, string AuxId, User systemOperator)
    {
        string returnValue = null;
        await using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                await using EWP_Command command = new("SP_SF_Attachment_Sync", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                _ = command.Parameters.AddWithValue("_attachmentId", AttachmentId);
                _ = command.Parameters.AddWithValue("_createuser", systemOperator.Id);
                _ = command.Parameters.AddWithValue("_auxId", AuxId);
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
                await connection.OpenAsync().ConfigureAwait(false);
                MySqlDataReader rdr = await command.ExecuteReaderAsync().ConfigureAwait(false);

                while (await rdr.ReadAsync().ConfigureAwait(false))
                {
                    returnValue = rdr["IdAttachment"].ToStr();
                }
            }
            catch
            {
                returnValue = null;
                throw;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
        return returnValue;
    }
    public async Task<AttachmentResponse> AttachmentPut(AttachmentLocal attachment, User systemOperator)
    {
        AttachmentResponse returnValue = new();
        await using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                await using MemoryStream memoryStream = new();
                if (attachment.KeyColumns is not null)
                {
                    await JsonSerializer.SerializeAsync(memoryStream, attachment.KeyColumns).ConfigureAwait(false);
                }
                await using EWP_Command command = new("SP_SF_Attachment_Ins", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                _ = command.Parameters.AddWithValue("_idAttachment", attachment.Id);
                _ = command.Parameters.AddWithValue("_typeCode", attachment.TypeCode);
                _ = command.Parameters.AddWithValue("_name", attachment.Name ?? "");
                _ = command.Parameters.AddWithValue("_ext", attachment.Extension ?? "");
                _ = command.Parameters.AddWithValue("_size", attachment.Size ?? "");
                _ = command.Parameters.AddWithValue("_createuser", systemOperator.Id);
                _ = command.Parameters.AddWithValue("_auxId", attachment.AuxId);
                _ = command.Parameters.AddWithValue("_entity", attachment.Entity);
                _ = command.Parameters.AddWithValue("_status", attachment.Status);
                _ = command.Parameters.AddWithValue("_KeyValues", Encoding.UTF8.GetString(memoryStream.ToArray()));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
                _ = command.Parameters.AddWithValue("_isTemp", attachment.IsTemp);
                _ = command.Parameters.AddWithValue("_IsImageEntity", attachment.IsImageEntity);
                _ = command.Parameters.AddWithValue("_IsSync", false);
                await connection.OpenAsync().ConfigureAwait(false);
                MySqlDataReader rdr = await command.ExecuteReaderAsync().ConfigureAwait(false);

                while (await rdr.ReadAsync().ConfigureAwait(false))
                {
                    returnValue.Id = rdr["IdAttachment"].ToStr();
                    returnValue.CreationDate = rdr["CreateDate"].ToDate();
                }
            }
            catch
            {
                returnValue.Id = null;
                throw;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
        return returnValue;
    }
    /// <summary>
	///
	/// </summary>
	public async Task<AttachmentResponse> AttachmentIns(AttachmentRequest attachment, User systemOperator, bool IsSync = false)
    {
        AttachmentResponse returnValue = new();
        await using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                await using MemoryStream memoryStream = new();
                await JsonSerializer.SerializeAsync(memoryStream, attachment.KeyColumns).ConfigureAwait(false);
                await using EWP_Command command = new("SP_SF_Attachment_Ins", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddWithValue("_idAttachment", attachment.Id);
                command.Parameters.AddWithValue("_typeCode", attachment.TypeCode);
                command.Parameters.AddWithValue("_name", attachment.Name ?? "");
                //command.Parameters.AddWithValue("_url", (attachment.URL is null ? "" : attachment.URL));
                command.Parameters.AddWithValue("_ext", attachment.Ext ?? "");
                command.Parameters.AddWithValue("_size", attachment.Size ?? "");
                command.Parameters.AddWithValue("_createuser", systemOperator.Id);
                command.Parameters.AddWithValue("_auxId", attachment.AuxId);
                command.Parameters.AddWithValue("_entity", attachment.Entity);
                command.Parameters.AddWithValue("_status", attachment.Status);
                command.Parameters.AddWithValue("_KeyValues", Encoding.UTF8.GetString(memoryStream.ToArray()));
                command.Parameters.AddWithValue("_isTemp", false);
                command.Parameters.AddWithValue("_IsImageEntity", false);
                command.Parameters.AddWithValue("_IsSync", IsSync);
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

                await connection.OpenAsync().ConfigureAwait(false);
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                while (await rdr.ReadAsync().ConfigureAwait(false))
                {
                    returnValue.Id = rdr["IdAttachment"].ToStr();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
        return returnValue;
    }
    /// <summary>
	///
	/// </summary>
	public async Task<List<AttachmentExternalResponse>> AttachmentSyncSel(List<AttachmentExternal> listAttachments,
	 User systemOperator)
	{
		List<AttachmentExternalResponse> returnValue = [];
		await using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				await using MemoryStream memoryStream = new();
				await using EWP_Command command = new("SP_SF_AttachmentSync_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_json", listAttachments.ToJSON());
				command.Parameters.AddWithValue("_user", systemOperator.Id);
				command.Parameters.AddWithValue("_employee", systemOperator.EmployeeId);
				await connection.OpenAsync().ConfigureAwait(false);
				MySqlDataReader rdr = await command.ExecuteReaderAsync().ConfigureAwait(false);

				while (await rdr.ReadAsync().ConfigureAwait(false))
				{
					AttachmentExternalResponse element = new()
					{
						Entity = rdr["Entity"].ToStr(),
						EntityType = rdr["EntityType"].ToStr(),
						AuxId = rdr["AuxId"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Aux1 = rdr["Aux1"].ToStr(),
						Aux2 = rdr["Aux2"].ToStr(),
						Aux3 = rdr["Aux3"].ToStr(),
						Aux4 = rdr["Aux4"].ToStr(),
						AttachmentIdExternal = rdr["AttachmentIdExternal"].ToStr(),
						AttachmentName = rdr["AttachmentName"].ToStr(),
						AttachmentExtension = rdr["AttachmentExtension"].ToStr(),
					};
					returnValue.Add(element);
				}
			}
			catch
			{
				returnValue = null;
				throw;
			}
			finally
			{
				await connection.CloseAsync().ConfigureAwait(false);
			}
		}
		return returnValue;
	}

    #endregion Attachment
}