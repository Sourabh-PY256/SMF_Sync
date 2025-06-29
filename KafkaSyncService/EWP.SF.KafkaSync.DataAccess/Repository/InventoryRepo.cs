using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using NLog;

namespace EWP.SF.KafkaSync.DataAccess;

public class InventoryRepo : IInventoryRepo
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public InventoryRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }

    #region Inventory
    public Inventory GetInventory(string Code)
    {
        Inventory returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ItemGroup_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                // command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "v"));
                _ = command.Parameters.AddWithValue("_Code", Code);
                command.Parameters.AddNull("_DeltaDate");

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new Inventory
                    {
                        InventoryId = rdr["Code"].ToStr(),
                        Code = rdr["Code"].ToStr(),
                        Name = rdr["Name"].ToStr(),
                        Status = (Status)rdr["Status"].ToInt32(),
                        Image = rdr["Image"].ToStr(),
                        CreationDate = rdr["CreateDate"].ToDate(),
                        CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                        LogDetailId = rdr["LogDetailId"].ToStr()
                    };

                    if (rdr["UpdateDate"].ToDate().Year > 1900)
                    {
                        returnValue.ModifyDate = rdr["UpdateDate"].ToDate();
                        returnValue.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
                    }
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
    public List<Inventory> ListInventory(string Code = "", DateTime? DeltaDate = null)
    {
        List<Inventory> returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ItemGroup_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code));
                command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    Inventory element = new()
                    {
                        InventoryId = rdr["Code"].ToStr(),
                        Code = rdr["Code"].ToStr(),
                        Name = rdr["Name"].ToStr(),
                        Status = (Status)rdr["Status"].ToInt32(),
                        Image = rdr["Image"].ToStr(),
                        CreationDate = rdr["CreateDate"].ToDate(),
                        CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                        LogDetailId = rdr["LogDetailId"].ToStr()
                    };

                    if (rdr["UpdateDate"].ToDate().Year > 1900)
                    {
                        element.ModifyDate = rdr["UpdateDate"].ToDate();
                        element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
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
    public ResponseData MergeInventory(Inventory InventoryInfo, User systemOperator, bool Validation)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ItemGroup_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                // command.Parameters.AddNull("_ParentId");
                _ = command.Parameters.AddWithValue("_Code", InventoryInfo.Code);
                _ = command.Parameters.AddWithValue("_Name", InventoryInfo.Name);
                _ = command.Parameters.AddWithValue("_Status", InventoryInfo.Status.ToInt32());
                _ = command.Parameters.AddWithValue("_Image", InventoryInfo.Image);
                command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
                _ = command.Parameters.AddWithValue("_IsValidation", Validation);
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
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }
    #endregion Inventory
    #region SalesOrder

	/// <summary>
	///
	/// </summary>
	public List<SaleOrder> ListSalesOrder(string Id, string SalesOrder, string CustomerCode, DateTime? DeltaDate = null)
	{
		List<SaleOrder> masterList = [];

		SaleOrder element = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SalesOrder_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_Id", Id);
				command.Parameters.AddWithValue("_SalesOrder", SalesOrder);
				command.Parameters.AddWithValue("_CustomerCode", CustomerCode);
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				//while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				//{
				//    element = new SaleOrder()
				//    {
				//        Id = rdr["Id"].ToStr(),
				//        SalesOrder = rdr["Code"].ToStr(),
				//        CustomerName = rdr["CustomerName"].ToStr(),
				//        CustomerCode = rdr["CustomerCode"].ToStr(),
				//        ExpectedDate = rdr["ExpectedDate"] != DBNull.Value ? rdr["ExpectedDate"].ToDate() : (DateTime?)null,
				//        Priority = rdr["Priority"].ToInt32(),
				//        Type = rdr["Type"].ToStr(),
				//        Status = rdr["Status"].ToInt32(),
				//        LogDetailId= rdr["LogDetailId"].ToStr()
				//    };
				//    masterList.Add(element);
				//}
				//rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				List<SaleOrder> detailList = [];
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					element = new SaleOrder
					{
						Id = rdr["Id"].ToStr(),
						SalesOrder = rdr["Code"].ToStr(),
						CustomerName = rdr["CustomerName"].ToStr(),
						CustomerCode = rdr["CustomerCode"].ToStr(),
						LineNo = rdr["LineNo"].ToStr(),
						ItemName = rdr["ItemName"].ToStr(),
						ItemCode = rdr["ItemCode"].ToStr(),
						ExpectedDate = rdr["ExpectedDate"] != DBNull.Value ? rdr["ExpectedDate"].ToDate() : null,
						Quantity = rdr["Quantity"] != DBNull.Value ? rdr["Quantity"].ToDecimal() : null,
						UnitMeasure = rdr["Unit"].ToStr(),
						Priority = rdr["Priority"].ToInt32(),
						Status = rdr["Status"].ToInt32(),
						WarehouseCode = rdr["WarehouseCode"].ToStr(),
						Type = rdr["Type"].ToStr()
					};
					detailList.Add(element);
				}

				// se agrega detalle a maestro
				masterList = [.. detailList
						.GroupBy(static x => new
						{
							x.Id,
							x.SalesOrder,
							x.CustomerCode,
							x.CustomerName,
							x.ExpectedDate,
							x.Type,
							x.Status,
							x.LogDetailId
						})
						.Select(static x => new SaleOrder
						{
							Id = x.Key.Id,
							SalesOrder = x.Key.SalesOrder,
							CustomerCode = x.Key.CustomerCode,
							CustomerName = x.Key.CustomerName,
							ExpectedDate = x.Key.ExpectedDate,
							Type = x.Key.Type,
							Status = x.Key.Status,
							LogDetailId = x.Key.LogDetailId,
							Detail = [.. x]
						})];
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				logger.Error(ex);
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return masterList;
	}
	#endregion SalesOrder
}