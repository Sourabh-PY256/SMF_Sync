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
using EWP.SF.Common.Models.Catalogs;
using NLog;

namespace EWP.SF.KafkaSync.DataAccess;

public class CatalogRepo : ICatalogRepo
{
	private static Logger logger = LogManager.GetCurrentClassLogger();
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public CatalogRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }

	#region CatalogsSkills

	/// <summary>
	///
	/// </summary>
	public List<CatSkills> GetCatSkillsList(string skill = "", DateTime? DeltaDate = null)
	{
		List<CatSkills> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_CatSkills_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.AddCondition("_Skill", skill, !string.IsNullOrEmpty(skill));
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					CatSkills catSkills = new()
					{
						SkillId = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						ProcessType = rdr["OperationTypeCode"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32())
					};

					returnValue.Add(catSkills);
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
	public CatSkills CreateSkill(CatSkills catSkills, User systemOperator)
	{
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_CatSkills_INS", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			// command.Parameters.AddCondition("_Operator", systemOperator?.Id, !systemOperator.IsNull(), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Operator Id"));
			command.Parameters.AddCondition("_Code", catSkills?.Code, !string.IsNullOrEmpty(catSkills?.Code));
			command.Parameters.AddCondition("_Name", catSkills?.Name, !string.IsNullOrEmpty(catSkills?.Name));
			command.Parameters.AddCondition("_OperationTypeCode", catSkills?.ProcessType, !string.IsNullOrEmpty(catSkills?.ProcessType));
			command.Parameters.AddWithValue("_Status", catSkills?.Status);
			command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				catSkills.SkillId = rdr["Code"].ToString();
				catSkills.CreationDate = rdr["CreateDate"].ToDate();
				catSkills.CreatedBy = new User(rdr["CreateUser"].ToInt32());
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
		return catSkills;
	}

	/// <summary>
	///
	/// </summary>
	public ResponseData MergeSkill(CatSkills catSkills, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_CatSkills_INS", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddNull("_SkillId");
			command.Parameters.AddWithValue("_IsValidation", Validation);
			command.Parameters.AddCondition("_Code", catSkills?.Code, !string.IsNullOrEmpty(catSkills?.Code));
			command.Parameters.AddCondition("_Name", catSkills?.Name, !string.IsNullOrEmpty(catSkills?.Name));
			command.Parameters.AddCondition("_OperationTypeCode", catSkills?.ProcessType, !string.IsNullOrEmpty(catSkills?.ProcessType));
			command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
			command.Parameters.AddWithValue("_Status", catSkills?.Status);
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
		return returnValue;
	}

	#endregion CatalogsSkills

	#region CatalogsProfile

	/// <summary>
	///
	/// </summary>
	public List<CatProfile> GetCatalogProfile(string profile = "", DateTime? DeltaDate = null)
	{
		List<CatProfile> returnValue = [];
		PositionSkills detailSkill = null;
		PositionAssets detailAsset = null;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_CatProfile_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.AddCondition("_Profile", profile, !string.IsNullOrEmpty(profile));
			command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				CatProfile element = new()
				{
					ProfileId = rdr["Code"].ToStr(),
					NameProfile = rdr["Name"].ToStr(),
					Image = rdr["Image"].ToStr(),
					Code = rdr["Code"].ToStr(),
					Status = (Status)rdr["Status"].ToInt32(),
					AuthorizationRequired = rdr["AuthorizationRequired"].ToBool(),
					Attribute1 = !string.IsNullOrEmpty(rdr["Attribute1"].ToStr()) ? rdr["Attribute1"].ToStr() : null,
					Attribute2 = !string.IsNullOrEmpty(rdr["Attribute2"].ToStr()) ? rdr["Attribute2"].ToDecimal() : null,
					Attribute3 = !string.IsNullOrEmpty(rdr["Attribute3"].ToStr()) ? rdr["Attribute3"].ToInt32() : (int?)null,
					ScheduleLevel = rdr["ScheduleLevel"].ToStr(),
					Schedule = rdr["Schedule"].ToBool(),
					CostPerHour = rdr["CostPerHour"] != DBNull.Value ? rdr["CostPerHour"].ToDecimal() : null,
					CreationDate = rdr["CreateDate"].ToDate(),
					CreatedBy = new User(rdr["CreateUser"].ToInt32()),
					AssignedAsset = "",
					Skills = "",
					LogDetailId = rdr["LogDetailId"].ToStr()
				};

				if (returnValue.IsNull())
				{
					returnValue = [];
				}
				returnValue.Add(element);
			}

			if (returnValue.Count > 0)
			{
				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					detailSkill = new PositionSkills
					{
						SkillCode = rdr["SkillCode"].ToStr(),
					};
					CatProfile elem = returnValue.Find(x => x.Code == rdr["PositionCode"].ToStr());
					if (elem is not null)
					{
						elem.PositionSkills.Add(detailSkill);
						elem.Skills = string.Join(",", returnValue.Find(x => x.Code == rdr["PositionCode"].ToStr())?.PositionSkills?.Select(x => x.SkillCode));
					}
				}
			}

			if (returnValue.Count > 0)
			{
				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					detailAsset = new PositionAssets
					{
						AssetCode = rdr["AssetCode"].ToStr(),
						AssetTypeCode = rdr["AssetTypeCode"].ToStr(),
					};

					CatProfile elem = returnValue.Find(x => x.Code == rdr["PositionCode"].ToStr());
					if (elem is not null)
					{
						elem.PositionAssets.Add(detailAsset);
						elem.AssignedAsset = string.Join(",", returnValue.Find(x => x.Code == rdr["PositionCode"].ToStr()).PositionAssets?.Select(x => x.AssetTypeCode + "-" + x.AssetCode));
					}
				}
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
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public ResponseData MergeProfile(CatProfile catProfile, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_CatProfile_INS", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddNull("_ProfileId");
			command.Parameters.AddWithValue("_IsValidation", Validation);
			command.Parameters.AddCondition("_NameProfile", catProfile?.NameProfile, !string.IsNullOrEmpty(catProfile?.NameProfile));
			command.Parameters.AddCondition("_Code", catProfile?.Code, !string.IsNullOrEmpty(catProfile?.Code));
			command.Parameters.AddWithValue("_AuthorizationRequired", catProfile?.AuthorizationRequired);
			command.Parameters.AddWithValue("_Attribute1", !string.IsNullOrEmpty(catProfile.Attribute1) ? catProfile.Attribute1 : null);
			command.Parameters.AddWithValue("_Attribute2", catProfile.Attribute2);
			command.Parameters.AddWithValue("_Attribute3", catProfile.Attribute3);
			command.Parameters.AddWithValue("_ScheduleLevel", catProfile.ScheduleLevel ?? "Primary");
			command.Parameters.AddWithValue("_CostPerHour", catProfile.CostPerHour);
			command.Parameters.AddWithValue("_Schedule", catProfile.Schedule);
			command.Parameters.AddWithValue("_DetallesSkillsJSON", catProfile.PositionSkillsToJSON());
			command.Parameters.AddWithValue("_DetallesAssignedAssetJSON", catProfile.PositionAssetsToJSON());
			command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
			command.Parameters.AddWithValue("_Status", catProfile?.Status);
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
		catch (Exception ex)
		{
			logger.Error(ex);
			throw;
		}
		finally
		{
			connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}
		return returnValue;
	}

	#endregion CatalogsProfile

	#region Countries

	/// <summary>
	///
	/// </summary>
	public List<Country> GetCountryList(string Code)
	{
		List<Country> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Countries_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Country newCountry = new()
					{
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Alpha2Code = rdr["Alpha2Code"].ToStr(),
						Alpha3Code = rdr["Alpha3Code"].ToStr(),
						Icon = rdr["Alpha3Code"].ToStr(),
						NumericCode = rdr["NumericCode"].ToStr(),
						Status = rdr["Status"].ToInt32()
					};

					returnValue.Add(newCountry);
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

	#endregion Countries

	#region Genres

	/// <summary>
	///
	/// </summary>
	public List<Genre> GetGenreList(string Code)
	{
		List<Genre> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Genre_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Genre element = new()
					{
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = rdr["Status"].ToInt32()
					};

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

	#endregion Genres

	#region Marital Status

	/// <summary>
	///
	/// </summary>
	public List<MaritalStatus> GetMaritalStatusList(string Code)
	{
		List<MaritalStatus> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MaritalStatus_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					MaritalStatus element = new()
					{
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = rdr["Status"].ToInt32()
					};

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

	#endregion Marital Status

	#region Layout

	/// <summary>
	///
	/// </summary>
	public List<Layout> GetLayoutList(string Code)
	{
		List<Layout> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Layout_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Layout element = new()
					{
						Id = rdr["Code"].ToStr(),
						Value = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),

						Template = rdr["Template"].ToStr(),
						Key = rdr["Template"].ToStr(),
						Status = rdr["Status"].ToInt32()
					};

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

	#endregion Layout
}
