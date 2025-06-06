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

public class ProductionLinesRepo : IProductionLinesRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public ProductionLinesRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    /// <summary>
	///
	/// </summary>
	public List<ProductionLine> ListProductionLines(DateTime? DeltaDate = null)
	{
		List<ProductionLine> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ProductionLine_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddNull("_ProductionLineCode");
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					if (returnValue.IsNull())
					{
						returnValue = [];
					}

					ProductionLine element = new()
					{
						Id = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						AssetTypeCode = rdr["AssetTypeCode"].ToStr(),
						Description = rdr["Name"].ToStr(),
						WorkingTime = rdr["WorkingTime"].ToInt32(),
						Image = rdr["Picture"].ToStr(),
						Icon = rdr["Icon"].ToStr(),
						Location = rdr["Location"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						ParentId = rdr["ParentCode"].ToStr(),
						ParentCode = rdr["ParentCode"].ToStr(),
						ParentAssetTypeCode = rdr["AssetParentTypeCode"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						LogDetailId = rdr["LogDetailId"].ToStr()
					};

					if (rdr["UpdateDate"].ToDate().Year >= 1900)
					{
						element.ModifyDate = rdr["UpdateDate"].ToDate();
						element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
					}

					returnValue.Add(element);
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
	public ProductionLine GetProductionLine(string Code)
	{
		ProductionLine returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ProductionLine_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_ProductionLineCode", Code);
				command.Parameters.AddNull("_DeltaDate");
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ProductionLine
					{
						Id = rdr["Code"].ToStr(),
						ParentId = rdr["ParentCode"].ToStr(),
						Description = rdr["Name"].ToStr(),
						Code = rdr["Code"].ToStr(),
						WorkingTime = rdr["WorkingTime"].ToInt32(),
						Image = rdr["Picture"].ToStr(),
						Icon = rdr["Icon"].ToStr(),
						Location = rdr["Location"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						ParentCode = rdr["ParentCode"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						LogDetailId = rdr["LogDetailId"].ToStr()
					};

					if (rdr["UpdateDate"].ToDate().Year >= 1900)
					{
						returnValue.ModifyDate = rdr["UpdateDate"].ToDate();
						returnValue.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
					}
				}

				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					DeviceLink element = new()
					{
						Id = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate()
					};
					if (!string.IsNullOrEmpty(rdr["Status"].ToStr()))
					{
						switch (rdr["Status"].ToStr().ToLowerInvariant())
						{
							case "active":
								element.Status = Status.Active;
								break;

							case "disabled":
								element.Status = Status.Disabled;
								break;

							case "deleted":
								element.Status = Status.Deleted;
								break;
						}
					}
					else
					{
						element.Status = Status.Active;
					}
					(returnValue.Devices ??= []).Add(element);
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
	public ResponseData CreateProductionLine(ProductionLine productionLineInfo, User systemOperator, bool Validation, string Level)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ProductionLine_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddWithValue("_Level", Level);
				command.Parameters.AddWithValue("_Code", productionLineInfo.Code);
				command.Parameters.AddCondition("_Name", productionLineInfo?.Description, !string.IsNullOrEmpty(productionLineInfo?.Description), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Description"));
				command.Parameters.AddWithValue("_Status", productionLineInfo?.Status.ToInt32());
				command.Parameters.AddWithValue("_Icon", productionLineInfo?.Icon);
				command.Parameters.AddCondition("_Picture", productionLineInfo?.Image, !string.IsNullOrEmpty(productionLineInfo?.Image));
				command.Parameters.AddWithValue("_WorkingTime", productionLineInfo?.WorkingTime);
				command.Parameters.AddCondition("_Location", productionLineInfo?.Location, !string.IsNullOrEmpty(productionLineInfo?.Location));
				command.Parameters.AddCondition("_Operator", systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Employee"));
				command.Parameters.AddWithValue("_ParentCode", productionLineInfo.ParentCode);

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
	public bool DeleteProductionLine(ProductionLine productionLineInfo, User SystemOperator)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ProductionLine_DEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Operator", SystemOperator?.Id, SystemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_ProductionLineId", productionLineInfo?.Id, !string.IsNullOrEmpty(productionLineInfo?.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Production Line Id"));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = true;
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
	public bool Integrator_MergeProductionLine_JSON(string Json, User SystemOperator)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ProductionLine_BLK", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Operator", SystemOperator?.Id, SystemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_JSON", Json, !string.IsNullOrEmpty(Json));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = true;
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