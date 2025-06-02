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

public class ComponentRepo : IComponentRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public ComponentRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region Component

    /// <summary>
    ///
    /// </summary>
    public ResponseData MergeProduct(Component componentInfo, User systemOperator, bool Validation, LevelMessage Level)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Item_Product_MRG", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_Code", componentInfo.Code, !string.IsNullOrEmpty(componentInfo.Code));
                command.Parameters.AddCondition("_Name", componentInfo.Name, !string.IsNullOrEmpty(componentInfo.Name));
                command.Parameters.AddWithValue("_Image", componentInfo.Image);
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddWithValue("_Status", componentInfo.Status);
                command.Parameters.AddWithValue("_Tracking", componentInfo.Tracking);
                command.Parameters.AddWithValue("_StockType", componentInfo.Type);
                command.Parameters.AddWithValue("_Maker", componentInfo.Maker);
                command.Parameters.AddCondition("_ItemGroupCode", componentInfo.InventoryId, !string.IsNullOrEmpty(componentInfo.InventoryId));
                command.Parameters.AddWithValue("_ManagedBy", componentInfo.ManagedBy);
                command.Parameters.AddWithValue("_InventoryUnit", componentInfo.UnitInventory);
                command.Parameters.AddWithValue("_ProductionUnit", componentInfo.UnitProduction);
                command.Parameters.AddCondition("_UnitTypes", componentInfo.UnitTypesToJSON, componentInfo.UnitTypes?.Count > 0);
                command.Parameters.AddWithValue("_IsValidation", Validation);
                command.Parameters.AddWithValue("_Level", Level);
                command.Parameters.AddWithValue("_EnableSchedule", componentInfo.Schedule);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
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
    #endregion Component
}