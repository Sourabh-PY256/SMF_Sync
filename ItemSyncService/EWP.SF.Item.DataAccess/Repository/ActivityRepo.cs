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

public class ActivityRepo : IActivityRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public ActivityRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region Activity
    public bool UpdateActivity(Activity activityInfo, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Activity_UPD", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_ActivityId", activityInfo.Id, activityInfo is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Activity Id"));
                command.Parameters.AddCondition("_Name", activityInfo.Name, !string.IsNullOrEmpty(activityInfo.Name));
                command.Parameters.AddCondition("_ActivityClassId", activityInfo.ActivityClassId, activityInfo.ActivityClassId > 0);
                command.Parameters.AddCondition("_ActivityTypeCode", activityInfo.ActivityTypeId, !string.IsNullOrEmpty(activityInfo.ActivityTypeId));
                command.Parameters.AddCondition("_InterventionId", activityInfo.InterventionId, !string.IsNullOrEmpty(activityInfo.InterventionId));
                _ = command.Parameters.AddWithValue("_AssetLevel", activityInfo.AssetLevel);
                _ = command.Parameters.AddWithValue("_AssetCode", activityInfo.AssetCode);
                command.Parameters.AddCondition("_SourceId", activityInfo.SourceId, !string.IsNullOrEmpty(activityInfo.SourceId));
                //command.Parameters.AddCondition("_AssetId", activityInfo.AssetId, !string.IsNullOrEmpty(activityInfo.AssetId));
                command.Parameters.AddCondition("_IncludedAssets", activityInfo.IncludedAssets, !string.IsNullOrEmpty(activityInfo.IncludedAssets));
                _ = command.Parameters.AddWithValue("_IsAvailable", activityInfo.IsAvailable);
                command.Parameters.AddCondition("_Description", activityInfo.Description, !string.IsNullOrEmpty(activityInfo.Description));
                _ = command.Parameters.AddWithValue("_RequiresNotifications", activityInfo.RequiresNotifications);
                _ = command.Parameters.AddWithValue("_RequiresInstructions", activityInfo.RequiresInstructions);
                _ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
                command.Parameters.AddCondition("_ProcedureId", activityInfo.ParentId, !string.IsNullOrEmpty(activityInfo.ParentId));
                _ = command.Parameters.AddWithValue("_OperatorEmployee", systemOperator.EmployeeId);

                _ = command.Parameters.AddWithValue("_Status", activityInfo.Status);
                _ = command.Parameters.AddWithValue("_WorkDay", activityInfo.WorkDay);
                _ = command.Parameters.AddWithValue("_EventType", activityInfo.EventType);
                _ = command.Parameters.AddWithValue("_Origin", activityInfo.Origin);
                _ = command.Parameters.AddWithValue("_CodeShiftStatus", activityInfo.CodeShiftStatus);
                _ = command.Parameters.AddWithValue("_IsMandatory", activityInfo.IsMandatory);

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
    public List<ActivityInstanceCalculateResponse> ActivityInstanceCalculate(ActivityInstanceCalculateRequest activityInfo, User systemOperator)
    {
        List<ActivityInstanceCalculateResponse> returnValue = [];
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Activity_Instance_Calculate_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                //command.Parameters.AddNull("_ActivityId");
                _ = command.Parameters.AddWithValue("_ActivityId", activityInfo.ActivityId);
                _ = command.Parameters.AddWithValue("_OneTime", activityInfo.OneTime);

                //command.Parameters.AddCondition("_RecurrenceCode", activityInfo.RecurrenceCode.ToUpper(), activityInfo.RecurrenceCode is not null);
                //command.Parameters.AddCondition("_OptionsEndCode", activityInfo.OptionsEndCode.ToUpper(), !string.IsNullOrEmpty(activityInfo.OptionsEndCode is not null),);
                //command.Parameters.AddCondition("_DailyCode", activityInfo.DailyCode.ToUpper(), !string.IsNullOrEmpty(activityInfo.DailyCode), null);
                _ = command.Parameters.AddWithValue("_RecurrenceCode", activityInfo.RecurrenceCode);
                _ = command.Parameters.AddWithValue("_OptionsEndCode", activityInfo.OptionsEndCode);
                _ = command.Parameters.AddWithValue("_DailyCode", activityInfo.DailyCode);
                _ = command.Parameters.AddWithValue("_StartDate", activityInfo.StartDate);
                _ = command.Parameters.AddWithValue("_EndDate", activityInfo.EndDate);
                _ = command.Parameters.AddWithValue("_DurationInSec", activityInfo.DurationInSec);
                _ = command.Parameters.AddWithValue("_DailyDays", activityInfo.DailyDays);
                _ = command.Parameters.AddWithValue("_NoWeeks", activityInfo.NoWeeks);
                _ = command.Parameters.AddWithValue("_OptionsWeekly", activityInfo.OptionsWeekly);
                _ = command.Parameters.AddWithValue("_MonthlyOptionsCode", activityInfo.MonthlyOptionsCode);
                _ = command.Parameters.AddWithValue("_MonthlyEvery", activityInfo.MonthlyEvery);
                _ = command.Parameters.AddWithValue("_MonthlyDay", activityInfo.MonthlyDay);
                _ = command.Parameters.AddWithValue("_EditSeries", activityInfo.EditSeries);
                _ = command.Parameters.AddWithValue("_Name", activityInfo.Name);
                _ = command.Parameters.AddWithValue("_InstanceId", activityInfo.InstanceId);
                _ = command.Parameters.AddWithValue("_IsCreateActivity", activityInfo.IsCreateActivity);
                _ = command.Parameters.AddWithValue("_MonthlyOrderDaysCode", activityInfo.MonthlyOrderDaysCode);
                _ = command.Parameters.AddWithValue("_MonthlyDayCode", activityInfo.MonthlyDayCode);
                _ = command.Parameters.AddWithValue("_Occurrences", activityInfo.Occurrences);
                _ = command.Parameters.AddWithValue("_MonthlyByYearly", activityInfo.MonthlyByYearly);
                _ = command.Parameters.AddWithValue("_YearlyOptionsCode", activityInfo.YearlyOptionsCode);
                _ = command.Parameters.AddWithValue("_EveryYear", activityInfo.EveryYear);
                _ = command.Parameters.AddWithValue("_EveryHour", activityInfo.EveryHour);
                _ = command.Parameters.AddWithValue("_CodeShift", activityInfo.CodeShift);
                _ = command.Parameters.AddWithValue("_Orgin", activityInfo.Origin);
                _ = command.Parameters.AddWithValue("_IsShift", activityInfo.IsShift);
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int IdOrdinal = rdr.GetOrdinal("Id");
                int StartDateOrdinal = rdr.GetOrdinal("StartDate");
                int EndDateOrdinal = rdr.GetOrdinal("EndDate");
                int ActivityIdOrdinal = rdr.GetOrdinal("ActivityId");
                int NotificationsOrdinal = rdr.GetOrdinal("Notifications");
                int StatusOrdinal = rdr.GetOrdinal("Status");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    ActivityInstanceCalculateResponse element = new()
                    {
                        Id = rdr[IdOrdinal].ToStr(),
                        StartDate = rdr[StartDateOrdinal].ToDate(),
                        EndDate = rdr[EndDateOrdinal].ToDate(),
                        ActivityId = rdr[ActivityIdOrdinal].ToStr(),
                        Notifications = rdr[NotificationsOrdinal].ToStr(),
                        Status = rdr[StatusOrdinal].ToInt32()
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

    public Activity GetActivity(Activity activityInfo)
    {
        Activity returnValue = null;

        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Activity_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_ActivityId", () => activityInfo.Id, activityInfo is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Activity"));
                command.Parameters.AddNull("_AssetCode");
                command.Parameters.AddNull("_StartDate");
                command.Parameters.AddNull("_EndDate");
                command.Parameters.AddNull("_AssetLevel");
                command.Parameters.AddNull("_Origin");
                //command.Parameters.AddWithValue("_Origin", activityInfo.Orgin);
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int ActivityIdOrdinal = rdr.GetOrdinal("ActivityId");
                int NameOrdinal = rdr.GetOrdinal("Name");
                int ActivityClassIdOrdinal = rdr.GetOrdinal("ActivityClassId");
                int ActivityTypeCodeOrdinal = rdr.GetOrdinal("ActivityTypeCode");
                int InterventionIdOrdinal = rdr.GetOrdinal("InterventionId");
                int SourceIdOrdinal = rdr.GetOrdinal("SourceId");
                int AssetCodeOrdinal = rdr.GetOrdinal("AssetCode");
                int ImageOrdinal = rdr.GetOrdinal("Image");
                int EmployeeCodeOrdinal = rdr.GetOrdinal("EmployeeCode");
                int IncludedAssetsOrdinal = rdr.GetOrdinal("IncludedAssets");
                int IsAvailableOrdinal = rdr.GetOrdinal("IsAvailable");
                int DescriptionOrdinal = rdr.GetOrdinal("Description");
                int RequiresInstructionsOrdinal = rdr.GetOrdinal("RequiresInstructions");
                int RequiresNotificationsOrdinal = rdr.GetOrdinal("RequiresNotifications");
                int ProcedureIdOrdinal = rdr.GetOrdinal("ProcedureId");
                int CodeShiftStatusOrdinal = rdr.GetOrdinal("CodeShiftStatus");
                int StartDateOrdinal = rdr.GetOrdinal("StartDate");
                int EndDateOrdinal = rdr.GetOrdinal("EndDate");

                int StatusOrdinal = rdr.GetOrdinal("Status");
                int EventTypeOrdinal = rdr.GetOrdinal("EventType");
                int OriginOrdinal = rdr.GetOrdinal("Origin");
                int WorkDayOrdinal = rdr.GetOrdinal("WorkDay");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new Activity
                    {
                        Id = rdr[ActivityIdOrdinal].ToStr(),
                        Name = rdr[NameOrdinal].ToStr(),
                        ActivityClassId = rdr[ActivityClassIdOrdinal].ToInt32(),
                        ActivityTypeId = rdr[ActivityTypeCodeOrdinal].ToStr(),
                        InterventionId = rdr[InterventionIdOrdinal].ToStr(),
                        SourceId = rdr[SourceIdOrdinal].ToStr(),
                        AssetId = rdr[AssetCodeOrdinal].ToStr(),
                        Image = rdr[ImageOrdinal].ToStr(),
                        EmployeeCode = rdr[EmployeeCodeOrdinal].ToStr(),
                        IncludedAssets = rdr[IncludedAssetsOrdinal].ToStr(),
                        IsAvailable = rdr[IsAvailableOrdinal].ToBool(),
                        Description = rdr[DescriptionOrdinal].ToStr(),
                        RequiresInstructions = rdr[RequiresInstructionsOrdinal].ToBool(),
                        RequiresNotifications = rdr[RequiresNotificationsOrdinal].ToBool(),
                        ParentId = rdr[ProcedureIdOrdinal].ToStr(),
                        CodeShiftStatus = rdr[CodeShiftStatusOrdinal].ToStr(),
                        Schedule = new ActivitySchedule
                        {
                            StartDate = rdr[StartDateOrdinal].ToDate(),
                            EndDate = rdr[EndDateOrdinal].ToDate()
                        },
                        CurrentProcessMaster = new Procedure
                        {
                            ProcedureId = rdr[ProcedureIdOrdinal].ToStr(),
                        },
                        Status = (Status)rdr[StatusOrdinal].ToInt32(),
                        EventType = rdr[EventTypeOrdinal].ToStr(),
                        Origin = rdr[OriginOrdinal].ToStr(),
                        WorkDay = rdr[WorkDayOrdinal].ToBool()
                    };
                }
                _ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int DetailActivityIdOrdinal = rdr.GetOrdinal("ActivityId");
                int SeriesIdOrdinal = rdr.GetOrdinal("SeriesId");
                int DetailStartDateOrdinal = rdr.GetOrdinal("StartDate");
                int DetailEndDateOrdinal = rdr.GetOrdinal("EndDate");
                int DurationOrdinal = rdr.GetOrdinal("Duration");
                int DurationUnitOrdinal = rdr.GetOrdinal("DurationUnit");
                int DurationInSecOrdinal = rdr.GetOrdinal("DurationInSec");
                int EndModeOrdinal = rdr.GetOrdinal("EndMode");
                int EndValueOrdinal = rdr.GetOrdinal("EndValue");
                int FreqModeOrdinal = rdr.GetOrdinal("FreqMode");
                int FreqValueOrdinal = rdr.GetOrdinal("FreqValue");
                int OneTimeOrdinal = rdr.GetOrdinal("OneTime");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue.Schedule ??= new ActivitySchedule();
                    returnValue.Schedule.ActivityId = rdr[DetailActivityIdOrdinal].ToStr();
                    returnValue.Schedule.SeriesId = rdr[SeriesIdOrdinal].ToStr();
                    returnValue.Schedule.StartDate = rdr[DetailStartDateOrdinal].ToDate();
                    returnValue.Schedule.EndDate = rdr[DetailEndDateOrdinal].ToDate();
                    returnValue.Schedule.Duration = rdr[DurationOrdinal].ToDouble();
                    returnValue.Schedule.DurationUnit = rdr[DurationUnitOrdinal].ToInt32();
                    returnValue.Schedule.DurationInSec = rdr[DurationInSecOrdinal].ToInt32();
                    returnValue.Schedule.EndMode = (EndMode)rdr[EndModeOrdinal].ToInt32();
                    returnValue.Schedule.EndValue = rdr[EndValueOrdinal].ToStr();
                    returnValue.Schedule.FrequencyMode = rdr[FreqModeOrdinal].ToStr();
                    returnValue.Schedule.FreqValue = rdr[FreqValueOrdinal].ToDouble();
                    returnValue.Schedule.OneTime = rdr[OneTimeOrdinal].ToInt32().ToBool();
                    //returnValue.Schedule.DayAuxiliar = rdr["DayAux"].ToStr();
                    //returnValue.Schedule.WeekDayAuxiliar = rdr["WeekDayAux"].ToStr();
                    //returnValue.Schedule.MonthAuxiliar = rdr["MonthAux"].ToStr();
                }
                _ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int RequirementTypeIdOrdinal = rdr.GetOrdinal("RequirementTypeId");
                int RequirementSourceIdOrdinal = rdr.GetOrdinal("RequirementSourceId");
                int QuantityOrdinal = rdr.GetOrdinal("Quantity");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    ActivityRequirement element = new()
                    {
                        RequirementTypeId = rdr[RequirementTypeIdOrdinal].ToInt32(),
                        RequirementSourceId = rdr[RequirementSourceIdOrdinal].ToStr(),
                        Quantity = rdr[QuantityOrdinal].ToDouble()
                    };
                    (returnValue.Requirements ??= []).Add(element);
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

    public Activity CreateActivity(Activity activityInfo, User systemOperator)

    {
        Activity returnValue = activityInfo;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Activity_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                _ = command.Parameters.AddWithValue("_Name", activityInfo.Name);
                _ = command.Parameters.AddWithValue("_ActivityClassId", activityInfo.ActivityClassId);
                _ = command.Parameters.AddWithValue("_ActivityTypeCode", activityInfo.ActivityTypeId);
                _ = command.Parameters.AddWithValue("_InterventionId", activityInfo.InterventionId);
                _ = command.Parameters.AddWithValue("_SourceId", activityInfo.SourceId);
                _ = command.Parameters.AddWithValue("_AssetId", activityInfo.AssetId);
                _ = command.Parameters.AddWithValue("_AssetTypeCode", activityInfo.AssetTypeCode);
                _ = command.Parameters.AddWithValue("_AssetCode", activityInfo.AssetCode);
                _ = command.Parameters.AddWithValue("_EmployeeCode", activityInfo.EmployeeCode);
                _ = command.Parameters.AddWithValue("_IncludedAssets", activityInfo.IncludedAssets);
                _ = command.Parameters.AddWithValue("_IsAvailable", activityInfo.IsAvailable);
                _ = command.Parameters.AddWithValue("_Description", activityInfo.Description);
                _ = command.Parameters.AddWithValue("_RequiresNotifications", activityInfo.RequiresNotifications);
                _ = command.Parameters.AddWithValue("_RequiresInstructions", activityInfo.RequiresInstructions);
                _ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
                _ = command.Parameters.AddWithValue("_OperatorEmployee", systemOperator.EmployeeId);
                _ = command.Parameters.AddWithValue("_ProcedureId", activityInfo.ParentId);
                _ = command.Parameters.AddWithValue("_Status", 1);

                _ = command.Parameters.AddWithValue("_WorkDay", activityInfo.WorkDay);
                _ = command.Parameters.AddWithValue("_EventType", activityInfo.EventType);
                _ = command.Parameters.AddWithValue("_Origin", activityInfo.Origin);
                _ = command.Parameters.AddWithValue("_CodeShiftStatus", activityInfo.CodeShiftStatus);
                _ = command.Parameters.AddWithValue("_IsMandatory", activityInfo.IsMandatory);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue.Id = rdr["ActivityId"].ToStr();
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
    public MessageBroker ActivityMergeSchedule(Activity activityInfo, User systemOperator)
    {
        MessageBroker returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Activity_Schedule_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_ActivityId", activityInfo.Id, activityInfo is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Activity Id"));
                _ = command.Parameters.AddWithValue("_SeriesId", activityInfo.Schedule.SeriesId);
                _ = command.Parameters.AddWithValue("_StartDate", activityInfo.Schedule.StartDate);
                _ = command.Parameters.AddWithValue("_EndDate", activityInfo.Schedule.EndDate);
                _ = command.Parameters.AddWithValue("_DurationinSec", activityInfo.Schedule.DurationInSec);
                _ = command.Parameters.AddWithValue("_EndMode", activityInfo.Schedule.EndMode);
                _ = command.Parameters.AddWithValue("_EndValue", activityInfo.Schedule.EndValue);
                _ = command.Parameters.AddWithValue("_FreqMode", activityInfo.Schedule.FrequencyMode);
                _ = command.Parameters.AddWithValue("_FreqValue", activityInfo.Schedule.FreqValue);
                _ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
                _ = command.Parameters.AddWithValue("_OperatorEmployee", systemOperator.EmployeeId);
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                DataTable schemaTable = rdr.GetSchemaTableAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                HashSet<string> columns = schemaTable?.Rows.Cast<DataRow>()
                                .Select(row => row["ColumnName"].ToString())
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                int? elementIdOrdinal = columns?.Contains("ElementId") == true ? rdr.GetOrdinal("ElementId") : (int?)null;
                int? elementValueOrdinal = columns?.Contains("ElementValue") == true ? rdr.GetOrdinal("ElementValue") : (int?)null;
                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new MessageBroker
                    {
                        Type = MessageBrokerType.ActivitySchedule,
                        ElementId = elementIdOrdinal.HasValue ? rdr[elementIdOrdinal.Value].ToString() : null,
                        ElementValue = elementValueOrdinal.HasValue ? rdr[elementValueOrdinal.Value].ToString() : null,
                        MachineId = null,
                        Aux = null
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
    public bool DeleteActivity(Activity activityInfo, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Activity_DEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_ActivityId", activityInfo.Id, activityInfo is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Activity Id"));
                _ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
                _ = command.Parameters.AddWithValue("_InstanceId", activityInfo.InstanceId);
                _ = command.Parameters.AddWithValue("_EditSeries", activityInfo.EditSeries);
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
    #endregion Activity
}