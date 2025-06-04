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

namespace EWP.SF.Item.DataAccess;

public class OrderTransactionProductRepo : IOrderTransactionProductRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public OrderTransactionProductRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region OrderTransactionProduct
    /// <summary>
	///
	/// </summary>
	public ResponseData MergeOrderTransactionProduct(OrderTransactionProduct OrderProductInfo, User systemOperator, bool Validation, IntegrationSource intSrc = IntegrationSource.ERP)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_OrderTransactionProduct_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddNull("_TransactionId");
				command.Parameters.AddWithValue("_OrderCode", OrderProductInfo.OrderId);
				command.Parameters.AddWithValue("_OperationNo", OrderProductInfo.OperationId);
				command.Parameters.AddWithValue("_IsOutput", OrderProductInfo.IsOutput);
				command.Parameters.AddWithValue("_StartEntryDate", OrderProductInfo.StartEntryDate);
				command.Parameters.AddWithValue("_EndEntryDate", OrderProductInfo.EndEntryDate);
				command.Parameters.AddWithValue("_Direction", OrderProductInfo.Direction);
				command.Parameters.AddWithValue("_OrderFactor", OrderProductInfo.OrderFactor);
				command.Parameters.AddWithValue("_ProcessFactor", OrderProductInfo.ProcessFactor);
				command.Parameters.AddWithValue("_EmployeeId", OrderProductInfo.EmployeeId);
				command.Parameters.AddWithValue("_Operator", OrderProductInfo.Operator);
				command.Parameters.AddWithValue("_ActivityInstanceId", OrderProductInfo.ActivityInstanceId);
				command.Parameters.AddWithValue("_IsPartial", OrderProductInfo.IsPartial);
				command.Parameters.AddWithValue("_IssuedLot", OrderProductInfo.IssuedLot);
				command.Parameters.AddWithValue("_DocCode", OrderProductInfo.DocCode);
				command.Parameters.AddWithValue("_Comments", OrderProductInfo.Comments);
				command.Parameters.AddWithValue("_DocDate", OrderProductInfo.DocDate);
				command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_Details", OrderProductInfo.DetailToJSON, OrderProductInfo.Details?.Count > 0);
				command.Parameters.AddWithValue("_Origin", intSrc.ToInt32());
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

	/// <summary>
	///
	/// </summary>
	public ResponseData MergeOrderTransactionProductStatus(OrderTransactionProductStatus OrderProductInfo, User systemOperator, bool Validation, IntegrationSource intSrc = IntegrationSource.ERP)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_OrderTransactionProductStatus_INS", connection)
				{
					CommandType = CommandType.StoredProcedure,
					CommandTimeout = 120000
				};
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_TransactionId", OrderProductInfo.TransactionId);
				command.Parameters.AddWithValue("_OrderCode", OrderProductInfo.OrderCode);
				command.Parameters.AddWithValue("_OperationNo", OrderProductInfo.OperationNo);
				command.Parameters.AddWithValue("_ItemCode", OrderProductInfo.ItemId);
				command.Parameters.AddWithValue("_Quantity", OrderProductInfo.Quantity);
				command.Parameters.AddCondition("_LotNo", OrderProductInfo.LotNo, !string.IsNullOrEmpty(OrderProductInfo.LotNo));
				command.Parameters.AddCondition("_Pallet", OrderProductInfo.Pallet, !string.IsNullOrEmpty(OrderProductInfo.Pallet));
				command.Parameters.AddCondition("_MachineCode", OrderProductInfo.MachineCode, !string.IsNullOrEmpty(OrderProductInfo.MachineCode));
				command.Parameters.AddCondition("_BinLocationCode", OrderProductInfo.BinLocationCode, !string.IsNullOrEmpty(OrderProductInfo.BinLocationCode));
				command.Parameters.AddCondition("_InventoryStatusCode", OrderProductInfo.InventoryStatusCode, !string.IsNullOrEmpty(OrderProductInfo.InventoryStatusCode));
				command.Parameters.AddCondition("_NewWarehouseCode", OrderProductInfo.NewWarehouseCode, !string.IsNullOrEmpty(OrderProductInfo.NewWarehouseCode));
				command.Parameters.AddWithValue("_NewInventoryStatusCode", OrderProductInfo.NewInventoryStatusCode);
				command.Parameters.AddWithValue("_EmployeeId", systemOperator.EmployeeId);
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddWithValue("_ExternalId", OrderProductInfo.ExternalId);
				command.Parameters.AddWithValue("_ExternalDate", OrderProductInfo.ExternalDate);
				command.Parameters.AddWithValue("_Origin", intSrc.ToInt32());

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
    #endregion OrderTransactionProduct

}   