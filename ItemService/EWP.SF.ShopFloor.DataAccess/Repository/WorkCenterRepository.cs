using System.Data;
using System.Globalization;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.ShopFloor.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.ShopFloor.DataAccess;

public class WorkCenterRepository : IWorkCenterRepository
{
	
	private readonly string ConnectionString;
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");


	public WorkCenterRepository(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
	}
	public async Task<List<WorkCenter>> ListWorkCenter(DateTime? DeltaDate = null)
	{
		List<WorkCenter> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_WorkCenter_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddNull("_WorkCenterCode");
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					WorkCenter element = new()
					{
						ParentCode = rdr["ParentCode"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Icon = rdr["Icon"].ToStr(),
						Id = rdr["Code"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						Status = (Status)rdr["Status"].ToInt32(),
						Image = rdr["Picture"].ToStr(),
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

	public async Task<WorkCenter> GetWorkCenter(string WorkCenterCode)
	{
		WorkCenter returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_WorkCenter_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_WorkCenterCode", WorkCenterCode);
				command.Parameters.AddNull("_DeltaDate");
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					WorkCenter element = new()
					{
						ParentCode = rdr["ParentCode"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Id = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Icon = rdr["Icon"].ToStr(),

						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						Status = (Status)rdr["Status"].ToInt32(),
						Children = [],
						Image = rdr["Picture"].ToStr(),
						LogDetailId = rdr["LogDetailId"].ToStr()
					};

					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						element.ModifyDate = rdr["UpdateDate"].ToDate();
						element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
					}

					returnValue = element;
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					if (returnValue is not null)
					{
						ProductionLine element = new()
						{
							ParentCode = rdr["ParentCode"].ToStr(),
							AssetTypeCode = rdr["AssetTypeCode"].ToStr(),
							Code = rdr["Code"].ToStr(),
							Description = rdr["Name"].ToStr(),
							Icon = rdr["Icon"].ToStr(),

							CreationDate = rdr["CreateDate"].ToDate(),
							CreatedBy = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};

						//if (rdr["Status"].ToStr() != "")
						//{
						//    switch (rdr["Status"].ToStr().ToLower())
						//    {
						//        case "active":
						//            {
						//                element.Status = Status.Active;
						//                break;
						//            }
						//        case "disabled":
						//            {
						//                element.Status = Status.Disabled;
						//                break;
						//            }
						//        case "deleted":
						//            {
						//                element.Status = Status.Deleted;
						//                break;
						//            }
						//    }
						//}
						//else
						//{
						//    element.Status = Status.Active;
						//}

						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.ModifyDate = rdr["UpdateDate"].ToDate();
							element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
						}
						returnValue.Children.Add(element);
					}
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

	public async Task<ResponseData> CreateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator, bool Validation, string Level)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_WorkCenter_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", WorkCenterInfo.Code);
				_ = command.Parameters.AddWithValue("_Name", WorkCenterInfo.Name);
				_ = command.Parameters.AddWithValue("_Icon", WorkCenterInfo.Icon);
				_ = command.Parameters.AddWithValue("_Status", WorkCenterInfo.Status.ToInt32());
				command.Parameters.AddCondition("_Picture", WorkCenterInfo.Image, !string.IsNullOrEmpty(WorkCenterInfo.Image));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				_ = command.Parameters.AddWithValue("_IsValidation", Validation);
				_ = command.Parameters.AddWithValue("_Level", Level);
				_ = command.Parameters.AddWithValue("_ParentCode", WorkCenterInfo.ParentCode);
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

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

	public async Task<bool> UpdateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_WorkCenter_UPD", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_WorkCenterCode", () => WorkCenterInfo.Id, !string.IsNullOrEmpty(WorkCenterInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Work Center Id"));
				_ = command.Parameters.AddWithValue("_ParentCode", WorkCenterInfo.ParentId);
				_ = command.Parameters.AddWithValue("_Code", WorkCenterInfo.Code);
				_ = command.Parameters.AddWithValue("_Name", WorkCenterInfo.Name);
				_ = command.Parameters.AddWithValue("_Icon", WorkCenterInfo.Icon);

				_ = command.Parameters.AddWithValue("_Status", WorkCenterInfo.Status.ToInt32());
				command.Parameters.AddCondition("_Picture", WorkCenterInfo.Image, !string.IsNullOrEmpty(WorkCenterInfo.Image));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				_ = command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public async Task<bool> DeleteWorkCenter(WorkCenter WorkCenterInfo, User systemOperator)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_WorkCenter_DEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_Code", () => WorkCenterInfo.Code, !string.IsNullOrEmpty(WorkCenterInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Work Center Id"));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				_ = command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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
