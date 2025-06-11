using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models.IntegrationStaging;

namespace EWP.SF.KafkaSync.DataAccess;
public class SkillRepo : ISkillRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public SkillRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }// <summary>
	///
	/// </summary>
	public List<Skill> ListAllSkills(DateTime? DeltaDate = null)
	{
		List<Skill> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Skill_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddNull("_SkillId");
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Skill element = new()
					{
						Id = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32())
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

	/// <summary>
	///
	/// </summary>
	public List<Skill> ListMachineSkills(string machineId = "")
	{
		List<Skill> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Machine_Skill_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Skill element = new()
					{
						Id = rdr["SkillCode"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						ParentId = rdr["MachineCode"].ToStr()
					};

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
	public bool SaveMachineSkills(string machineId, List<string> skillIds, User systemOperator)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Machine_Skill_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine"));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, !systemOperator.IsNull(), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Operator"));
				command.Parameters.AddCondition("_Skills", () => string.Join(",", [.. skillIds]), !skillIds.IsNull());

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
