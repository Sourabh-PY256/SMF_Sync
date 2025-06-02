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

public class OrderTransactionMaterialRepo : IOrderTransactionMaterialRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public OrderTransactionMaterialRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region OrderTransactionMaterial
    public ResponseData MergeOrderTransactionMaterial(OrderTransactionMaterial OrderMaterialInfo, User systemOperator, bool Validation, IntegrationSource intSrc = IntegrationSource.ERP)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_OrderTransactionMaterial_INS", connection)
				{
					CommandType = CommandType.StoredProcedure,
					CommandTimeout = 120000
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddNull("_TransactionId");
				command.Parameters.AddWithValue("_OrderCode", OrderMaterialInfo.OrderId);
				command.Parameters.AddWithValue("_OperationNo", OrderMaterialInfo.OperationId);
				command.Parameters.AddWithValue("_Direction", OrderMaterialInfo.Direction);
				command.Parameters.AddWithValue("_EmployeeId", OrderMaterialInfo.EmployeeId);
				command.Parameters.AddWithValue("_DocCode", OrderMaterialInfo.DocCode);
				command.Parameters.AddWithValue("_Comments", OrderMaterialInfo.Comments);
				command.Parameters.AddWithValue("_DocDate", OrderMaterialInfo.DocDate);
				command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_Details", OrderMaterialInfo.DetailToJSON, OrderMaterialInfo.Details?.Count > 0);
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
    #endregion OrderTransactionMaterialRepo
}