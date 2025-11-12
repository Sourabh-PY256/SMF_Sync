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

				// Convert Direction from INT to ENUM string (1=Issue, 2=Return, 3=Scrap)
				string directionStr = OrderMaterialInfo.Direction switch
				{
					1 => "Issue",
					2 => "Return",
					3 => "Scrap",
					_ => "Issue" // Default to Issue
				};
				command.Parameters.AddWithValue("_Direction", directionStr);
				command.Parameters.AddWithValue("_EmployeeId", OrderMaterialInfo.EmployeeId);
				command.Parameters.AddWithValue("_DocCode", OrderMaterialInfo.DocCode);
				command.Parameters.AddWithValue("_Comments", OrderMaterialInfo.Comments);
				command.Parameters.AddWithValue("_DocDate", OrderMaterialInfo.DocDate);
				command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_Details", OrderMaterialInfo.DetailToJSON, OrderMaterialInfo.Details?.Count > 0);

				// Convert Origin from IntegrationSource enum to database ENUM string (SF, ERP, APS)
				string originStr = intSrc switch
				{
					IntegrationSource.ERP => "ERP",
					IntegrationSource.APS => "APS",
					_ => "SF" // Default to SF
				};
				command.Parameters.AddWithValue("_Origin", originStr);
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
// 	public async Task<List<OrderTransactionMaterial>> ListOrderTransactions(CancellationToken cancel = default)
// {
//     List<OrderTransactionMaterial> returnValue = null;

//     await using EWP_Connection connection = new(ConnectionString);
//     await using (connection.ConfigureAwait(false))
//     {
//         await connection.OpenAsync(cancel).ConfigureAwait(false);

//         await using EWP_Command command = new("SP_sf_order_transactions_SEL", connection)
//         {
//             CommandType = CommandType.StoredProcedure
//         };

//         await using (command.ConfigureAwait(false))
//         {
//             command.Parameters.Clear();
//             command.Parameters.AddWithValue("_ExternalId", string.Empty); // filter for empty externalId

//             try
//             {
//                 MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);
//                 await using (rdr.ConfigureAwait(false))
//                 {
//                     // --- First result set: Materials ---
//                     var materials = new Dictionary<string, OrderTransactionMaterial>();

//                     while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
//                     {
//                         var material = new OrderTransactionMaterial
//                         {
//                             Id = rdr["Id"].ToStr(),
//                             OrderCode = rdr["OrderCode"].ToStr(),
//                             OperationNo = rdr["OperationNo"].ToStr(),
//                             Direction = rdr["Direction"].ToInt32(),
//                             ExternalId = rdr["ExternalId"].ToStr(),
//                             ExternalDate = rdr["ExternalDate"].ToDateNullable(),
//                             Comments = rdr["Comments"].ToStr(),
//                             CreateDate = rdr["CreateDate"].ToDate(),
//                             CreateUser = rdr["CreateUser"].ToStr(),
//                             CreateEmployee = rdr["CreateEmployee"].ToStr(),
//                             Origin = rdr["Origin"].ToStr(),
//                             Details = new()
//                         };

//                         materials[material.Id] = material;
//                     }

//                     // --- Move to second result set: Details ---
//                     if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
//                     {
//                         while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
//                         {
//                             var detail = new OrderTransactionMaterialDetail
//                             {
//                                 TransactionId = rdr["TransactionId"].ToStr(),
//                                 MachineCode = rdr["MachineCode"].ToStr(),
//                                 OriginalItemCode = rdr["OriginalItemCode"].ToStr(),
//                                 ItemCode = rdr["ItemCode"].ToStr(),
//                                 LineNo = rdr["LineNo"].ToStr(),
//                                 Quantity = rdr["Quantity"].ToDouble(),
//                                 LotNumber = rdr["LotNumber"].ToStr(),
//                                 Pallet = rdr["Pallet"].ToStr(),
//                                 BinLocationCode = rdr["BinLocationCode"].ToStr(),
//                                 InventoryStatusCode = rdr["InventoryStatusCode"].ToStr(),
//                                 ExpDate = rdr["ExpDate"].ToDateNullable(),
//                                 AllocationOrderCode = rdr["AllocationOrderCode"].ToStr(),
//                                 WarehouseCode = rdr["WarehouseCode"].ToStr(),
//                                 ScrapTypeCode = rdr["ScrapTypeCode"].ToStr(),
//                                 Comments = rdr["Comments"].ToStr()
//                             };

