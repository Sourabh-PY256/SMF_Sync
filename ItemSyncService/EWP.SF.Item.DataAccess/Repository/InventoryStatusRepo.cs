using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.ConnectionModule;
using System.Text;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

public class InventoryStatusRepo : IInventoryStatusRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public InventoryStatusRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }

#region InventoryStaus
/// <summary>
	///
	/// </summary>
	public List<InventoryStatus> ListInventoryStatus(string Code = "", DateTime? DeltaDate = null)
	{
		List<InventoryStatus> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_InventoryStatus_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				if (string.IsNullOrEmpty(Code))
				{
					command.Parameters.AddNull("_Code");
				}
				else
				{
					command.Parameters.AddWithValue("_Code", Code);
				}
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					InventoryStatus element = new()
					{
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						CreateDate = rdr["CreateDate"].ToDate(),
						CreateUser = new User(rdr["CreateUser"].ToInt32()),
						Status = (Status)rdr["Status"].ToInt32(),
						IsDelivery = rdr["IsDelivery"].ToBool(),
						IsARInvoice = rdr["IsARInvoice"].ToBool(),
						IsARCreditMemo = rdr["IsARCreditMemo"].ToBool(),
						IsReturn = rdr["IsReturn"].ToBool(),
						IsAPReturn = rdr["IsAPReturn"].ToBool(),
						IsAPCreditMemo = rdr["IsAPCreditMemo"].ToBool(),
						IsInventoryIssue = rdr["IsInventoryIssue"].ToBool(),
						IsMaterialIssue = rdr["IsMaterialIssue"].ToBool(),
						IsAllocation = rdr["IsAllocation"].ToBool(),
						IsInventoryTransfer = rdr["IsInventoryTransfer"].ToBool(),
						IsInventoryCounting = rdr["IsInventoryCounting"].ToBool(),
						IsPlanning = rdr["IsPlanning"].ToBool(),
						LogDetailId = rdr["LogDetailId"].ToStr()
					};

					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						element.UpdateDate = rdr["UpdateDate"].ToDate();
						element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
					}

					(returnValue ??= []).Add(element);
				}
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
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public ResponseData MergeInventoryStatus(InventoryStatus InventoryStatusInfo, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_InventoryStatus_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddWithValue("_Code", InventoryStatusInfo.Code);
				command.Parameters.AddWithValue("_Name", InventoryStatusInfo.Name);
				command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				command.Parameters.AddWithValue("_Status", InventoryStatusInfo.Status.ToInt32());
				// command.Parameters.AddWithValue("_Level", Level);
				command.Parameters.AddWithValue("_IsDelivery", InventoryStatusInfo.IsDelivery);
				command.Parameters.AddWithValue("_IsARInvoice", InventoryStatusInfo.IsARInvoice);
				command.Parameters.AddWithValue("_IsARCreditMemo", InventoryStatusInfo.IsARCreditMemo);
				command.Parameters.AddWithValue("_IsReturn", InventoryStatusInfo.IsReturn);
				command.Parameters.AddWithValue("_IsAPReturn", InventoryStatusInfo.IsAPReturn);
				command.Parameters.AddWithValue("_IsAPCreditMemo", InventoryStatusInfo.IsAPCreditMemo);
				command.Parameters.AddWithValue("_IsInventoryIssue", InventoryStatusInfo.IsInventoryIssue);
				command.Parameters.AddWithValue("_IsMaterialIssue", InventoryStatusInfo.IsMaterialIssue);
				command.Parameters.AddWithValue("_IsAllocation", InventoryStatusInfo.IsAllocation);
				command.Parameters.AddWithValue("_IsInventoryTransfer", InventoryStatusInfo.IsInventoryTransfer);
				command.Parameters.AddWithValue("_IsInventoryCounting", InventoryStatusInfo.IsInventoryCounting);
				command.Parameters.AddWithValue("_IsPlanning", InventoryStatusInfo.IsPlanning);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ResponseData
					{
						Action = (ActionDB)rdr["Action"].ToInt32(),
						IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
						Code = rdr["Code"].ToStr(),
						Message = rdr["Message"].ToStr(),
					};
				}
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
		return returnValue;
	}
#endregion InventoryStatus
}