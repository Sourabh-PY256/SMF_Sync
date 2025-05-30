using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

public class EmployeeRepo : IEmployeeRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public EmployeeRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }

    #region Employee
    public List<EmployeeSkills> EmployeeSkillsList(string id)
	{
		List<EmployeeSkills> returnValue = [];

		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_Employee_Skills_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddWithValue("_EmployeeId", id);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				EmployeeSkills element = new()
				{
					Employee_Skills_Id = rdr["Id"].ToStr(),
					EmployeeId = rdr["EmployeeCode"].ToStr(),
					SkillId = rdr["SkillCode"].ToStr(),
					NameSkill = rdr["NameSkill"].ToStr(),
					QualificationObtained = rdr["SkillLevelCode"].ToStr(),
					CertificationDate = rdr["CertificationDate"].ToDate(),
					NameDocument = rdr["NameDocument"].ToStr(),
					AttachmentId = rdr["AttachmentId"].ToStr()
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
		return returnValue;
	}
	public List<EmployeeContractsDetail> ContractsDetailList(string id)
	{
		List<EmployeeContractsDetail> returnValue = [];

		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_Employee_Contracts_Detail_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddWithValue("_EmployeeId", id);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				EmployeeContractsDetail element = new()
				{
					Employee_Contracts_Detail_Id = rdr["Id"].ToStr(),
					EmployeeId = rdr["EmployeeCode"].ToStr(),
					DateStart = rdr["StartDate"].ToDate(),
					//DateEnd = rdr["EndDate"].ToDate(),
					//Validar cual es el registro actual
					DateEnd = rdr["EndDate"] != DBNull.Value ? rdr["EndDate"].ToDate() : null,
					ProfileId = rdr["PositionCode"].ToStr(),
					Salary = rdr["Salary"].ToDecimal(),
					OvertimeCost = rdr["OvertimeCost"].ToDecimal(),
					VacationsDays = rdr["VacationsDays"].ToDecimal(),
					HolidaysIncluded = rdr["HolidaysIncluded"].ToDecimal()
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
		return returnValue;
	}

    public List<EmployeeSkills> CreateEmployeeSkills(string employeeId, string XML, User systemOperator)
    {
        List<EmployeeSkills> returnValue = [];
        EmployeeSkills employeeSkill = null;

        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Employee_Skillss_MRG", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_EmployeeId", employeeId, !string.IsNullOrEmpty(employeeId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "EmployeeId"));
                command.Parameters.AddCondition("_XML", XML, !string.IsNullOrEmpty(XML));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    employeeSkill = new EmployeeSkills
                    {
                        Employee_Skills_Id = rdr["EmployeeSkillId"].ToStr(),
                        EmployeeId = rdr["EmployeeCode"].ToStr(),
                        SkillId = rdr["SkillCode"].ToStr()
                    };
                    //employeeSkill.Name = rdr["Name"].ToStr();
                    //downtimeType.ClassId = rdr["ClassId"].ToStr();
                    //downtimeType.CreationDate = rdr["CreationDate"].ToDate();
                    //downtimeType.CreatedById = rdr["CreatedById"].ToInt32();
                    //downtimeType.Status = rdr["Status"].ToInt32();

                    returnValue.Add(employeeSkill);
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
	public List<EmployeeContractsDetail> CreateEmployeeContractsDetail(string employeeId, string XML, User systemOperator)
	{
		List<EmployeeContractsDetail> returnValue = [];

		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Employee_Contracts_Detail_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_EmployeeId", employeeId, !string.IsNullOrEmpty(employeeId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "EmployeeId"));
				command.Parameters.AddCondition("_XML", XML, !string.IsNullOrEmpty(XML));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					//DowntimeType downtimeType = new DowntimeType();

					//downtimeType.DowntimeTypeId = rdr["DowntimeTypeId"].ToStr();
					//downtimeType.Code = rdr["Code"].ToStr();
					//downtimeType.Name = rdr["Name"].ToStr();
					//downtimeType.ClassId = rdr["ClassId"].ToStr();
					//downtimeType.CreationDate = rdr["CreationDate"].ToDate();
					//downtimeType.CreatedById = rdr["CreatedById"].ToInt32();
					//downtimeType.Status = rdr["Status"].ToInt32();

					//returnValue.Add(downtimeType);
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

public void GeneratePositionShiftExplosion(string EmployeeCode)

	{
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_PositionShiftExplosion_INS", connection)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = 30000
			};
			command.Parameters.Clear();

			command.Parameters.AddWithValue("_EmployeeCode", EmployeeCode);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				ResponseData returnValue = new()
				{
					Id = rdr["Id"].ToStr(),
					Action = (ActionDB)rdr["Action"].ToInt32(),
					IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
					Code = rdr["Code"].ToStr(),
					Message = rdr["Message"].ToStr(),
				};
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
public ResponseData MRGEmployee(Employee employee, bool Validation, User systemOperator)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Employee_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddWithValue("_EmployeeId", employee.Id);
				command.Parameters.AddCondition("_Code", employee.Code, !string.IsNullOrEmpty(employee.Code), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "_Code"));
				command.Parameters.AddCondition("_Name", employee.Name, !string.IsNullOrEmpty(employee.Name), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "_Name"));
				command.Parameters.AddWithValue("_LastName", employee.LastName);
				//command.Parameters.AddCondition("_ShiftId", employee.ShiftId, !string.IsNullOrEmpty(employee.ShiftId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "_ShiftId"));
				//command.Parameters.AddWithValue("_ShiftId", null);
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				command.Parameters.AddWithValue("_Operator", employee.UserId);
				command.Parameters.AddWithValue("_Status", employee.Status);
				command.Parameters.AddWithValue("_Hash", employee.Password);
				command.Parameters.AddWithValue("_PasswordFlag", employee.FlagPassword);
				command.Parameters.AddWithValue("_SupervisorId", employee.SupervisorId);
				command.Parameters.AddWithValue("_AuthorizationRequired", employee.AuthorizationRequired);
				command.Parameters.AddWithValue("_DetallesAssignedAssetJSON", employee.AssetsId is not null ? employee.EmployeeAssetsToJSON() : null);

				command.Parameters.AddWithValue("_Email", employee.Email);
				command.Parameters.AddWithValue("_PhoneNumber", employee.PhoneNumber);
				command.Parameters.AddWithValue("_Street", employee.Street);
				command.Parameters.AddWithValue("_Number", employee.Number);
				command.Parameters.AddWithValue("_Region", employee.Region);
				command.Parameters.AddWithValue("_Country", employee.Country);
				command.Parameters.AddWithValue("_StateProvince", employee.StateProvince);
				command.Parameters.AddWithValue("_City", employee.City);
				command.Parameters.AddWithValue("_Other", employee.Other);
				command.Parameters.AddWithValue("_Role", employee.Role);
				command.Parameters.AddWithValue("_ZipCode", employee.ZipCode);
				command.Parameters.AddWithValue("_Genre", employee.Genre);
				command.Parameters.AddWithValue("_BirthDate", employee.DateBirth);
				command.Parameters.AddWithValue("_PlaceBirth", employee.PlaceBirth);
				command.Parameters.AddWithValue("_MaritalStatus", employee.MaritalStatus);
				command.Parameters.AddWithValue("_NumberChildren", employee.NumberChildren);
				command.Parameters.AddWithValue("_IDNumber", employee.IDNumber);
				command.Parameters.AddWithValue("_Nationality", employee.Nationality);
				command.Parameters.AddWithValue("_PassportNumber", employee.PassportNumber);
				command.Parameters.AddWithValue("_PassportIssueDate", employee.PassportIssueDate);
				command.Parameters.AddWithValue("_PassportExpDate", employee.PassportExpiry);
				command.Parameters.AddWithValue("_IssuingAuthority", employee.IssuingAuthority);
				command.Parameters.AddWithValue("_TimeTolerance", employee.TimeTolerance);
				command.Parameters.AddWithValue("_CostPerHour", employee.CostPerHour);
				command.Parameters.AddWithValue("_NotificationGroup", employee.NotificationGroup);
				command.Parameters.AddWithValue("_ExternalId", employee.ExternalId);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					if (rdr["Id"].ToStr() == "@CEOPositionHeldBy")
					{
						returnValue = new ResponseData
						{
							Id = rdr["Id"].ToStr(),
							IsSuccess = true,
							Message = "@CEOPositionHeldBy",
						};
					}
					else
					{
						returnValue = new ResponseData
						{
							Id = rdr["Id"].ToStr(),
							Action = (ActionDB)rdr["Action"].ToInt32(),
							IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
							Code = rdr["Code"].ToStr(),
							Message = rdr["Message"].ToStr(),
						};
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

    #endregion Employee
}