//                             if (materials.TryGetValue(detail.TransactionId, out var parent))
//                             {
//                                 parent.Details.Add(detail);
//                             }
//                         }
//                     }

//                     returnValue = materials.Values.ToList();
//                 }
//             }
//             catch (Exception ex)
//             {
//                 //logger?.Error(ex);
//                 throw;
//             }
//             finally
//             {
//                 await connection.CloseAsync().ConfigureAwait(false);
//             }

//             return returnValue;
//         }
//     }
// }

	public async Task<List<OrderTransactionMaterial>> GetOrderTransactionMaterialByTransactionId(string transactionId, CancellationToken cancel = default)
	{
		List<OrderTransactionMaterial> returnValue = [];

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancel).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_OrderTransactionMaterial_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_TransactionId", transactionId);

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// --- First result set: Materials ---
						var materials = new Dictionary<string, OrderTransactionMaterial>();

						while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
						{
							// Map Direction from ENUM string to INT (Issue=1, Return=2, Scrap=3)
							string directionStr = rdr["Direction"].ToStr();
							int directionInt = directionStr switch
							{
								"Issue" => 1,
								"Return" => 2,
								"Scrap" => 3,
								_ => 1 // Default to Issue
							};

							var material = new OrderTransactionMaterial
							{
								TransactionId = rdr["TransactionId"].ToStr(),
								OrderId = rdr["OrderCode"].ToStr(),
								OperationId = rdr["OperationNo"].ToStr(),
								Direction = directionInt,
								EmployeeId = rdr["EmployeeId"].ToStr(),
								DocCode = rdr["DocCode"].ToStr(),
								Comments = rdr["Comments"].ToStr(),
								DocDate = rdr["DocDate"].ToDate(),
								LogDate = rdr["LogDate"].ToDate(),
								Operator = rdr["UserId"].ToInt32(),
								ExternalId = rdr["ExternalId"].ToStr(),
								Details = []
							};

							materials[material.TransactionId] = material;
						}

						// --- Move to second result set: Details ---
						if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
						{
							while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
							{
								var detail = new OrderTransactionMaterialDetail
								{
									TransactionId = rdr["TransactionId"].ToStr(),
									MachineId = rdr["MachineCode"].ToStr(),
									OriginalItemId = rdr["OriginalItemCode"].ToStr(),
									ItemId = rdr["ItemCode"].ToStr(),
									LineId = rdr["LineNo"].ToStr(),
									Quantity = rdr["Quantity"].ToDecimal(),
									LotNumber = rdr["LotNumber"].ToStr(),
									Pallet = rdr["Pallet"].ToStr(),
									LocationCode = rdr["BinLocationCode"].ToStr(),
									InventoryStatus = rdr["InventoryStatusCode"].ToStr(),
									ExpDate = rdr["ExpDate"].ToDate(),
									OrderId = rdr["AllocationOrderCode"].ToStr(),
									WarehouseCode = rdr["WarehouseCode"].ToStr(),
									ScrapTypeCode = rdr["ScrapTypeCode"].ToStr(),
									Comments = rdr["DetailComments"].ToStr(),
									Type = string.Empty // Type column doesn't exist in database
								};

								if (materials.TryGetValue(detail.TransactionId, out var parent))
								{
									parent.Details.Add(detail);
								}
							}
						}

						returnValue = materials.Values.ToList();
					}
				}
				catch
				{
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

	/// <summary>
	/// Gets all order transaction materials where ExternalId is null or empty
	/// </summary>
	public async Task<List<OrderTransactionMaterial>> GetOrderTransactionMaterialWithoutExternalId(CancellationToken cancel = default)
	{
		List<OrderTransactionMaterial> returnValue = [];

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancel).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_OrderTransactionMaterial_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_TransactionId", string.Empty); // Empty to get all
				command.Parameters.AddWithValue("_ExternalId", string.Empty); // Filter for empty ExternalId

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// --- First result set: Materials ---
						var materials = new Dictionary<string, OrderTransactionMaterial>();

						while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
						{
							// Map Direction from ENUM string to INT (Issue=1, Return=2, Scrap=3)
							string directionStr = rdr["Direction"].ToStr();
							int directionInt = directionStr switch
							{
								"Issue" => 1,
								"Return" => 2,
								"Scrap" => 3,
								_ => 1 // Default to Issue
							};

							var material = new OrderTransactionMaterial
							{
								TransactionId = rdr["TransactionId"].ToStr(),
								OrderId = rdr["OrderCode"].ToStr(),
								OperationId = rdr["OperationNo"].ToStr(),
								Direction = directionInt,
								EmployeeId = rdr["EmployeeId"].ToStr(),
								DocCode = rdr["DocCode"].ToStr(),
								Comments = rdr["Comments"].ToStr(),
								DocDate = rdr["DocDate"].ToDate(),
								LogDate = rdr["LogDate"].ToDate(),
								Operator = rdr["UserId"].ToInt32(),
								ExternalId = rdr["ExternalId"].ToStr(),
								Details = []
							};

							materials[material.TransactionId] = material;
						}

						// --- Move to second result set: Details ---
						if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
						{
							while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
							{
								var detail = new OrderTransactionMaterialDetail
								{
									TransactionId = rdr["TransactionId"].ToStr(),
									MachineId = rdr["MachineCode"].ToStr(),
									OriginalItemId = rdr["OriginalItemCode"].ToStr(),
									ItemId = rdr["ItemCode"].ToStr(),
									LineId = rdr["LineNo"].ToStr(),
									Quantity = rdr["Quantity"].ToDecimal(),
									LotNumber = rdr["LotNumber"].ToStr(),
									Pallet = rdr["Pallet"].ToStr(),
									LocationCode = rdr["BinLocationCode"].ToStr(),
									InventoryStatus = rdr["InventoryStatusCode"].ToStr(),
									ExpDate = rdr["ExpDate"].ToDate(),
									OrderId = rdr["AllocationOrderCode"].ToStr(),
									WarehouseCode = rdr["WarehouseCode"].ToStr(),
									ScrapTypeCode = rdr["ScrapTypeCode"].ToStr(),
									Comments = rdr["DetailComments"].ToStr(),
									Type = string.Empty // Type column doesn't exist in database
								};

								if (materials.TryGetValue(detail.TransactionId, out var parent))
								{
									parent.Details.Add(detail);
								}
							}
						}

						returnValue = materials.Values.ToList();
					}
				}
				catch
				{
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

	/// <summary>
	/// Updates the ExternalId for a specific order transaction material
	/// </summary>
	public async Task<bool> UpdateOrderTransactionMaterialExternalId(string transactionId, string externalId, User systemOperator, CancellationToken cancel = default)
	{
		bool returnValue = false;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancel).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_OrderTransactionMaterial_UPD", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_TransactionId", transactionId);
				command.Parameters.AddWithValue("_ExternalId", externalId);
				command.Parameters.AddWithValue("_User", systemOperator.Id);

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
						{
							returnValue = rdr["IsSuccess"].ToInt32().ToBool();
						}
					}
				}
				catch
				{
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
    #endregion OrderTransactionMaterialRepo
}