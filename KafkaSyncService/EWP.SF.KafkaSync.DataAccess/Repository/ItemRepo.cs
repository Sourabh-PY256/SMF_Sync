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

namespace EWP.SF.KafkaSync.DataAccess;

public class ItemRepo : IItemRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public ItemRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region Item
    public ResponseData MergeComponentBulk(string Json, User systemOperator, bool Validation)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Item_BLK", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_JSON", Json, !string.IsNullOrEmpty(Json));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

                // command.Parameters.AddWithValue("_Level", Level);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int ActionOrdinal = rdr.GetOrdinal("Action");
                int IsSuccessOrdinal = rdr.GetOrdinal("IsSuccess");
                int CodeOrdinal = rdr.GetOrdinal("Code");
                int MessageOrdinal = rdr.GetOrdinal("Message");
                int IdOrdinal = rdr.GetOrdinal("Id");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new ResponseData
                    {
                        Action = (ActionDB)rdr[ActionOrdinal].ToInt32(),
                        IsSuccess = rdr[IsSuccessOrdinal].ToInt32().ToBool(),
                        Code = rdr[CodeOrdinal].ToStr(),
                        Message = rdr[MessageOrdinal].ToStr(),
                        Id = rdr[IdOrdinal].ToStr()
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
    
    public List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null)
    {
        List<MeasureUnit> returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Unit_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_UnitType", () => unitType.Value.ToInt32(), unitType.HasValue);
                command.Parameters.AddCondition("_UnitId", unitId, !string.IsNullOrEmpty(unitId));
                command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    MeasureUnit element = new()
                    {
                        Id = rdr["Code"].ToStr(),
                        Code = rdr["Code"].ToStr(),
                        Type = (UnitType)rdr["UnitTypeId"].ToInt32(),
                        TypeName = rdr["UnitType"].ToStr(),
                        Name = rdr["Name"].ToStr(),
                        Factor = rdr["Factor"].ToDecimal(),
                        CreationDate = rdr["CreateDate"].ToDate(),
                        CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                        Status = (Status)rdr["Status"].ToInt32(),
                        IsProductionResult = rdr["IsProductionResult"].ToBool(),
                        StatusMessage = rdr["StatusMessage"].ToStr(),
                        Image = rdr["Image"].ToStr(),
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
    public async Task<List<Component>> ListComponents(string componentId, bool ignoreImages = false, string filter = "", DateTime? DeltaDate = null, CancellationToken cancellationToken = default)
    {
        List<Component> returnValue = [];

        await using EWP_Connection connection = new(ConnectionString);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using EWP_Command command = new("SP_SF_Item_SEL", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 200
            };

            await using (command.ConfigureAwait(false))
            {
                command.Parameters.AddCondition("_Item", componentId, !string.IsNullOrEmpty(componentId));
                command.Parameters.AddCondition("_Filter", filter, !string.IsNullOrEmpty(filter));
                command.Parameters.AddNull("_Warehouse");
                command.Parameters.AddNull("_Version");
                command.Parameters.AddNull("_Sequence");
                command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

                try
                {
                    MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

                    await using (rdr.ConfigureAwait(false))
                    {
                        // Declare GetOrdinal variables for better performance
                        int codeOrdinal = rdr.GetOrdinal("Code");
                        int nameOrdinal = rdr.GetOrdinal("Name");
                        int itemTypeOrdinal = rdr.GetOrdinal("ItemType");
                        int unitTypeOrdinal = rdr.GetOrdinal("UnitType");
                        int unitIdOrdinal = rdr.GetOrdinal("UnitId");
                        int createDateOrdinal = rdr.GetOrdinal("CreateDate");
                        int createUserOrdinal = rdr.GetOrdinal("CreateUser");
                        int statusOrdinal = rdr.GetOrdinal("Status");
                        int stockTypeOrdinal = rdr.GetOrdinal("StockType");
                        int makerOrdinal = rdr.GetOrdinal("Maker");
                        int itemGroupCodeOrdinal = rdr.GetOrdinal("ItemGroupCode");
                        int managedByOrdinal = rdr.GetOrdinal("ManagedBy");
                        int managedByNameOrdinal = rdr.GetOrdinal("ManagedByName");
                        int inventoryUnitOrdinal = rdr.GetOrdinal("InventoryUnit");
                        int productionUnitOrdinal = rdr.GetOrdinal("ProductionUnit");
                        int supplyLeadTimeOrdinal = rdr.GetOrdinal("SupplyLeadTime");
                        int safetyQtyOrdinal = rdr.GetOrdinal("SafetyQty");
                        int enableScheduleOrdinal = rdr.GetOrdinal("EnableSchedule");
                        int enableProductSyncOrdinal = rdr.GetOrdinal("EnableProductSync");
                        int logDetailIdOrdinal = rdr.GetOrdinal("LogDetailId");
                        int isStockOrdinal = rdr.GetOrdinal("IsStock");
                        int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                        int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");
                        int imageOrdinal = rdr.GetOrdinal("Image");
                        while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            Component element = new()
                            {
                                Id = rdr[codeOrdinal].ToStr(),
                                ExternalId = rdr[codeOrdinal].ToStr(),
                                Code = rdr[codeOrdinal].ToStr(),
                                Name = rdr[nameOrdinal].ToStr(),
                                ComponentType = (ComponentType)rdr[itemTypeOrdinal].ToInt32(),
                                UnitType = (UnitType)rdr[unitTypeOrdinal].ToInt32(),
                                UnitId = rdr[unitIdOrdinal].ToStr(),
                                CreationDate = !await rdr.IsDBNullAsync(createDateOrdinal, cancellationToken).ConfigureAwait(false) ? rdr[createDateOrdinal].ToDate() : default,
                                CreatedBy = new User(rdr[createUserOrdinal].ToInt32()),
                                Status = (Status)rdr[statusOrdinal].ToInt32(),
                                Type = rdr[stockTypeOrdinal].ToInt32(),
                                Maker = rdr[makerOrdinal].ToStr(),
                                InventoryId = rdr[itemGroupCodeOrdinal].ToStr(),
                                ManagedBy = rdr[managedByOrdinal].ToInt32(),
                                ManagedByName = rdr[managedByNameOrdinal].ToStr(),
                                UnitInventory = rdr[inventoryUnitOrdinal].ToStr(),
                                UnitProduction = rdr[productionUnitOrdinal].ToStr(),
                                SupplyLeadTime = rdr[supplyLeadTimeOrdinal].ToInt32(),
                                SafetyQty = rdr[safetyQtyOrdinal].ToDouble(),
                                Schedule = rdr[enableScheduleOrdinal].ToInt32(),
                                ProductSync = rdr[enableProductSyncOrdinal].ToBool(),
                                LogDetailId = rdr[logDetailIdOrdinal].ToStr(),
                                IsStock = rdr[isStockOrdinal].ToBool()
                            };

                            if (!ignoreImages)
                            {
                                element.Image = rdr[imageOrdinal].ToStr();
                            }

                            if (rdr[updateDateOrdinal].ToDate().Year > 1900)
                            {
                                element.ModifyDate = !await rdr.IsDBNullAsync(updateDateOrdinal, cancellationToken).ConfigureAwait(false) ? rdr[updateDateOrdinal].ToDate() : default;
                                element.ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32());
                            }

                            returnValue.Add(element);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //logger.Error(ex, "An error occurred while fetching components");
                    throw;
                }

                return returnValue;
            }
        }
    }
    #endregion Item
}