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

public class BinLocationRepo : IBinLocationRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public BinLocationRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }

    #region BinLocation
    public List<BinLocation> ListBinLocation(string Code = "", DateTime? DeltaDate = null)
	{
		List<BinLocation> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_BinLocation_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				if (string.IsNullOrEmpty(Code))
				{
					command.Parameters.AddNull("_Code");
				}
				else
				{
					_ = command.Parameters.AddWithValue("_Code", Code);
				}
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				int CodeOrdinal = rdr.GetOrdinal("Code");
				int NameOrdinal = rdr.GetOrdinal("Name");
				int AisleOrdinal = rdr.GetOrdinal("Aisle");
				int RackOrdinal = rdr.GetOrdinal("Rack");
				int LevelOrdinal = rdr.GetOrdinal("Level");
				int ColOrdinal = rdr.GetOrdinal("Col");
				int ImageOrdinal = rdr.GetOrdinal("Image");
				int WarehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
				int CreateDateOrdinal = rdr.GetOrdinal("CreateDate");
				int CreateUserOrdinal = rdr.GetOrdinal("CreateUser");
				int StatusOrdinal = rdr.GetOrdinal("Status");
				int LogDetailIdOrdinal = rdr.GetOrdinal("LogDetailId");

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					BinLocation element = new()
					{
						LocationCode = rdr[CodeOrdinal].ToStr(),
						Name = rdr[NameOrdinal].ToStr(),
						Aisle = rdr[AisleOrdinal].ToStr(),
						Rack = rdr[RackOrdinal].ToStr(),
						Level = rdr[LevelOrdinal].ToStr(),
						Column = rdr[ColOrdinal].ToStr(),
						Image = rdr[ImageOrdinal].ToStr(),
						WarehouseId = rdr[WarehouseCodeOrdinal].ToStr(),
						CreateDate = rdr[CreateDateOrdinal].ToDate(),
						CreateUser = new User(rdr[CreateUserOrdinal].ToInt32()),
						Status = (Status)rdr[StatusOrdinal].ToInt32(),
						InventoryStatusCodes = [],
						LogDetailId = rdr[LogDetailIdOrdinal].ToStr()
					};

					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						element.UpdateDate = rdr["UpdateDate"].ToDate();
						element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
					}

					(returnValue ??= []).Add(element);
				}
				if (rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						BinLocation element = returnValue.FirstOrDefault(x => x.LocationCode == rdr["BinLocationCode"].ToStr());
						element?.InventoryStatusCodes.Add(rdr["InventoryStatusCode"].ToStr());
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

	public ResponseData MergeBinLocation(BinLocation BinLocationInfo, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_BinLocation_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_IsValidation", Validation);
				_ = command.Parameters.AddWithValue("_Code", BinLocationInfo.LocationCode);
				_ = command.Parameters.AddWithValue("_Name", BinLocationInfo.Name);
				_ = command.Parameters.AddWithValue("_Aisle", BinLocationInfo.Aisle);
				_ = command.Parameters.AddWithValue("_Rack", BinLocationInfo.Rack);
				_ = command.Parameters.AddWithValue("_Level", BinLocationInfo.Level);
				_ = command.Parameters.AddWithValue("_Column", BinLocationInfo.Column);
				_ = command.Parameters.AddWithValue("_WarehouseCode", BinLocationInfo.WarehouseId);
				command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				_ = command.Parameters.AddWithValue("_Status", BinLocationInfo.Status.ToInt32());
				_ = command.Parameters.AddWithValue("_InventoryStatusCode", JsonConvert.SerializeObject(BinLocationInfo.InventoryStatusCodes));
				// command.Parameters.AddWithValue("_Level", Level);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				int ActionOrdinal = rdr.GetOrdinal("Action");
				int IsSuccessOrdinal = rdr.GetOrdinal("IsSuccess");
				int CodeOrdinal = rdr.GetOrdinal("Code");
				int MessageOrdinal = rdr.GetOrdinal("Message");

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ResponseData
					{
						Action = (ActionDB)rdr[ActionOrdinal].ToInt32(),
						IsSuccess = rdr[IsSuccessOrdinal].ToInt32().ToBool(),
						Code = rdr[CodeOrdinal].ToStr(),
						Message = rdr[MessageOrdinal].ToStr(),
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
    #endregion BinLocation
}
