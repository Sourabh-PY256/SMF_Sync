using System.Data;
using System.Globalization;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;
using EWP.SF.Common.ResponseModels;
using System.Data;
using System.Globalization;

using EWP.SF.ConnectionModule;
using EWP.SF.Helper;


using Newtonsoft.Json;

namespace EWP.SF.Item.DataAccess;

public class DataSyncRepository : IDataSyncRepository
{
	private readonly string ConnectionString;
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");


	public DataSyncRepository(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
	}

	public async Task DatasyncTempServiceLogAsync(string EntityCode, string mode, string Exception = "", CancellationToken cancellationToken = default)
	{
		await using EWP_Connection connection = new(ConnectionString);

		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DatasyncPolling_INS", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.AddCondition("_EntityCode", EntityCode, !string.IsNullOrEmpty(EntityCode), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "EntityCode"));
				_ = command.Parameters.AddWithValue("_Mode", mode);
				command.Parameters.AddCondition("_Exception", Exception, !string.IsNullOrEmpty(Exception));

				try
				{
					_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				}
				catch
				{
					// Log the exception or handle it as required.
					throw;
				}
			}
		}
	}

	public List<DataSyncCatalog> ListDataSyncErp(DataSyncCatalogFilter SearchFilter)
	{
		List<DataSyncCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", SearchFilter.Code);
				_ = command.Parameters.AddWithValue("_LanguageCode", SearchFilter.LanguageCode);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncCatalog element = new()
						{
							Code = rdr["Code"].ToStr(),
							Name = rdr["Name"].ToStr(),
							Icon = rdr["Icon"].ToStr(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncCatalog> ListDataSyncErpVersion(DataSyncCatalogFilter SearchFilter)
	{
		List<DataSyncCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_Version_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", SearchFilter.Code);
				_ = command.Parameters.AddWithValue("_ParentCode", SearchFilter.ParentCode);
				_ = command.Parameters.AddWithValue("_LanguageCode", SearchFilter.LanguageCode);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncCatalog element = new()
						{
							Code = rdr["Code"].ToStr(),
							Name = rdr["Name"].ToStr(),
							Icon = rdr["Icon"].ToStr(),
							ParentCode = rdr["ParentCode"].ToStr(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncCatalog> ListDataSyncErpDatabase(DataSyncCatalogFilter SearchFilter)
	{
		List<DataSyncCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_Database_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", SearchFilter.Code);
				_ = command.Parameters.AddWithValue("_ParentCode", SearchFilter.ParentCode);
				_ = command.Parameters.AddWithValue("_LanguageCode", SearchFilter.LanguageCode);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncCatalog element = new()
						{
							Code = rdr["Code"].ToStr(),
							Name = rdr["Name"].ToStr(),
							Icon = rdr["Icon"].ToStr(),
							ParentCode = rdr["ParentCode"].ToStr(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncCatalog> ListDataSyncErpDatabaseVersion(DataSyncCatalogFilter SearchFilter)
	{
		List<DataSyncCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_Database_Version_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", SearchFilter.Code);
				_ = command.Parameters.AddWithValue("_ParentCode", SearchFilter.ParentCode);
				_ = command.Parameters.AddWithValue("_LanguageCode", SearchFilter.LanguageCode);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncCatalog element = new()
						{
							Code = rdr["Code"].ToStr(),
							Name = rdr["Name"].ToStr(),
							Icon = rdr["Icon"].ToStr(),
							ParentCode = rdr["ParentCode"].ToStr(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncCatalog> ListDataSyncErpManufacturing(DataSyncCatalogFilter SearchFilter)
	{
		List<DataSyncCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_Manufacturing_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", SearchFilter.Code);
				_ = command.Parameters.AddWithValue("_ParentCode", SearchFilter.ParentCode);
				_ = command.Parameters.AddWithValue("_LanguageCode", SearchFilter.LanguageCode);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncCatalog element = new()
						{
							Code = rdr["Code"].ToStr(),
							Name = rdr["Name"].ToStr(),
							Icon = rdr["Icon"].ToStr(),
							ParentCode = rdr["ParentCode"].ToStr(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncCatalog> ListDataSyncErpManufacturingVersion(DataSyncCatalogFilter SearchFilter)
	{
		List<DataSyncCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_Manufacturing_Version_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", SearchFilter.Code);
				_ = command.Parameters.AddWithValue("_ParentCode", SearchFilter.ParentCode);
				_ = command.Parameters.AddWithValue("_LanguageCode", SearchFilter.LanguageCode);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncCatalog element = new()
						{
							Code = rdr["Code"].ToStr(),
							Name = rdr["Name"].ToStr(),
							Icon = rdr["Icon"].ToStr(),
							ParentCode = rdr["ParentCode"].ToStr(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncCatalog> ListDataSyncInstanceCategory(DataSyncCatalogFilter SearchFilter)
	{
		List<DataSyncCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_Instance_Category_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", SearchFilter.Code);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncCatalog element = new()
						{
							Code = rdr["Code"].ToStr(),
							Name = rdr["Name"].ToStr(),
							Icon = rdr["Icon"].ToStr(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncErp> ListDataSyncERP(string Id = "", EnableType GetInstances = EnableType.Yes)
	{
		List<DataSyncErp> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Erp_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Id", Id, !string.IsNullOrEmpty(Id));
				_ = command.Parameters.AddWithValue("_GetInstances", GetInstances);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncErp element = new()
						{
							Id = rdr["Id"].ToStr(),
							ErpCode = rdr["ErpCode"].ToStr(),
							Erp = rdr["Erp"].ToStr(),
							ErpVersionCode = rdr["ErpVersionCode"].ToStr(),
							ErpVersion = rdr["ErpVersion"].ToStr(),
							DbCode = rdr["DbCode"].ToStr(),
							Db = rdr["Db"].ToStr(),
							DbVersionCode = rdr["DbVersionCode"].ToStr(),
							DbVersion = rdr["DbVersion"].ToStr(),
							ManufacturingCode = rdr["ManufacturingCode"].ToStr(),
							Manufacturing = rdr["Manufacturing"].ToStr(),
							ManufacturingVersionCode = rdr["ManufacturingVersionCode"].ToStr(),
							ManufacturingVersion = rdr["ManufacturingVersion"].ToStr(),
							BaseUrl = rdr["BaseUrl"].ToStr(),
							TokenRequestJson = rdr["TokenRequestJson"].ToStr(),
							TokenRequestPath = rdr["TokenRequestPath"].ToStr(),
							TokenRequestMapSchema = rdr["TokenRequestMapSchema"].ToStr(),
							TokenRequestResultProp = rdr["TokenRequestResultProp"].ToStr(),
							RequiresTokenRenewal = (EnableType)rdr["RequiresTokenRenewal"].ToInt32(),
							TokenRenewalMapSchema = rdr["TokenRenewalMapSchema"].ToStr(),
							RequiredHeaders = rdr["RequiredHeaders"].ToStr(),
							DateTimeFormat = (DateTimeFormatType)rdr["DateTimeFormat"].ToInt32(),
							TimeZone = rdr["TimeZone"].ToStr(),
							ReprocessingTime = rdr["ReprocessingTime"].ToInt32(),
							MaxReprocessingTime = rdr["MaxReprocessingTime"].ToInt32(),
							ReprocessingTimeOffset = rdr["ReprocessingOffsetSeconds"].ToInt32(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public async Task SaveDatasyncMappingLog(DataSyncErpMapping DataSyncInfo, User SystemOperator, CancellationToken cancellationToken = default)
	{
		await using EWP_Connection connection = new(ConnectionStringLogs);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DATASYNC_MAPPING_LOG_INS", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				_ = command.Parameters.AddWithValue("_Database", Database);
				_ = command.Parameters.AddWithValue("_EntityCode", DataSyncInfo.EntityId);
				_ = command.Parameters.AddWithValue("_JSON", JsonConvert.SerializeObject(DataSyncInfo));
				_ = command.Parameters.AddWithValue("_UserId", SystemOperator.Id);
				_ = command.Parameters.AddWithValue("_EmployeeCode", SystemOperator.EmployeeId);

				try
				{
					_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				}
				catch
				{
					throw;
				}
				finally
				{
					await connection.CloseAsync().ConfigureAwait(false);
				}
			}
		}
	}

	public async Task SaveDatasyncLog(DataSyncErp DataSyncInfo, User SystemOperator, CancellationToken cancellationToken = default)
	{
		await using EWP_Connection connection = new(ConnectionStringLogs);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_ERP_Log_INS", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				_ = command.Parameters.AddWithValue("_Database", Database);
				_ = command.Parameters.AddWithValue("_ErpCode", DataSyncInfo.ErpCode);
				_ = command.Parameters.AddWithValue("_JSON", JsonConvert.SerializeObject(DataSyncInfo));
				_ = command.Parameters.AddWithValue("_UserId", SystemOperator.Id);
				_ = command.Parameters.AddWithValue("_EmployeeCode", SystemOperator.EmployeeId);

				try
				{
					_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				}
				catch
				{
					throw;
				}
				finally
				{
					await connection.CloseAsync().ConfigureAwait(false);
				}
			}
		}
	}
	public DataSyncErp MergeDataSyncERP(DataSyncErp DataSyncInfo, User SystemOperator)
	{
		DataSyncErp returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Erp_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Id", DataSyncInfo.Id);
				_ = command.Parameters.AddWithValue("_ErpCode", DataSyncInfo.ErpCode);
				_ = command.Parameters.AddWithValue("_ErpVersionCode", DataSyncInfo.ErpVersionCode);
				_ = command.Parameters.AddWithValue("_DbCode", DataSyncInfo.DbCode);
				_ = command.Parameters.AddWithValue("_DbVersionCode", DataSyncInfo.DbVersionCode);
				_ = command.Parameters.AddWithValue("_ManufacturingCode", DataSyncInfo.ManufacturingCode);
				_ = command.Parameters.AddWithValue("_ManufacturingVersionCode", DataSyncInfo.ManufacturingVersionCode);
				_ = command.Parameters.AddWithValue("_BaseUrl", DataSyncInfo.BaseUrl);
				_ = command.Parameters.AddWithValue("_TokenRequestJson", DataSyncInfo.TokenRequestJson);
				_ = command.Parameters.AddWithValue("_TokenRequestPath", DataSyncInfo.TokenRequestPath);
				_ = command.Parameters.AddWithValue("_TokenRequestMapSchema", DataSyncInfo.TokenRequestMapSchema);
				_ = command.Parameters.AddWithValue("_TokenRequestResultProp", DataSyncInfo.TokenRequestResultProp);
				_ = command.Parameters.AddWithValue("_RequiresTokenRenewal", DataSyncInfo.RequiresTokenRenewal);
				_ = command.Parameters.AddWithValue("_TokenRenewalMapSchema", DataSyncInfo.TokenRenewalMapSchema);
				_ = command.Parameters.AddWithValue("_RequiredHeaders", DataSyncInfo.RequiredHeaders);
				_ = command.Parameters.AddWithValue("_DateTimeFormat", DataSyncInfo.DateTimeFormat);
				_ = command.Parameters.AddWithValue("_TimeZone", DataSyncInfo.TimeZone);
				_ = command.Parameters.AddWithValue("_ReprocessingTime", DataSyncInfo.ReprocessingTime);
				_ = command.Parameters.AddWithValue("_MaxReprocessingTime", DataSyncInfo.MaxReprocessingTime);
				_ = command.Parameters.AddWithValue("_ReprocessingOffset", DataSyncInfo.ReprocessingTimeOffset);
				_ = command.Parameters.AddWithValue("_Status", DataSyncInfo.Status.ToInt32());
				command.Parameters.AddCondition("_User", () => SystemOperator.Id, SystemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", SystemOperator.EmployeeId, !string.IsNullOrEmpty(SystemOperator.EmployeeId));

				if (DataSyncInfo.Instances.Count > 0)
				{
					string instances = JsonConvert.SerializeObject(DataSyncInfo.Instances);
					if (!string.IsNullOrEmpty(instances))
					{
						_ = command.Parameters.AddWithValue("_Instances", instances);
					}
				}

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new DataSyncErp
					{
						Id = rdr["Id"].ToStr(),
						ErpCode = rdr["ErpCode"].ToStr(),
						Erp = rdr["Erp"].ToStr(),
						ErpVersionCode = rdr["ErpVersionCode"].ToStr(),
						ErpVersion = rdr["ErpVersion"].ToStr(),
						DbCode = rdr["DbCode"].ToStr(),
						Db = rdr["Db"].ToStr(),
						DbVersionCode = rdr["DbVersionCode"].ToStr(),
						DbVersion = rdr["DbVersion"].ToStr(),
						ManufacturingCode = rdr["ManufacturingCode"].ToStr(),
						Manufacturing = rdr["Manufacturing"].ToStr(),
						ManufacturingVersionCode = rdr["ManufacturingVersionCode"].ToStr(),
						ManufacturingVersion = rdr["ManufacturingVersion"].ToStr(),
						BaseUrl = rdr["BaseUrl"].ToStr(),
						TokenRequestJson = rdr["TokenRequestJson"].ToStr(),
						TokenRequestPath = rdr["TokenRequestPath"].ToStr(),
						TokenRequestMapSchema = rdr["TokenRequestMapSchema"].ToStr(),
						TokenRequestResultProp = rdr["TokenRequestResultProp"].ToStr(),
						RequiresTokenRenewal = (EnableType)rdr["RequiresTokenRenewal"].ToInt32(),
						TokenRenewalMapSchema = rdr["TokenRenewalMapSchema"].ToStr(),
						RequiredHeaders = rdr["RequiredHeaders"].ToStr(),
						DateTimeFormat = (DateTimeFormatType)rdr["DateTimeFormat"].ToInt32(),
						TimeZone = rdr["DateTimeFormat"].ToStr(),
						ReprocessingTime = rdr["ReprocessingTime"].ToInt32(),
						MaxReprocessingTime = rdr["MaxReprocessingTime"].ToInt32(),
						ReprocessingTimeOffset = rdr["ReprocessingOffsetSeconds"].ToInt32(),
						CreateDate = rdr["CreateDate"].ToDate(),
						CreateUser = new User(rdr["CreateUser"].ToInt32()),
						Status = (Status)rdr["Status"].ToInt32()
					};
					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						returnValue.UpdateDate = rdr["UpdateDate"].ToDate();
						returnValue.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
					}
					if (rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult() && returnValue is not null)
					{
						returnValue.Instances = [];
						while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
						{
							DataSyncService element = new()
							{
								Id = rdr["Id"].ToStr(),
								ErpDataId = rdr["ErpDataId"].ToStr(),
								ErpData = new DataSyncErp
								{
									Id = rdr["ErpDataId"].ToStr(),
									ErpCode = rdr["ErpCode"].ToStr(),
									Erp = rdr["Erp"].ToStr(),
									ErpVersionCode = rdr["ErpVersionCode"].ToStr(),
									ErpVersion = rdr["ErpVersion"].ToStr(),
									DbCode = rdr["DbCode"].ToStr(),
									Db = rdr["Db"].ToStr(),
									DbVersionCode = rdr["DbVersionCode"].ToStr(),
									DbVersion = rdr["DbVersion"].ToStr(),
									ManufacturingCode = rdr["ManufacturingCode"].ToStr(),
									Manufacturing = rdr["Manufacturing"].ToStr(),
									ManufacturingVersionCode = rdr["ManufacturingVersionCode"].ToStr(),
									ManufacturingVersion = rdr["ManufacturingVersion"].ToStr(),
									BaseUrl = rdr["BaseUrl"].ToStr(),
									TokenRequestJson = rdr["TokenRequestJson"].ToStr(),
									TokenRequestPath = rdr["TokenRequestPath"].ToStr(),
									TokenRequestMapSchema = rdr["TokenRequestMapSchema"].ToStr(),
									TokenRequestResultProp = rdr["TokenRequestResultProp"].ToStr(),
									RequiresTokenRenewal = (EnableType)rdr["RequiresTokenRenewal"].ToInt32(),
									TokenRenewalMapSchema = rdr["TokenRenewalMapSchema"].ToStr(),
									RequiredHeaders = rdr["RequiredHeaders"].ToStr(),
									DateTimeFormat = (DateTimeFormatType)rdr["DateTimeFormat"].ToInt32(),
									TimeZone = rdr["DateTimeFormat"].ToStr(),
									ReprocessingTime = rdr["ReprocessingTime"].ToInt32(),
									MaxReprocessingTime = rdr["MaxReprocessingTime"].ToInt32()
								},
								EntityId = rdr["EntityCode"].ToStr(),
								Entity = new Entity
								{
									Id = rdr["EntityCode"].ToStr(),
									Name = rdr["EntityName"].ToStr(),
									Description = rdr["EntityDescription"].ToStr(),
									NameEntityExternal = rdr["EntityClassExternal"].ToStr(),
								},
								EntityCategoryCode = rdr["EntityCategoryCode"].ToStr(),
								ErpMapping = new DataSyncErpMapping
								{
									ErpId = rdr["ErpDataId"].ToStr(),
									Id = rdr["EntityCode"].ToStr(),
									HttpMethod = rdr["HttpMethod"].ToStr(),
									ResponseMapSchema = rdr["ResponseMapSchema"].ToStr(),
									RequestMapSchema = rdr["RequestMapSchema"].ToStr(),
									ExpectedResult = rdr["ExpectedResult"].ToStr(),
									ResultProperty = rdr["ResultProperty"].ToStr(),
									ErrorProperty = rdr["ErrorProperty"].ToStr()
								},
								TokenData = new DataSyncErpAuth
								{
									ErpId = rdr["ErpDataId"].ToStr(),
									Token = rdr["Token"].ToStr(),
									TokenType = rdr["TokenType"].ToStr(),
									ExpirationTime = rdr["ExpirationTime"].ToInt32(),
									ExpirationDate = rdr["ExpirationDate"].ToDate()
								},
								Path = rdr["Path"].ToStr(),
								UrlParams = rdr["UrlParams"].ToStr(),
								SingleRecordParam = rdr["SingleRecordParam"].ToStr(),
								HttpMethod = rdr["HttpMethod"].ToStr(),
								TimeTriggerEnable = (EnableType)rdr["TimeTriggerEnable"].ToInt32(),
								FrequencyMin = rdr["FrequencyMin"].ToInt32(),
								ErpTriggerEnable = (EnableType)rdr["ErpTriggerEnable"].ToInt32(),
								SfTriggerEnable = (EnableType)rdr["SfTriggerEnable"].ToInt32(),
								ManualSyncEnable = (EnableType)rdr["ManualSyncEnable"].ToInt32(),
								DeltaSync = (EnableType)rdr["DeltaSync"].ToInt32(),
								LastExecutionDate = rdr["LastExecutionDate"].ToDate(),
								OffsetMin = rdr["OffsetMin"].ToInt32(),
								RequestTimeoutSecs = rdr["RequestTimeoutSecs"].ToInt32(),
								RequestReprocFrequencySecs = rdr["RequestReprocFrequencySecs"].ToInt32(),
								RevalidationEnable = (EnableType)rdr["RevalidationEnable"].ToInt32(),
								CreationDate = rdr["CreateDate"].ToDate(),
								CreatedBy = new User(rdr["CreateUser"].ToInt32()),
								Status = (ServiceStatus)rdr["Status"].ToInt32(),
								EnableDynamicBody = rdr["EnableDynamicBody"].ToInt32()
							};
							if (rdr["UpdateDate"].ToDate().Year > 1900)
							{
								element.ModifyDate = rdr["UpdateDate"].ToDate();
								element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
							}
							returnValue.Instances.Add(element);
						}
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

	public List<DataSyncService> ListDisabledServices()
	{
		List<DataSyncService> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Entities_Disabled_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncService element = new()
						{
							EntityId = rdr["EntityCode"].ToStr(),
							HttpMethod = rdr["HttpMethod"].ToStr()
						};
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncService> ListDataSyncService(TriggerType Trigger, string Id = "")
	{
		List<DataSyncService> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Entities_Instance_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				if (string.IsNullOrEmpty(Id))
				{
					command.Parameters.AddNull("_Id");
				}
				else
				{
					_ = command.Parameters.AddWithValue("_Id", Id);
				}
				_ = command.Parameters.AddWithValue("_TriggerType", (int)Trigger);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncService element = new()
						{
							Id = rdr["Id"].ToStr(),
							ErpDataId = rdr["ErpDataId"].ToStr(),
							ErpData = new DataSyncErp
							{
								Id = rdr["ErpDataId"].ToStr(),
								ErpCode = rdr["ErpCode"].ToStr(),
								Erp = rdr["Erp"].ToStr(),
								ErpVersionCode = rdr["ErpVersionCode"].ToStr(),
								ErpVersion = rdr["ErpVersion"].ToStr(),
								DbCode = rdr["DbCode"].ToStr(),
								Db = rdr["Db"].ToStr(),
								DbVersionCode = rdr["DbVersionCode"].ToStr(),
								DbVersion = rdr["DbVersion"].ToStr(),
								ManufacturingCode = rdr["ManufacturingCode"].ToStr(),
								Manufacturing = rdr["Manufacturing"].ToStr(),
								ManufacturingVersionCode = rdr["ManufacturingVersionCode"].ToStr(),
								ManufacturingVersion = rdr["ManufacturingVersion"].ToStr(),
								BaseUrl = rdr["BaseUrl"].ToStr(),
								TokenRequestJson = rdr["TokenRequestJson"].ToStr(),
								TokenRequestPath = rdr["TokenRequestPath"].ToStr(),
								TokenRequestMapSchema = rdr["TokenRequestMapSchema"].ToStr(),
								TokenRequestResultProp = rdr["TokenRequestResultProp"].ToStr(),
								RequiresTokenRenewal = (EnableType)rdr["RequiresTokenRenewal"].ToInt32(),
								TokenRenewalMapSchema = rdr["TokenRenewalMapSchema"].ToStr(),
								RequiredHeaders = rdr["RequiredHeaders"].ToStr(),
								DateTimeFormat = (DateTimeFormatType)rdr["DateTimeFormat"].ToInt32(),
								TimeZone = rdr["DateTimeFormat"].ToStr(),
								ReprocessingTime = rdr["ReprocessingTime"].ToInt32(),
								MaxReprocessingTime = rdr["MaxReprocessingTime"].ToInt32()
							},
							EntityId = rdr["EntityCode"].ToStr(),
							Entity = new Entity
							{
								Id = rdr["EntityCode"].ToStr(),
								Name = rdr["EntityName"].ToStr(),
								Description = rdr["EntityDescription"].ToStr(),
								NameEntityExternal = rdr["EntityClassExternal"].ToStr(),
							},
							EntityCategoryCode = rdr["EntityCategoryCode"].ToStr(),
							Path = rdr["Path"].ToStr(),
							UrlParams = rdr["UrlParams"].ToStr(),
							SingleRecordParam = rdr["SingleRecordParam"].ToStr(),
							HttpMethod = rdr["HttpMethod"].ToStr(),
							TimeTriggerEnable = (EnableType)rdr["TimeTriggerEnable"].ToInt32(),
							FrequencyMin = rdr["FrequencyMin"].ToInt32(),
							ErpTriggerEnable = (EnableType)rdr["ErpTriggerEnable"].ToInt32(),
							SfTriggerEnable = (EnableType)rdr["SfTriggerEnable"].ToInt32(),
							ManualSyncEnable = (EnableType)rdr["ManualSyncEnable"].ToInt32(),
							DeltaSync = (EnableType)rdr["DeltaSync"].ToInt32(),
							LastExecutionDate = rdr["LastExecutionDate"].ToDate(),
							OffsetMin = rdr["OffsetMin"].ToInt32(),
							RequestTimeoutSecs = rdr["RequestTimeoutSecs"].ToInt32(),
							RequestReprocFrequencySecs = rdr["RequestReprocFrequencySecs"].ToInt32(),
							CreationDate = rdr["CreateDate"].ToDate(),
							CreatedBy = new User(rdr["CreateUser"].ToInt32()),
							Status = (ServiceStatus)rdr["Status"].ToInt32(),
							EnableDynamicBody = rdr["EnableDynamicBody"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.ModifyDate = rdr["UpdateDate"].ToDate();
							element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
						}
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public DataSyncService MergeDataSyncService(DataSyncService DataSyncInfo, User SystemOperator)
	{
		DataSyncService returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Entities_Instance_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Id", DataSyncInfo.Id);
				_ = command.Parameters.AddWithValue("_ErpDataId", DataSyncInfo.ErpDataId);
				_ = command.Parameters.AddWithValue("_EntityId", DataSyncInfo.EntityId);
				_ = command.Parameters.AddWithValue("_EntityCategoryCode", DataSyncInfo.EntityCategoryCode);
				_ = command.Parameters.AddWithValue("_Path", DataSyncInfo.Path);
				_ = command.Parameters.AddWithValue("_UrlParams", DataSyncInfo.UrlParams);
				_ = command.Parameters.AddWithValue("_SingleRecordParam", DataSyncInfo.SingleRecordParam);
				_ = command.Parameters.AddWithValue("_HttpMethod", DataSyncInfo.HttpMethod);
				_ = command.Parameters.AddWithValue("_TimeTriggerEnable", DataSyncInfo.TimeTriggerEnable);
				_ = command.Parameters.AddWithValue("_FrequencyMin", DataSyncInfo.FrequencyMin);
				_ = command.Parameters.AddWithValue("_ErpTriggerEnable", DataSyncInfo.ErpTriggerEnable);
				_ = command.Parameters.AddWithValue("_SfTriggerEnable", DataSyncInfo.SfTriggerEnable);
				_ = command.Parameters.AddWithValue("_ManualSyncEnable", DataSyncInfo.ManualSyncEnable);
				_ = command.Parameters.AddWithValue("_DeltaSync", DataSyncInfo.DeltaSync);
				// command.Parameters.AddWithValue("_LastExecutionDate", DataSyncInfo.LastExecutionDate);
				command.Parameters.AddNull("_LastExecutionDate");
				_ = command.Parameters.AddWithValue("_OffsetMin", DataSyncInfo.OffsetMin);
				_ = command.Parameters.AddWithValue("_RequestTimeoutSecs", DataSyncInfo.RequestTimeoutSecs);
				_ = command.Parameters.AddWithValue("_RequestReprocFrequencySecs", DataSyncInfo.RequestReprocFrequencySecs);
				_ = command.Parameters.AddWithValue("_RevalidationEnable", DataSyncInfo.RevalidationEnable);
				command.Parameters.AddCondition("_User", () => SystemOperator.Id, SystemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				_ = command.Parameters.AddWithValue("_Status", DataSyncInfo.Status.ToInt32());
				command.Parameters.AddCondition("_OperatorEmployee", SystemOperator.EmployeeId, !string.IsNullOrEmpty(SystemOperator.EmployeeId));
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new DataSyncService
					{
						Id = rdr["Id"].ToStr(),
						ErpDataId = rdr["ErpDataId"].ToStr(),
						ErpData = new DataSyncErp
						{
							Id = rdr["ErpDataId"].ToStr(),
							ErpCode = rdr["ErpCode"].ToStr(),
							Erp = rdr["Erp"].ToStr(),
							ErpVersionCode = rdr["ErpVersionCode"].ToStr(),
							ErpVersion = rdr["ErpVersion"].ToStr(),
							DbCode = rdr["DbCode"].ToStr(),
							Db = rdr["Db"].ToStr(),
							DbVersionCode = rdr["DbVersionCode"].ToStr(),
							DbVersion = rdr["DbVersion"].ToStr(),
							ManufacturingCode = rdr["ManufacturingCode"].ToStr(),
							Manufacturing = rdr["Manufacturing"].ToStr(),
							ManufacturingVersionCode = rdr["ManufacturingVersionCode"].ToStr(),
							ManufacturingVersion = rdr["ManufacturingVersion"].ToStr(),
							BaseUrl = rdr["BaseUrl"].ToStr(),
							TokenRequestJson = rdr["TokenRequestJson"].ToStr(),
							TokenRequestPath = rdr["TokenRequestPath"].ToStr(),
							TokenRequestMapSchema = rdr["TokenRequestMapSchema"].ToStr(),
							TokenRequestResultProp = rdr["TokenRequestResultProp"].ToStr(),
							RequiresTokenRenewal = (EnableType)rdr["RequiresTokenRenewal"].ToInt32(),
							TokenRenewalMapSchema = rdr["TokenRenewalMapSchema"].ToStr(),
							RequiredHeaders = rdr["RequiredHeaders"].ToStr(),
							DateTimeFormat = (DateTimeFormatType)rdr["DateTimeFormat"].ToInt32(),
							TimeZone = rdr["DateTimeFormat"].ToStr()
						},
						EntityId = rdr["EntityId"].ToStr(),
						Entity = new Entity
						{
							Id = rdr["EntityId"].ToStr(),
							Name = rdr["EntityName"].ToStr(),
							Description = rdr["EntityDescription"].ToStr(),
							NameEntityExternal = rdr["EntityClassExternal"].ToStr(),
						},
						Path = rdr["Path"].ToStr(),
						UrlParams = rdr["UrlParams"].ToStr(),
						TimeTriggerEnable = (EnableType)rdr["TimeTriggerEnable"].ToInt32(),
						FrequencyMin = rdr["FrequencyMin"].ToInt32(),
						ErpTriggerEnable = (EnableType)rdr["ErpTriggerEnable"].ToInt32(),
						SfTriggerEnable = (EnableType)rdr["SfTriggerEnable"].ToInt32(),
						LastExecutionDate = rdr["LastExecutionDate"].ToDate(),
						OffsetMin = rdr["OffsetMin"].ToInt32(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						Status = (ServiceStatus)rdr["Status"].ToInt32()
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

	public bool UpdateDataSyncServiceExecution(string Id, DateTime ExecutionDate)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Entities_Instance_Exec_UPD", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Id", Id, !string.IsNullOrEmpty(Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Service Instance Id"));
				_ = command.Parameters.AddWithValue("_ExecutionDate", ExecutionDate);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public bool UpdateDataSyncServiceStatus(string Id, ServiceStatus Status)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Entities_Instance_Status_UPD", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Id", Id, !string.IsNullOrEmpty(Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Service Instance Id"));
				_ = command.Parameters.AddWithValue("_Status", Status);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public async Task<List<DataSyncService>> GetBackgroundService(string backgroundService, string httpMethod, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(backgroundService, nameof(backgroundService));
		ArgumentException.ThrowIfNullOrEmpty(httpMethod, nameof(httpMethod));

		List<DataSyncService> returnValue = null;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_BackgroundService_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Service", backgroundService);
				command.Parameters.AddWithValue("_HttpMethod", httpMethod);

				try
				{
					// Execute reader asynchronously
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (rdr.HasRows)
						{
							// Get ordinals for column indices
							int idOrdinal = rdr.GetOrdinal("Id");
							int erpDataIdOrdinal = rdr.GetOrdinal("ErpDataId");
							int erpCodeOrdinal = rdr.GetOrdinal("ErpCode");
							int erpOrdinal = rdr.GetOrdinal("Erp");
							int erpVersionCodeOrdinal = rdr.GetOrdinal("ErpVersionCode");
							int erpVersionOrdinal = rdr.GetOrdinal("ErpVersion");
							int dbCodeOrdinal = rdr.GetOrdinal("DbCode");
							int dbOrdinal = rdr.GetOrdinal("Db");
							int dbVersionCodeOrdinal = rdr.GetOrdinal("DbVersionCode");
							int dbVersionOrdinal = rdr.GetOrdinal("DbVersion");
							int manufacturingCodeOrdinal = rdr.GetOrdinal("ManufacturingCode");
							int manufacturingOrdinal = rdr.GetOrdinal("Manufacturing");
							int manufacturingVersionCodeOrdinal = rdr.GetOrdinal("ManufacturingVersionCode");
							int manufacturingVersionOrdinal = rdr.GetOrdinal("ManufacturingVersion");
							int baseUrlOrdinal = rdr.GetOrdinal("BaseUrl");
							int tokenRequestJsonOrdinal = rdr.GetOrdinal("TokenRequestJson");
							int tokenRequestPathOrdinal = rdr.GetOrdinal("TokenRequestPath");
							int tokenRequestMapSchemaOrdinal = rdr.GetOrdinal("TokenRequestMapSchema");
							int tokenRequestResultPropOrdinal = rdr.GetOrdinal("TokenRequestResultProp");
							int requiresTokenRenewalOrdinal = rdr.GetOrdinal("RequiresTokenRenewal");
							int tokenRenewalMapSchemaOrdinal = rdr.GetOrdinal("TokenRenewalMapSchema");
							int requiredHeadersOrdinal = rdr.GetOrdinal("RequiredHeaders");
							int dateTimeFormatOrdinal = rdr.GetOrdinal("DateTimeFormat");
							int timeZoneOrdinal = rdr.GetOrdinal("TimeZone");
							int reprocessingTimeOrdinal = rdr.GetOrdinal("ReprocessingTime");
							int maxReprocessingTimeOrdinal = rdr.GetOrdinal("MaxReprocessingTime");
							int entityCodeOrdinal = rdr.GetOrdinal("EntityCode");
							int entityNameOrdinal = rdr.GetOrdinal("EntityName");
							int entityDescriptionOrdinal = rdr.GetOrdinal("EntityDescription");
							int entityClassExternalOrdinal = rdr.GetOrdinal("EntityClassExternal");
							int httpMethodOrdinal = rdr.GetOrdinal("HttpMethod");
							int responseMapSchemaOrdinal = rdr.GetOrdinal("ResponseMapSchema");
							int requestMapSchemaOrdinal = rdr.GetOrdinal("RequestMapSchema");
							int expectedResultOrdinal = rdr.GetOrdinal("ExpectedResult");
							int resultPropertyOrdinal = rdr.GetOrdinal("ResultProperty");
							int errorPropertyOrdinal = rdr.GetOrdinal("ErrorProperty");
							int tokenOrdinal = rdr.GetOrdinal("Token");
							int tokenTypeOrdinal = rdr.GetOrdinal("TokenType");
							int expirationTimeOrdinal = rdr.GetOrdinal("ExpirationTime");
							int expirationDateOrdinal = rdr.GetOrdinal("ExpirationDate");
							int pathOrdinal = rdr.GetOrdinal("Path");
							int urlParamsOrdinal = rdr.GetOrdinal("UrlParams");
							int singleRecordParamOrdinal = rdr.GetOrdinal("SingleRecordParam");
							int timeTriggerEnableOrdinal = rdr.GetOrdinal("TimeTriggerEnable");
							int frequencyMinOrdinal = rdr.GetOrdinal("FrequencyMin");
							int erpTriggerEnableOrdinal = rdr.GetOrdinal("ErpTriggerEnable");
							int sfTriggerEnableOrdinal = rdr.GetOrdinal("SfTriggerEnable");
							int manualSyncEnableOrdinal = rdr.GetOrdinal("ManualSyncEnable");
							int deltaSyncOrdinal = rdr.GetOrdinal("DeltaSync");
							int lastExecutionDateOrdinal = rdr.GetOrdinal("LastExecutionDate");
							int offsetMinOrdinal = rdr.GetOrdinal("OffsetMin");
							int requestTimeoutSecsOrdinal = rdr.GetOrdinal("RequestTimeoutSecs");
							int requestReprocMaxRetriesOrdinal = rdr.GetOrdinal("RequestReprocMaxRetries");
							int requestReprocFrequencySecsOrdinal = rdr.GetOrdinal("RequestReprocFrequencySecs");
							int revalidationEnableOrdinal = rdr.GetOrdinal("RevalidationEnable");
							int creationDateOrdinal = rdr.GetOrdinal("CreateDate");
							int createUserOrdinal = rdr.GetOrdinal("CreateUser");
							int statusOrdinal = rdr.GetOrdinal("Status");
							int enableDynamicBodyOrdinal = rdr.GetOrdinal("EnableDynamicBody");
							int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
							int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");

							while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								DataSyncService element = new()
								{
									Id = rdr[idOrdinal].ToStr(),
									ErpDataId = rdr[erpDataIdOrdinal].ToStr(),
									ErpData = new DataSyncErp
									{
										Id = rdr[erpDataIdOrdinal].ToStr(),
										ErpCode = rdr[erpCodeOrdinal].ToStr(),
										Erp = rdr[erpOrdinal].ToStr(),
										ErpVersionCode = rdr[erpVersionCodeOrdinal].ToStr(),
										ErpVersion = rdr[erpVersionOrdinal].ToStr(),
										DbCode = rdr[dbCodeOrdinal].ToStr(),
										Db = rdr[dbOrdinal].ToStr(),
										DbVersionCode = rdr[dbVersionCodeOrdinal].ToStr(),
										DbVersion = rdr[dbVersionOrdinal].ToStr(),
										ManufacturingCode = rdr[manufacturingCodeOrdinal].ToStr(),
										Manufacturing = rdr[manufacturingOrdinal].ToStr(),
										ManufacturingVersionCode = rdr[manufacturingVersionCodeOrdinal].ToStr(),
										ManufacturingVersion = rdr[manufacturingVersionOrdinal].ToStr(),
										BaseUrl = rdr[baseUrlOrdinal].ToStr(),
										TokenRequestJson = rdr[tokenRequestJsonOrdinal].ToStr(),
										TokenRequestPath = rdr[tokenRequestPathOrdinal].ToStr(),
										TokenRequestMapSchema = rdr[tokenRequestMapSchemaOrdinal].ToStr(),
										TokenRequestResultProp = rdr[tokenRequestResultPropOrdinal].ToStr(),
										RequiresTokenRenewal = (EnableType)rdr[requiresTokenRenewalOrdinal].ToInt32(),
										TokenRenewalMapSchema = rdr[tokenRenewalMapSchemaOrdinal].ToStr(),
										RequiredHeaders = rdr[requiredHeadersOrdinal].ToStr(),
										DateTimeFormat = (DateTimeFormatType)rdr[dateTimeFormatOrdinal].ToInt32(),
										TimeZone = rdr[timeZoneOrdinal].ToStr(),
										ReprocessingTime = rdr[reprocessingTimeOrdinal].ToInt32(),
										MaxReprocessingTime = rdr[maxReprocessingTimeOrdinal].ToInt32()
									},
									EntityId = rdr[entityCodeOrdinal].ToStr(),
									Entity = new Entity
									{
										Id = rdr[entityCodeOrdinal].ToStr(),
										Name = rdr[entityNameOrdinal].ToStr(),
										Description = rdr[entityDescriptionOrdinal].ToStr(),
										NameEntityExternal = rdr[entityClassExternalOrdinal].ToStr(),
									},
									EntityCategoryCode = rdr[entityCodeOrdinal].ToStr(),
									ErpMapping = new DataSyncErpMapping
									{
										ErpId = rdr[erpDataIdOrdinal].ToStr(),
										Id = rdr[entityCodeOrdinal].ToStr(),
										HttpMethod = rdr[httpMethodOrdinal].ToStr(),
										ResponseMapSchema = rdr[responseMapSchemaOrdinal].ToStr(),
										RequestMapSchema = rdr[requestMapSchemaOrdinal].ToStr(),
										ExpectedResult = rdr[expectedResultOrdinal].ToStr(),
										ResultProperty = rdr[resultPropertyOrdinal].ToStr(),
										ErrorProperty = rdr[errorPropertyOrdinal].ToStr()
									},
									TokenData = new DataSyncErpAuth
									{
										ErpId = rdr[erpDataIdOrdinal].ToStr(),
										Token = rdr[tokenOrdinal].ToStr(),
										TokenType = rdr[tokenTypeOrdinal].ToStr(),
										ExpirationTime = rdr[expirationTimeOrdinal].ToInt32(),
										ExpirationDate = rdr[expirationDateOrdinal].ToDate()
									},
									Path = rdr[pathOrdinal].ToStr(),
									UrlParams = rdr[urlParamsOrdinal].ToStr(),
									SingleRecordParam = rdr[singleRecordParamOrdinal].ToStr(),
									HttpMethod = rdr[httpMethodOrdinal].ToStr(),
									TimeTriggerEnable = (EnableType)rdr[timeTriggerEnableOrdinal].ToInt32(),
									FrequencyMin = rdr[frequencyMinOrdinal].ToInt32(),
									ErpTriggerEnable = (EnableType)rdr[erpTriggerEnableOrdinal].ToInt32(),
									SfTriggerEnable = (EnableType)rdr[sfTriggerEnableOrdinal].ToInt32(),
									ManualSyncEnable = (EnableType)rdr[manualSyncEnableOrdinal].ToInt32(),
									DeltaSync = (EnableType)rdr[deltaSyncOrdinal].ToInt32(),
									LastExecutionDate = rdr[lastExecutionDateOrdinal].ToDate(),
									OffsetMin = rdr[offsetMinOrdinal].ToInt32(),
									RequestTimeoutSecs = rdr[requestTimeoutSecsOrdinal].ToInt32(),
									RequestReprocMaxRetries = rdr[requestReprocMaxRetriesOrdinal].ToInt32(),
									RequestReprocFrequencySecs = rdr[requestReprocFrequencySecsOrdinal].ToInt32(),
									RevalidationEnable = (EnableType)rdr[revalidationEnableOrdinal].ToInt32(),
									CreationDate = rdr[creationDateOrdinal].ToDate(),
									CreatedBy = new User(rdr[createUserOrdinal].ToInt32()),
									Status = (ServiceStatus)rdr[statusOrdinal].ToInt32(),
									EnableDynamicBody = rdr[enableDynamicBodyOrdinal].ToInt32()
								};

								if (rdr[updateDateOrdinal].ToDate().Year > 1900)
								{
									element.ModifyDate = rdr[updateDateOrdinal].ToDate();
									element.ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32());
								}

								(returnValue ??= []).Add(element);
							}
						}
						else
						{
							returnValue = [];
						}
					}
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					logger?.Error(ex);
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

	public async Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo, CancellationToken cancellationToken = default)
	{
		string returnValue = string.Empty;

		await using EWP_Connection connection = new(ConnectionStringLogs);
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_Execution_Log_INS", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();

				if (string.IsNullOrEmpty(logInfo.Id))
				{
					command.Parameters.AddNull("_Id");
				}
				else
				{
					_ = command.Parameters.AddWithValue("_Id", logInfo.Id);
				}

				_ = command.Parameters.AddWithValue("_Database", Database);
				_ = command.Parameters.AddWithValue("_ServiceInstanceId", logInfo.ServiceInstanceId);
				_ = command.Parameters.AddWithValue("_ExecutionInitDate", logInfo.ExecutionInitDate);
				_ = command.Parameters.AddWithValue("_SfProcessDate", logInfo.SfProcessDate);
				_ = command.Parameters.AddWithValue("_ExecutionFinishDate", logInfo.ExecutionFinishDate);
				_ = command.Parameters.AddWithValue("_FailedRecords", logInfo.FailedRecords);
				_ = command.Parameters.AddWithValue("_SuccessRecords", logInfo.SuccessRecords);
				_ = command.Parameters.AddWithValue("_ErpReceivedJson", logInfo.ErpReceivedJson);
				_ = command.Parameters.AddWithValue("_SfMappedJson", logInfo.SfMappedJson);
				_ = command.Parameters.AddWithValue("_SfResponseJson", logInfo.SfResponseJson);
				_ = command.Parameters.AddWithValue("_ServiceException", logInfo.ServiceException);
				_ = command.Parameters.AddWithValue("_ExecutionOrigin", logInfo.ExecutionOrigin);
				_ = command.Parameters.AddWithValue("_LogUser", logInfo.LogUser);
				_ = command.Parameters.AddWithValue("_LogEmployee", logInfo.LogEmployee);
				command.Parameters.AddCondition("_EndpointUrl", logInfo.EndpointUrl, !string.IsNullOrEmpty(logInfo.EndpointUrl));

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
						{
							returnValue = rdr["Id"].ToStr();
						}
					}
				}
				catch
				{
					// You can add logging here if necessary
					throw;
				}

				return returnValue;
			}
		}
	}

	public async Task<bool> InsertDataSyncServiceLogDetail(DataSyncServiceLogDetail logInfo, CancellationToken cancellationToken = default)
	{
		bool returnValue = false;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_Execution_Detail_Log_INS", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();

				if (!string.IsNullOrEmpty(logInfo.Id))
				{
					_ = command.Parameters.AddWithValue("_Id", logInfo.Id);
				}
				else
				{
					command.Parameters.AddNull("_Id");
				}

				_ = command.Parameters.AddWithValue("_LogId", logInfo.LogId);
				_ = command.Parameters.AddWithValue("_RowKey", logInfo.RowKey);
				_ = command.Parameters.AddWithValue("_ProcessDate", logInfo.ProcessDate);
				_ = command.Parameters.AddWithValue("_ErpReceivedJson", logInfo.ErpReceivedJson);
				_ = command.Parameters.AddWithValue("_SfMappedJson", logInfo.SfMappedJson);
				_ = command.Parameters.AddWithValue("_ResponseJson", logInfo.ResponseJson);
				_ = command.Parameters.AddWithValue("_MessageException", logInfo.MessageException);
				_ = command.Parameters.AddWithValue("_AttemptNo", logInfo.AttemptNo);
				_ = command.Parameters.AddWithValue("_LastAttemptDate", logInfo.LastAttemptDate);
				_ = command.Parameters.AddWithValue("_LogType", logInfo.LogType);

				try
				{
					int rdr = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
					if (rdr > 0)
					{
						returnValue = true;
					}
				}
				catch
				{
					throw;
				}

				return returnValue;
			}
		}
	}

	public bool InsertDataSyncServiceLogDetailBulk(string LogInfo)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Execution_Detail_Log_BLK", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_JSON", LogInfo);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public bool InsertDataSyncServiceErpToken(DataSyncErpAuth TokenInfo)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Erp_Token_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_ErpId", TokenInfo.ErpId);
				_ = command.Parameters.AddWithValue("_Token", TokenInfo.Token);
				_ = command.Parameters.AddWithValue("_TokenType", TokenInfo.TokenType);
				_ = command.Parameters.AddWithValue("_ExpirationTime", TokenInfo.ExpirationTime);
				_ = command.Parameters.AddWithValue("_ExpirationDate", TokenInfo.ExpirationDate);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public DataSyncErpAuth GetDataSyncServiceErpToken(string ErpCode)
	{
		DataSyncErpAuth returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Erp_Token_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_ErpId", ErpCode);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new DataSyncErpAuth
					{
						ErpId = rdr["ErpId"].ToStr(),
						Token = rdr["Token"].ToStr(),
						TokenType = rdr["TokenType"].ToStr(),
						ExpirationTime = rdr["ExpirationTime"].ToInt32(),
						ExpirationDate = rdr["ExpirationDate"].ToDate(),
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

	public List<DataSyncServiceLogDetail> GetDataSyncServiceFailRecords(string ErpId)
	{
		List<DataSyncServiceLogDetail> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Failed_Records_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_ErpId", ErpId);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncServiceLogDetail element = new()
						{
							Id = rdr["Id"].ToStr(),
							LogId = rdr["LogId"].ToStr(),
							ServiceInstanceId = rdr["ServiceInstanceId"].ToStr(),
							RowKey = rdr["RowKey"].ToStr(),
							ProcessDate = rdr["ProcessDate"].ToDate(),
							ErpReceivedJson = rdr["ErpReceivedJson"].ToStr(),
							SfMappedJson = rdr["SfMappedJson"].ToStr(),
							ResponseJson = rdr["ResponseJson"].ToStr(),
							MessageException = rdr["MessageException"].ToStr(),
							AttemptNo = rdr["AttemptNo"].ToInt32(),
							LastAttemptDate = rdr["LastAttemptDate"].ToDate(),
							LogType = (DataSyncLogType)rdr["LogType"].ToInt32()
						};
						returnValue.Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncService> GetServiceInstanceFullData(string ServiceInstance)
	{
		List<DataSyncService> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Entities_Instance_Full_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				if (!string.IsNullOrEmpty(ServiceInstance))
				{
					_ = command.Parameters.AddWithValue("_ServiceInstance", ServiceInstance);
				}
				else
				{
					command.Parameters.AddNull("_ServiceInstance");
				}
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncService element = new()
						{
							Id = rdr["Id"].ToStr(),
							ErpDataId = rdr["ErpDataId"].ToStr(),
							ErpData = new DataSyncErp
							{
								Id = rdr["ErpDataId"].ToStr(),
								ErpCode = rdr["ErpCode"].ToStr(),
								Erp = rdr["Erp"].ToStr(),
								ErpVersionCode = rdr["ErpVersionCode"].ToStr(),
								ErpVersion = rdr["ErpVersion"].ToStr(),
								DbCode = rdr["DbCode"].ToStr(),
								Db = rdr["Db"].ToStr(),
								DbVersionCode = rdr["DbVersionCode"].ToStr(),
								DbVersion = rdr["DbVersion"].ToStr(),
								ManufacturingCode = rdr["ManufacturingCode"].ToStr(),
								Manufacturing = rdr["Manufacturing"].ToStr(),
								ManufacturingVersionCode = rdr["ManufacturingVersionCode"].ToStr(),
								ManufacturingVersion = rdr["ManufacturingVersion"].ToStr(),
								BaseUrl = rdr["BaseUrl"].ToStr(),
								TokenRequestJson = rdr["TokenRequestJson"].ToStr(),
								TokenRequestPath = rdr["TokenRequestPath"].ToStr(),
								TokenRequestMapSchema = rdr["TokenRequestMapSchema"].ToStr(),
								TokenRequestResultProp = rdr["TokenRequestResultProp"].ToStr(),
								RequiresTokenRenewal = (EnableType)rdr["RequiresTokenRenewal"].ToInt32(),
								TokenRenewalMapSchema = rdr["TokenRenewalMapSchema"].ToStr(),
								RequiredHeaders = rdr["RequiredHeaders"].ToStr(),
								DateTimeFormat = (DateTimeFormatType)rdr["DateTimeFormat"].ToInt32(),
								TimeZone = rdr["DateTimeFormat"].ToStr(),
								ReprocessingTime = rdr["ReprocessingTime"].ToInt32(),
								MaxReprocessingTime = rdr["MaxReprocessingTime"].ToInt32(),
							},
							EntityId = rdr["EntityCode"].ToStr(),
							Entity = new Entity
							{
								Id = rdr["EntityCode"].ToStr(),
								Name = rdr["EntityName"].ToStr(),
								Description = rdr["EntityDescription"].ToStr(),
								NameEntityExternal = rdr["EntityClassExternal"].ToStr(),
							},
							EntityCategoryCode = rdr["EntityCategoryCode"].ToStr(),
							ErpMapping = new DataSyncErpMapping
							{
								ErpId = rdr["ErpDataId"].ToStr(),
								EntityId = rdr["EntityCode"].ToStr(),
								HttpMethod = rdr["HttpMethod"].ToStr(),
								ResponseMapSchema = rdr["ResponseMapSchema"].ToStr(),
								RequestMapSchema = rdr["RequestMapSchema"].ToStr(),
								ExpectedResult = rdr["ExpectedResult"].ToStr(),
								ResultProperty = rdr["ResultProperty"].ToStr(),
								ErrorProperty = rdr["ErrorProperty"].ToStr()
							},
							TokenData = new DataSyncErpAuth
							{
								ErpId = rdr["ErpDataId"].ToStr(),
								Token = rdr["Token"].ToStr(),
								TokenType = rdr["TokenType"].ToStr(),
								ExpirationTime = rdr["ExpirationTime"].ToInt32(),
								ExpirationDate = rdr["ExpirationDate"].ToDate()
							},
							Path = rdr["Path"].ToStr(),
							UrlParams = rdr["UrlParams"].ToStr(),
							SingleRecordParam = rdr["SingleRecordParam"].ToStr(),
							HttpMethod = rdr["HttpMethod"].ToStr(),
							TimeTriggerEnable = (EnableType)rdr["TimeTriggerEnable"].ToInt32(),
							FrequencyMin = rdr["FrequencyMin"].ToInt32(),
							ErpTriggerEnable = (EnableType)rdr["ErpTriggerEnable"].ToInt32(),
							SfTriggerEnable = (EnableType)rdr["SfTriggerEnable"].ToInt32(),
							ManualSyncEnable = (EnableType)rdr["ManualSyncEnable"].ToInt32(),
							DeltaSync = (EnableType)rdr["DeltaSync"].ToInt32(),
							LastExecutionDate = rdr["LastExecutionDate"].ToDate(),
							OffsetMin = rdr["OffsetMin"].ToInt32(),
							RequestTimeoutSecs = rdr["RequestTimeoutSecs"].ToInt32(),
							RequestReprocFrequencySecs = rdr["RequestReprocFrequencySecs"].ToInt32(),
							RevalidationEnable = (EnableType)rdr["RevalidationEnable"].ToInt32(),
							CreationDate = rdr["CreateDate"].ToDate(),
							CreatedBy = new User(rdr["CreateUser"].ToInt32()),
							Status = (ServiceStatus)rdr["Status"].ToInt32(),
							EnableDynamicBody = rdr["EnableDynamicBody"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.ModifyDate = rdr["UpdateDate"].ToDate();
							element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
						}
						returnValue ??= [];

						if (!string.IsNullOrEmpty(rdr["InstanceProperties"].ToStr()))
						{
							element.IndividualSyncProperties = rdr["InstanceProperties"].ToStr();
						}
						returnValue.Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public async Task<DataSyncServiceLog> GetDataSyncServiceLogs(string LogId, int logType = 0, CancellationToken cancellationToken = default)
	{
		DataSyncServiceLog returnValue = new();

		await using EWP_Connection connection = new(ConnectionStringLogs);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_Log_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Database", Database);
				command.Parameters.AddWithValue("_LogId", LogId);

				if (logType > 0)
				{
					command.Parameters.AddWithValue("_LogType", logType);
				}
				else
				{
					command.Parameters.AddNull("_LogType");
				}

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (rdr.HasRows)
						{
							int idOrdinal = rdr.GetOrdinal("Id");
							int serviceInstanceIdOrdinal = rdr.GetOrdinal("ServiceInstanceId");
							int executionInitDateOrdinal = rdr.GetOrdinal("ExecutionInitDate");
							int sfProcessDateOrdinal = rdr.GetOrdinal("SfProcessDate");
							int executionFinishDateOrdinal = rdr.GetOrdinal("ExecutionFinishDate");
							int failedRecordsOrdinal = rdr.GetOrdinal("FailedRecords");
							int successRecordsOrdinal = rdr.GetOrdinal("SuccessRecords");
							int erpReceivedJsonOrdinal = rdr.GetOrdinal("ErpReceivedJson");
							int sfMappedJsonOrdinal = rdr.GetOrdinal("SfMappedJson");
							int sfResponseJsonOrdinal = rdr.GetOrdinal("SfResponseJson");
							int serviceExceptionOrdinal = rdr.GetOrdinal("ServiceException");
							int executionOriginOrdinal = rdr.GetOrdinal("ExecutionOrigin");

							while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								returnValue = new DataSyncServiceLog
								{
									Id = rdr[idOrdinal].ToStr(),
									ServiceInstanceId = rdr[serviceInstanceIdOrdinal].ToStr(),
									ExecutionInitDate = rdr[executionInitDateOrdinal].ToDate(),
									SfProcessDate = rdr[sfProcessDateOrdinal].ToDate(),
									ExecutionFinishDate = rdr[executionFinishDateOrdinal].ToDate(),
									FailedRecords = rdr[failedRecordsOrdinal].ToInt32(),
									SuccessRecords = rdr[successRecordsOrdinal].ToInt32(),
									ErpReceivedJson = rdr[erpReceivedJsonOrdinal].ToStr(),
									SfMappedJson = rdr[sfMappedJsonOrdinal].ToStr(),
									SfResponseJson = rdr[sfResponseJsonOrdinal].ToStr(),
									ServiceException = rdr[serviceExceptionOrdinal].ToStr(),
									ExecutionOrigin = (ServiceExecOrigin)rdr[executionOriginOrdinal].ToInt32(),
								};
							}

							if (await rdr.NextResultAsync(cancellationToken).ConfigureAwait(false) && returnValue is not null)
							{
								int idDetailOrdinal = rdr.GetOrdinal("Id");
								int logIdDetailOrdinal = rdr.GetOrdinal("LogId");
								int rowKeyDetailOrdinal = rdr.GetOrdinal("RowKey");
								int processDateDetailOrdinal = rdr.GetOrdinal("ProcessDate");
								int erpReceivedJsonDetailOrdinal = rdr.GetOrdinal("ErpReceivedJson");
								int sfMappedJsonDetailOrdinal = rdr.GetOrdinal("SfMappedJson");
								int responseJsonDetailOrdinal = rdr.GetOrdinal("ResponseJson");
								int messageExceptionDetailOrdinal = rdr.GetOrdinal("MessageException");
								int attemptNoDetailOrdinal = rdr.GetOrdinal("AttemptNo");
								int lastAttemptDateDetailOrdinal = rdr.GetOrdinal("LastAttemptDate");
								int logTypeDetailOrdinal = rdr.GetOrdinal("LogType");

								returnValue.Details = [];
								while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
								{
									DataSyncServiceLogDetail element = new()
									{
										Id = rdr[idDetailOrdinal].ToStr(),
										LogId = rdr[logIdDetailOrdinal].ToStr(),
										RowKey = rdr[rowKeyDetailOrdinal].ToStr(),
										ProcessDate = rdr[processDateDetailOrdinal].ToDate(),
										ErpReceivedJson = rdr[erpReceivedJsonDetailOrdinal].ToStr(),
										SfMappedJson = rdr[sfMappedJsonDetailOrdinal].ToStr(),
										ResponseJson = rdr[responseJsonDetailOrdinal].ToStr(),
										MessageException = rdr[messageExceptionDetailOrdinal].ToStr(),
										AttemptNo = rdr[attemptNoDetailOrdinal].ToInt32(),
										LastAttemptDate = rdr[lastAttemptDateDetailOrdinal].ToDate(),
										LogType = (DataSyncLogType)rdr[logTypeDetailOrdinal].ToInt32(),
									};
									returnValue.Details.Add(element);
								}
							}
						}
						else
						{
							returnValue = new DataSyncServiceLog();
						}
					}
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					logger?.Error(ex);
					throw;
				}
			}
		}

		return returnValue;
	}

	public List<DataSyncServiceInstanceVisibility> GetSyncServiceInstanceVisibility(string Services, TriggerType Trigger)
	{
		List<DataSyncServiceInstanceVisibility> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Entities_Instance_Visibility_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Services", Services);
				_ = command.Parameters.AddWithValue("_Trigger", Trigger);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncServiceInstanceVisibility element = new()
						{
							ServiceInstanceId = rdr["ServiceInstanceId"].ToStr(),
							ServiceInstanceName = rdr["ServiceInstanceName"].ToStr(),
							IsActive = rdr["IsActive"].ToBool(),
							IsVisible = rdr["IsVisible"].ToBool(),
							PostEnabled = rdr["PostEnabled"].ToBool()
						};
						returnValue.Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncServiceLog> GetDataSyncServiceHeaderLogs(string ServiceInstanceId = "")
	{
		List<DataSyncServiceLog> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Execution_Log_Header_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				if (!string.IsNullOrEmpty(ServiceInstanceId))
				{
					_ = command.Parameters.AddWithValue("_ServiceInstanceId", ServiceInstanceId);
				}
				else
				{
					command.Parameters.AddNull("_ServiceInstanceId");
				}
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncServiceLog element = new()
						{
							Id = rdr["Id"].ToStr(),
							ServiceInstanceId = rdr["ServiceInstanceId"].ToStr(),
							ExecutionInitDate = rdr["ExecutionInitDate"].ToDate(),
							ExecutionInitDateTime = rdr["ExecutionInitDateTime"].ToStr(),
							ExecutionInitDateShort = rdr["ExecutionInitDateShort"].ToStr(),
							SfProcessDate = rdr["SfProcessDate"].ToDate(),
							ExecutionFinishDate = rdr["ExecutionFinishDate"].ToDate(),
							FailedRecords = rdr["FailedRecords"].ToInt32(),
							SuccessRecords = rdr["SuccessRecords"].ToInt32(),
							ErpReceivedJson = rdr["ErpReceivedJson"].ToStr(),
							SfMappedJson = rdr["SfMappedJson"].ToStr(),
							SfResponseJson = rdr["SfResponseJson"].ToStr(),
							ServiceException = rdr["ServiceException"].ToStr(),
							ExecutionOrigin = (ServiceExecOrigin)rdr["ExecutionOrigin"].ToInt32(),
							ExecutionOriginDescription = rdr["ExecutionOriginDescription"].ToStr(),
							UserName = rdr["UserName"].ToStr()
						};
						returnValue.Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<DataSyncServiceLog> GetDataSyncServiceHeaderErrorLogs(string ServiceInstanceId = "")
	{
		List<DataSyncServiceLog> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Execution_Log_Error_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_ServiceInstanceId", ServiceInstanceId);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncServiceLog element = new()
						{
							Id = rdr["RowKey"].ToStr(),
							ServiceInstanceId = rdr["ServiceInstanceId"].ToStr(),
							ExecutionInitDate = rdr["ProcessDate"].ToDate(),
							ServiceException = rdr["MessageException"].ToStr(),
							ExecutionOriginDescription = rdr["ExecutionOrigin"].ToStr()
						};
						returnValue.Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public async Task<string> GetDataSyncServiceHeaderDataLogs(string logId, string logType, CancellationToken cancellationToken = default)
	{
		string returnValue = string.Empty;

		await using EWP_Connection connection = new(ConnectionStringLogs);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_Execution_Log_HeaderData_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Database", Database);
				command.Parameters.AddCondition("_Id", logId,
												!string.IsNullOrEmpty(logId),
												string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Log Id"));
				command.Parameters.AddWithValue("_DataType", logType);

				try
				{
					object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
					returnValue = result.ToStr();
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					logger?.Error(ex, "Error while retrieving Data Sync Service Header Data Logs.");
					throw;
				}
			}
		}

		return returnValue;
	}

	public async Task<List<DataSyncServiceLogDetail>> GetDataSyncServiceDetailLogs(string logId, CancellationToken cancellationToken = default)
	{
		List<DataSyncServiceLogDetail> returnValue = [];

		await using EWP_Connection connection = new(ConnectionStringLogs);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_Execution_Log_Detail_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Database", Database);
				command.Parameters.AddCondition("_LogId", logId,
												!string.IsNullOrEmpty(logId),
												string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Log Id"));

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (rdr.HasRows)
						{
							// Cache column ordinals
							int idOrdinal = rdr.GetOrdinal("Id");
							int logIdOrdinal = rdr.GetOrdinal("LogId");
							int rowKeyOrdinal = rdr.GetOrdinal("RowKey");
							int processDateOrdinal = rdr.GetOrdinal("ProcessDate");
							int processDateTimeOrdinal = rdr.GetOrdinal("ProcessDateTime");
							int processDateShortOrdinal = rdr.GetOrdinal("ProcessDateShort");
							int erpReceivedJsonOrdinal = rdr.GetOrdinal("ErpReceivedJson");
							int sfMappedJsonOrdinal = rdr.GetOrdinal("SfMappedJson");
							int responseJsonOrdinal = rdr.GetOrdinal("ResponseJson");
							int messageExceptionOrdinal = rdr.GetOrdinal("MessageException");
							int attemptNoOrdinal = rdr.GetOrdinal("AttemptNo");
							int lastAttemptDateOrdinal = rdr.GetOrdinal("LastAttemptDate");
							int logTypeOrdinal = rdr.GetOrdinal("LogType");

							while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								DataSyncServiceLogDetail element = new()
								{
									Id = rdr[idOrdinal].ToStr(),
									LogId = rdr[logIdOrdinal].ToStr(),
									RowKey = rdr[rowKeyOrdinal].ToStr(),
									ProcessDate = rdr[processDateOrdinal].ToDate(),
									ProcessDateTime = rdr[processDateTimeOrdinal].ToStr(),
									ProcessDateShort = rdr[processDateShortOrdinal].ToStr(),
									ErpReceivedJson = rdr[erpReceivedJsonOrdinal].ToStr(),
									SfMappedJson = rdr[sfMappedJsonOrdinal].ToStr(),
									ResponseJson = rdr[responseJsonOrdinal].ToStr(),
									MessageException = rdr[messageExceptionOrdinal].ToStr(),
									AttemptNo = rdr[attemptNoOrdinal].ToInt32(),
									LastAttemptDate = rdr[lastAttemptDateOrdinal].ToDate(),
									LogType = (DataSyncLogType)rdr[logTypeOrdinal].ToInt32()
								};
								returnValue.Add(element);
							}
						}
					}
				}
				catch (Exception ex)
				{
					logger?.Error(ex, "Error while retrieving data sync service detail logs.");
					throw;
				}
			}
		}

		return returnValue;
	}

	public async Task<DataSyncServiceLogDetail> GetDataSyncServiceDetailLogsSingle(string logId, CancellationToken cancellationToken = default)
	{
		DataSyncServiceLogDetail returnValue = null;

		await using EWP_Connection connection = new(ConnectionStringLogs);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_Execution_Log_Detail_Single_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Database", Database);
				command.Parameters.AddCondition("_DetailId", logId,
												!string.IsNullOrEmpty(logId),
												string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Log Id"));

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (rdr.HasRows && await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
						{
							// Cache column ordinals
							int idOrdinal = rdr.GetOrdinal("Id");
							int logIdOrdinal = rdr.GetOrdinal("LogId");
							int rowKeyOrdinal = rdr.GetOrdinal("RowKey");
							int processDateOrdinal = rdr.GetOrdinal("ProcessDate");
							int erpReceivedJsonOrdinal = rdr.GetOrdinal("ErpReceivedJson");
							int sfMappedJsonOrdinal = rdr.GetOrdinal("SfMappedJson");
							int responseJsonOrdinal = rdr.GetOrdinal("ResponseJson");
							int messageExceptionOrdinal = rdr.GetOrdinal("MessageException");
							int attemptNoOrdinal = rdr.GetOrdinal("AttemptNo");
							int lastAttemptDateOrdinal = rdr.GetOrdinal("LastAttemptDate");
							int logTypeOrdinal = rdr.GetOrdinal("LogType");

							returnValue = new DataSyncServiceLogDetail
							{
								Id = rdr[idOrdinal].ToStr(),
								LogId = rdr[logIdOrdinal].ToStr(),
								RowKey = rdr[rowKeyOrdinal].ToStr(),
								ProcessDate = rdr[processDateOrdinal].ToDate(),
								ErpReceivedJson = rdr[erpReceivedJsonOrdinal].ToStr(),
								SfMappedJson = rdr[sfMappedJsonOrdinal].ToStr(),
								ResponseJson = rdr[responseJsonOrdinal].ToStr(),
								MessageException = rdr[messageExceptionOrdinal].ToStr(),
								AttemptNo = rdr[attemptNoOrdinal].ToInt32(),
								LastAttemptDate = rdr[lastAttemptDateOrdinal].ToDate(),
								LogType = (DataSyncLogType)rdr[logTypeOrdinal].ToInt32()
							};
						}
					}
				}
				catch (Exception ex)
				{
					logger?.Error(ex, "Error while retrieving single data sync service detail log.");
					throw;
				}
			}
		}

		return returnValue;
	}

	public DataSyncErpMapping MergeDataSyncServiceInstanceMapping(DataSyncErpMapping InstanceMapping, User SystemOperator)
	{
		DataSyncErpMapping returnValue = new();
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Erp_Mapping_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_ErpId", InstanceMapping.ErpId, !string.IsNullOrEmpty(InstanceMapping.ErpId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Erp ID"));
				command.Parameters.AddCondition("_EntityCode", InstanceMapping.EntityId, !string.IsNullOrEmpty(InstanceMapping.EntityId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Entity ID"));
				command.Parameters.AddCondition("_HttpMethod", InstanceMapping.HttpMethod, !string.IsNullOrEmpty(InstanceMapping.HttpMethod), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "HTTP Method"));
				_ = command.Parameters.AddWithValue("_ResponseMapSchema", InstanceMapping.ResponseMapSchema);
				_ = command.Parameters.AddWithValue("_RequestMapSchema", InstanceMapping.RequestMapSchema);
				_ = command.Parameters.AddWithValue("_ExpectedResult", InstanceMapping.ExpectedResult);
				_ = command.Parameters.AddWithValue("_ResultProperty", InstanceMapping.ResultProperty);
				_ = command.Parameters.AddWithValue("_ErrorProperty", InstanceMapping.ErrorProperty);
				command.Parameters.AddCondition("_User", () => SystemOperator.Id, SystemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", SystemOperator.EmployeeId, !string.IsNullOrEmpty(SystemOperator.EmployeeId));
				_ = command.Parameters.AddWithValue("_Status", InstanceMapping.Status.ToInt32());
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						returnValue = new DataSyncErpMapping
						{
							ErpId = rdr["ErpId"].ToStr(),
							EntityId = rdr["EntityCode"].ToStr(),
							HttpMethod = rdr["HttpMethod"].ToStr(),
							ResponseMapSchema = rdr["ResponseMapSchema"].ToStr(),
							RequestMapSchema = rdr["RequestMapSchema"].ToStr(),
							ExpectedResult = rdr["ExpectedResult"].ToStr(),
							ResultProperty = rdr["ResultProperty"].ToStr(),
							ErrorProperty = rdr["ErrorProperty"].ToStr(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							returnValue.UpdateDate = rdr["UpdateDate"].ToDate();
							returnValue.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
					}
				}
				else
				{
					returnValue = new DataSyncErpMapping();
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

	public List<DefaultMappingEntityObject> ListDefaultMappingEntityObject(string Entity)
	{
		List<DefaultMappingEntityObject> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Default_Mapping_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Entity", Entity, !string.IsNullOrEmpty(Entity), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Entity"));
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DefaultMappingEntityObject element = new()
						{
							Id = rdr["Id"].ToStr(),
							Name = rdr["Name"].ToStr()
						};
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public List<TimeZoneCatalog> GetTimezones(bool currentValues)
	{
		List<TimeZoneCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_Timezone_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				_ = command.Parameters.AddWithValue("_Current", currentValues);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						TimeZoneCatalog element = new()
						{
							Key = rdr["Label"].ToStr(),
							Value = rdr["Value"].ToStr(),
							Offset = rdr["Offset"].ToDouble()
						};
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public string GetDatasyncDynamicBody(string EntityCode)
	{
		string returnValue = string.Empty;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Datasync_DynamicBody_GET", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_EntityCode", EntityCode);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = command.ExecuteScalarAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToStr();
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

	public async Task<List<DataSyncIoTDataSimulator>> GetTagsSimulatorService(bool IsInitial, CancellationToken cancellationToken = default)
	{
		List<DataSyncIoTDataSimulator> returnValue = null;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_Tag_Simulator_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_IsInitial", IsInitial);
				try
				{
					// Execute reader asynchronously
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (rdr.HasRows)
						{
							// Get ordinals for column indices
							int tagOrdinal = rdr.GetOrdinal("Tag");
							int tagCollectorIdOrdinal = rdr.GetOrdinal("TagCollectorId");
							int valueOrdinal = rdr.GetOrdinal("Value");
							int valueDateOrdinal = rdr.GetOrdinal("ValueDate");
							int TypeOrdinal = rdr.GetOrdinal("Type");
							int processDateVersionOrdinal = rdr.GetOrdinal("ProcessDate");
							int readDateOrdinal = rdr.GetOrdinal("ReadDate");
							int senDateOrdinal = rdr.GetOrdinal("SendDate");

							while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								DataSyncIoTDataSimulator element = new()
								{
									Tag = rdr[tagOrdinal].ToStr(),
									TagCollectorId = rdr[tagCollectorIdOrdinal].ToStr(),
									Value = rdr[valueOrdinal].ToStr(),
									ValueDate = rdr[valueDateOrdinal].ToDate(),
									Type = rdr[TypeOrdinal].ToStr(),
									ProcessDate = rdr[processDateVersionOrdinal].ToDate(),
									ReadDate = rdr[readDateOrdinal].ToDate(),
									SendDate = rdr[senDateOrdinal].ToDate()
								};
								(returnValue ??= []).Add(element);
							}
						}
						else
						{
							returnValue = [];
						}
					}
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					logger?.Error(ex);
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
}
