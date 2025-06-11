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

namespace EWP.SF.KafkaSync.DataAccess;

public class SupplyRepo : ISupplyRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public SupplyRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    /// <summary>
	///
	/// </summary>
	public async Task<List<Supply>> ListSupply(string orderNumber = "", CancellationToken cancel = default)
	{
		List<Supply> returnValue = [];

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancel).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_Supply_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				// Clear and add parameters
				command.Parameters.Clear();
				if (string.IsNullOrEmpty(orderNumber))
				{
					command.Parameters.AddWithValue("_Id", DBNull.Value);
				}
				else
				{
					command.Parameters.AddWithValue("_Id", orderNumber);
				}

				try
				{
					// Execute the reader asynchronously
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// Get ordinals for performance optimization
						int idOrdinal = rdr.GetOrdinal("Code");
						int codeOrdinal = rdr.GetOrdinal("Code");
						int vendorCodeOrdinal = rdr.GetOrdinal("VendorCode");
						int vendorNameOrdinal = rdr.GetOrdinal("VendorName");
						int lineNoOrdinal = rdr.GetOrdinal("LineNo");
						int itemCodeOrdinal = rdr.GetOrdinal("ItemCode");
						int quantityOrdinal = rdr.GetOrdinal("Quantity");
						int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
						int unitOrdinal = rdr.GetOrdinal("Unit");
						int expectedDateOrdinal = rdr.GetOrdinal("ExpectedDate");
						int typeOrdinal = rdr.GetOrdinal("Type");
						int createDateOrdinal = rdr.GetOrdinal("CreateDate");
						int createUserOrdinal = rdr.GetOrdinal("CreateUser");
						int statusOrdinal = rdr.GetOrdinal("Status");

						while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
						{
							Supply element = new()
							{
								Id = rdr[idOrdinal].ToStr(),
								Code = rdr[codeOrdinal].ToStr(),
								VendorCode = rdr[vendorCodeOrdinal].ToStr(),
								VendorName = rdr[vendorNameOrdinal].ToStr(),
								LineNo = rdr[lineNoOrdinal].ToInt32(),
								ItemCode = rdr[itemCodeOrdinal].ToStr(),
								Quantity = rdr[quantityOrdinal].ToDecimal(),
								WarehouseCode = rdr[warehouseCodeOrdinal].ToStr(),
								Unit = rdr[unitOrdinal].ToStr(),
								ExpectedDate = rdr[expectedDateOrdinal].ToDate(),
								Type = rdr[typeOrdinal].ToInt32(),
								CreateDate = rdr[createDateOrdinal].ToDate(),
								CreateUser = new User(rdr[createUserOrdinal].ToInt32()),
								Status = (Status)rdr[statusOrdinal].ToInt32()
							};

							returnValue.Add(element);
						}
					}
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					//logger.Error(ex);
					throw;
				}

				return returnValue;
			}
		}
	}

	/// <summary>
	///
	/// </summary>
	public ResponseData MergeSupply(Supply SupplyInfo, User systemOperator, bool Validation)
	{
		ArgumentNullException.ThrowIfNull(SupplyInfo);

		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Supply_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddNull("_Id");
				command.Parameters.AddWithValue("_PurchaseOrder", SupplyInfo.Code);
				command.Parameters.AddWithValue("_VendorCode", SupplyInfo.VendorCode);
				command.Parameters.AddWithValue("_VendorName", SupplyInfo.VendorName);
				command.Parameters.AddWithValue("_LineNo", SupplyInfo.LineNo);
				command.Parameters.AddWithValue("_ItemCode", SupplyInfo.ItemCode);
				command.Parameters.AddWithValue("_Quantity", SupplyInfo.Quantity);
				command.Parameters.AddWithValue("_WarehouseCode", SupplyInfo.WarehouseCode);
				command.Parameters.AddWithValue("_Unit", SupplyInfo.Unit);
				command.Parameters.AddWithValue("_ExpectedDate", SupplyInfo.ExpectedDate);
				command.Parameters.AddWithValue("_Type", SupplyInfo.Type);
				command.Parameters.AddWithValue("_Status", SupplyInfo.Status);
				command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_Employee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				// command.Parameters.AddWithValue("_Level", Level);

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

}