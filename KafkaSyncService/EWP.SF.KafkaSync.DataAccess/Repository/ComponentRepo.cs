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
	public async Task<List<Component>> ListComponents(string componentId, bool ignoreImages = false, string filter = "", DateTime? DeltaDate = null, CancellationToken cancel = default)
    {
        List<Component> returnValue = [];

        await using EWP_Connection connection = new(ConnectionString);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancel).ConfigureAwait(false);

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
                    MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);

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
                        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
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
                                CreationDate = !await rdr.IsDBNullAsync(createDateOrdinal, cancel).ConfigureAwait(false) ? rdr[createDateOrdinal].ToDate() : default,
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
                                element.ModifyDate = !await rdr.IsDBNullAsync(updateDateOrdinal, cancel).ConfigureAwait(false) ? rdr[updateDateOrdinal].ToDate() : default;
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
    /// <summary>
    ///
    /// </summary>
    public async Task<List<Component>> ListProducts(string id, string warehouseCode = "", DateTime? deltaDate = null, CancellationToken cancel = default)
    {
        List<Component> returnValue = null;
        List<ProductVerions> versions = null;

        await using EWP_Connection connection = new(ConnectionString);

        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancel).ConfigureAwait(false);

            await using EWP_Command command = new("SP_SF_Product_SEL", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await using (command.ConfigureAwait(false))
            {
                command.Parameters.Clear();
                command.Parameters.AddNull("_Code");
                command.Parameters.AddCondition("_Warehouse", warehouseCode, !string.IsNullOrEmpty(warehouseCode));
                command.Parameters.AddNull("_Version");
                command.Parameters.AddNull("_Sequence");
                command.Parameters.AddNull("_Id");
                command.Parameters.AddCondition("_DeltaDate", deltaDate, deltaDate.HasValue);

                try
                {
                    // Execute reader asynchronously
                    MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);

                    await using (rdr.ConfigureAwait(false))
                    {
                        // Get ordinals for efficient access
                        int itemCodeOrdinal = rdr.GetOrdinal("ItemCode");
                        int codeOrdinal = rdr.GetOrdinal("Code");
                        int nameOrdinal = rdr.GetOrdinal("Name");
                        int itemTypeOrdinal = rdr.GetOrdinal("ItemType");
                        int unitTypeOrdinal = rdr.GetOrdinal("UnitType");
                        int unitOrdinal = rdr.GetOrdinal("Unit");
                        int createDateOrdinal = rdr.GetOrdinal("CreateDate");
                        int createUserOrdinal = rdr.GetOrdinal("CreateUser");
                        int statusOrdinal = rdr.GetOrdinal("Status");
                        int productIdOrdinal = rdr.GetOrdinal("ProductId");
                        int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
                        int versionOrdinal = rdr.GetOrdinal("Version");
                        int logDetailIdOrdinal = rdr.GetOrdinal("LogDetailId");
                        int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                        int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");
                        int imageOrdinal = rdr.GetOrdinal("Image");
                        // Read the first result set (products)
                        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
                        {
                            Component element = new()
                            {
                                Id = rdr[itemCodeOrdinal].ToStr(),
                                ExternalId = rdr[codeOrdinal].ToStr(),
                                Code = rdr[codeOrdinal].ToStr(),
                                Name = rdr[nameOrdinal].ToStr(),
                                ComponentType = (ComponentType)rdr[itemTypeOrdinal].ToInt32(),
                                Image = rdr[imageOrdinal].ToStr(),
                                UnitType = (UnitType)rdr[unitTypeOrdinal].ToInt32(),
                                UnitId = rdr[unitOrdinal].ToStr(),
                                CreationDate = rdr[createDateOrdinal].ToDate(),
                                CreatedBy = new User(rdr[createUserOrdinal].ToInt32()),
                                Status = (Status)rdr[statusOrdinal].ToInt32(),
                                ProcessEntryId = rdr[productIdOrdinal].ToStr(),
                                WarehouseId = rdr[warehouseCodeOrdinal].ToStr(),
                                Version = rdr[versionOrdinal].ToInt32(),
                                LogDetailId = rdr[logDetailIdOrdinal].ToStr()
                            };

                            if (rdr[updateDateOrdinal].ToDate().Year > 1900)
                            {
                                element.ModifyDate = rdr[updateDateOrdinal].ToDate();
                                element.ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32());
                            }
                            (returnValue ??= []).Add(element);
                        }

                        // Move to the next result set (product versions)
                        await rdr.NextResultAsync(cancel).ConfigureAwait(false);

                        int versionCodeOrdinal = rdr.GetOrdinal("ItemCode");
                        int versionOrdinal2 = rdr.GetOrdinal("Version");
                        int versionUpdateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                        int versionStatusOrdinal = rdr.GetOrdinal("Status");
                        int versionWarehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");

                        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
                        {
                            ProductVerions version = new()
                            {
                                Code = rdr[versionCodeOrdinal].ToStr(),
                                Version = rdr[versionOrdinal2].ToInt32(),
                                UpdateDate = rdr[versionUpdateDateOrdinal].ToDate(),
                                Status = rdr[versionStatusOrdinal].ToInt32(),
                                Warehouse = rdr[versionWarehouseCodeOrdinal].ToStr()
                            };
                            (versions ??= []).Add(version);
                        }

                        // Link versions to the components
                        if (returnValue is not null && versions is not null)
                        {
                            returnValue.ForEach(product =>
                                product.Versions = [.. versions.Where(p =>
                                        p.Code == product.Code
                                        && p.Warehouse == product.WarehouseId)]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception if a logger is available
                    //logger?.Error(ex);
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

    /// <summary>
    ///
    /// </summary>
    public ResponseData MergeComponent(Component componentInfo, User systemOperator, bool Validation)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Item_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_Code", componentInfo.Code, !string.IsNullOrEmpty(componentInfo.Code));
                command.Parameters.AddCondition("_Name", componentInfo.Name, !string.IsNullOrEmpty(componentInfo.Name));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddWithValue("_Status", componentInfo.Status);
                command.Parameters.AddWithValue("_Tracking", componentInfo.Tracking);
                command.Parameters.AddWithValue("_StockType", componentInfo.Type);
                command.Parameters.AddWithValue("_Maker", componentInfo.Maker);
                command.Parameters.AddCondition("_ItemGroupCode", componentInfo.InventoryId, !string.IsNullOrEmpty(componentInfo.InventoryId));
                command.Parameters.AddWithValue("_ManagedBy", componentInfo.ManagedBy);
                command.Parameters.AddWithValue("_InventoryUnit", componentInfo.UnitInventory);
                command.Parameters.AddWithValue("_ProductionUnit", componentInfo.UnitProduction);
                command.Parameters.AddCondition("_UnitTypes", () => componentInfo.UnitTypes.Count > 0 ? componentInfo.UnitTypesToJSON() : "", componentInfo.UnitTypes is not null);
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
                command.Parameters.AddWithValue("_IsValidation", Validation);
                command.Parameters.AddWithValue("_SupplyLeadTime", componentInfo.SupplyLeadTime);
                command.Parameters.AddWithValue("_SafetyQty", componentInfo.SafetyQty);
                command.Parameters.AddWithValue("_Schedule", componentInfo.Schedule);
                command.Parameters.AddWithValue("_ProductSync", componentInfo.ProductSync);
                command.Parameters.AddWithValue("_IsStock", componentInfo.IsStock);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int IdOrdinal = rdr.GetOrdinal("Id");
                int ActionOrdinal = rdr.GetOrdinal("Action");
                int IsSuccessOrdinal = rdr.GetOrdinal("IsSuccess");
                int CodeOrdinal = rdr.GetOrdinal("Code");
                int MessageOrdinal = rdr.GetOrdinal("Message");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    componentInfo.Id = rdr[IdOrdinal].ToStr();
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
    /// <summary>
	///
	/// </summary>
	public Component GetComponentByCode(string Code)
    {
        Component returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                const string script = "SP_SF_Item_By_Code_SEL";
                using EWP_Command command = new(script, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code));
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new Component
                    {
                        Id = rdr["Code"].ToStr(),
                        ExternalId = rdr["Code"].ToStr(),
                        Code = rdr["Code"].ToStr(),
                        Name = rdr["Name"].ToStr(),
                        ComponentType = (ComponentType)rdr["ItemType"].ToInt32(),
                        UnitType = (UnitType)rdr["UnitType"].ToInt32(),
                        UnitId = rdr["UnitId"].ToStr(),
                        CreationDate = rdr["CreateDate"].ToDate(),
                        CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                        Status = (Status)rdr["Status"].ToInt32(),
                        Type = rdr["StockType"].ToInt32(),
                        Maker = rdr["Maker"].ToStr(),
                        InventoryId = rdr["ItemGroupCode"].ToStr(),
                        ManagedBy = rdr["ManagedBy"].ToInt32(),
                        UnitInventory = rdr["InventoryUnit"].ToStr(),
                        UnitProduction = rdr["ProductionUnit"].ToStr(),
                        Image = rdr["Image"].ToStr(),
                        SupplyLeadTime = rdr["SupplyLeadTime"].ToInt32(),
                        SafetyQty = rdr["SafetyQty"].ToDouble(),
                        Schedule = rdr["EnableSchedule"].ToInt32()
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
    /// <summary>
	///
	/// </summary>
	public bool MergeProcessEntryAttributes(string processEntryId, string JSON, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ProductAttribute_MRG", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_ProductId", processEntryId, !string.IsNullOrEmpty(processEntryId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_JSON", JSON, !string.IsNullOrEmpty(JSON));

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
    /// <summary>
    ///
    /// </summary>
    public async Task<List<ProcessEntry>> ListProcessEntry(string code, string warehouse, int? version, int? sequence = 0, string id = "", CancellationToken cancel = default)
    {
        List<ProcessEntry> returnValue = null;

        await using EWP_Connection connection = new(ConnectionString);

        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancel).ConfigureAwait(false);

            await using EWP_Command command = new("SP_SF_Product_SEL", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await using (command.ConfigureAwait(false))
            {
                command.Parameters.Clear();
                command.Parameters.AddCondition("_Code", code, !string.IsNullOrEmpty(code));
                command.Parameters.AddCondition("_Warehouse", warehouse, !string.IsNullOrEmpty(warehouse));
                command.Parameters.AddCondition("_Version", version, version.HasValue);
                command.Parameters.AddCondition("_Sequence", sequence, sequence.HasValue);
                command.Parameters.AddCondition("_Id", id, !string.IsNullOrEmpty(id));
                command.Parameters.AddNull("_DeltaDate");

                try
                {
                    MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);

                    await using (rdr.ConfigureAwait(false))
                    {
                        // Get ordinals for efficient access
                        int productIdOrdinal = rdr.GetOrdinal("ProductId");
                        int itemCodeOrdinal = rdr.GetOrdinal("ItemCode");
                        int codeOrdinal = rdr.GetOrdinal("Code");
                        int nameOrdinal = rdr.GetOrdinal("Name");
                        int quantityOrdinal = rdr.GetOrdinal("Quantity");
                        int minQuantityOrdinal = rdr.GetOrdinal("MinQuantity");
                        int maxQuantityOrdinal = rdr.GetOrdinal("MaxQuantity");
                        int unitTypeOrdinal = rdr.GetOrdinal("UnitType");
                        int unitOrdinal = rdr.GetOrdinal("Unit");
                        int factorOrdinal = rdr.GetOrdinal("Factor");
                        int createDateOrdinal = rdr.GetOrdinal("CreateDate");
                        int createUserOrdinal = rdr.GetOrdinal("CreateUser");
                        int statusOrdinal = rdr.GetOrdinal("Status");
                        int timeOrdinal = rdr.GetOrdinal("Time");
                        int editableOrdinal = rdr.GetOrdinal("Editable");
                        int productTypeOrdinal = rdr.GetOrdinal("ProductType");
                        int instrucctionByDensityOrdinal = rdr.GetOrdinal("InstrucctionbyDensity");
                        int instrucctionByPhOrdinal = rdr.GetOrdinal("InstrucctionByPh");
                        int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
                        int earlierVersionOrdinal = rdr.GetOrdinal("EarlierVersion");
                        int versionOrdinal = rdr.GetOrdinal("Version");
                        int sequenceOrdinal = rdr.GetOrdinal("Sequence");
                        int validFromOrdinal = rdr.GetOrdinal("ValidFrom");
                        int validToOrdinal = rdr.GetOrdinal("ValidTo");
                        int scrapOrdinal = rdr.GetOrdinal("ScrapQty");
                        int supplyLeadTimeOrdinal = rdr.GetOrdinal("SupplyLeadTime");
                        int enableScheduleOrdinal = rdr.GetOrdinal("EnableSchedule");
                        int commentsOrdinal = rdr.GetOrdinal("Comments");
                        int formulaOrdinal = rdr.GetOrdinal("Formula");
                        int logDetailIdOrdinal = rdr.GetOrdinal("LogDetailId");
                        int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                        int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");
                        int bomVersionOrdinal = rdr.GetOrdinal("BomVersion");
                        int bomSequenceOrdinal = rdr.GetOrdinal("BomSequence");
                        int routeVersionOrdinal = rdr.GetOrdinal("RouteVersion");
                        int routeSequenceOrdinal = rdr.GetOrdinal("RouteSequence");
                        // Read the first result set (ProcessEntry data)
                        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
                        {
                            bool isEditable = false;
                            try
                            {
                                isEditable = rdr[editableOrdinal].ToBool();
                            }
                            catch
                            {
                                // Handle potential conversion issues
                            }

                            ProcessEntry element = new()
                            {
                                Id = rdr[productIdOrdinal].ToStr(),
                                ComponentId = rdr[itemCodeOrdinal].ToStr(),
                                Code = rdr[codeOrdinal].ToStr(),
                                Name = rdr[nameOrdinal].ToStr(),
                                Quantity = rdr[quantityOrdinal].ToDouble(),
                                MinQuantity = rdr[minQuantityOrdinal].ToDouble(),
                                MaxQuantity = rdr[maxQuantityOrdinal].ToDouble(),
                                UnitType = (UnitType)rdr[unitTypeOrdinal].ToInt32(),
                                UnitId = rdr[unitOrdinal].ToStr(),
                                Factor = rdr[factorOrdinal].ToDouble(),
                                CreationDate = rdr[createDateOrdinal].ToDate(),
                                CreatedBy = new User(rdr[createUserOrdinal].ToInt32()),
                                Status = (Status)rdr[statusOrdinal].ToInt32(),
                                Time = rdr[timeOrdinal].ToStr(),
                                IsEditable = isEditable,
                                ProductType = (ProductType)rdr[productTypeOrdinal].ToInt32(),
                                InstrucctionbyDensity = rdr[instrucctionByDensityOrdinal].ToStr(),
                                InstrucctionByPh = rdr[instrucctionByPhOrdinal].ToStr(),
                                Warehouse = rdr[warehouseCodeOrdinal].ToStr(),
                                EarlierVersion = rdr[earlierVersionOrdinal].ToInt32(),
                                Version = rdr[versionOrdinal].ToInt32(),
                                Sequence = rdr[sequenceOrdinal].ToInt32(),
                                MakeObsolete = true,
                                ValidFrom = rdr[validFromOrdinal].ToDate(),
                                ValidTo = rdr[validToOrdinal].ToDate(),
                                Scrap = rdr[scrapOrdinal].ToDouble(),
                                SupplyLeadTime = rdr[supplyLeadTimeOrdinal].ToDouble(),
                                Schedule = rdr[enableScheduleOrdinal].ToInt32(),
                                Comments = rdr[commentsOrdinal].ToStr(),
                                Formula = rdr[formulaOrdinal].ToStr(),
                                LogDetailId = rdr[logDetailIdOrdinal].ToStr(),
                                BomVersion = rdr[bomVersionOrdinal].ToInt64(),
                                BomSequence = rdr[bomSequenceOrdinal].ToInt32(),
                                RouteVersion = rdr[routeVersionOrdinal].ToInt64(),
                                RouteSequence = rdr[routeSequenceOrdinal].ToInt32()
                            };

                            if (rdr[updateDateOrdinal].ToDate().Year > 1900)
                            {
                                element.ModifyDate = rdr[updateDateOrdinal].ToDate();
                                element.ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32());
                            }

                            (returnValue ??= []).Add(element);
                        }

                        // Process additional result sets, similar to your original method
                        if (!string.IsNullOrEmpty(id) || (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(warehouse) && sequence > 0))
                        {
                            await ProcessAdditionalResultsAsync(rdr, returnValue, cancel).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception if a logger is available
                    //logger?.Error(ex);
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
    private static async Task ProcessAdditionalResultsAsync(MySqlDataReader rdr, List<ProcessEntry> returnValue, CancellationToken cancel)
    {
        // Processing ProcessEntryProcess
        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
        {
            // Get ordinals for ProcessEntryProcess
            int productIdOrdinal = rdr.GetOrdinal("ProductId");
            int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
            int operationTypeCodeOrdinal = rdr.GetOrdinal("OperationTypeCode");
            int operationSubtypeCodeOrdinal = rdr.GetOrdinal("OperationSubtypeCode");
            int operationClassIdOrdinal = rdr.GetOrdinal("OperationClassId");
            int nameOrdinal = rdr.GetOrdinal("Name");
            int stepOrdinal = rdr.GetOrdinal("Step");
            int sortOrdinal = rdr.GetOrdinal("Sort");
            int isOutputOrdinal = rdr.GetOrdinal("IsOutput");
            int outputItemCodeOrdinal = rdr.GetOrdinal("OutputItemCode");
            int operationTimeOrdinal = rdr.GetOrdinal("OperationTime");
            int availableDevicesOrdinal = rdr.GetOrdinal("AvailableDevices");
            int destFactorOrdinal = rdr.GetOrdinal("DestFactor");
            int transformFactorOrdinal = rdr.GetOrdinal("TransformFactor");
            int outputNameOrdinal = rdr.GetOrdinal("OutputName");
            int outputCodeOrdinal = rdr.GetOrdinal("OutputCode");
            int outputUnitIdOrdinal = rdr.GetOrdinal("OutputUnitId");
            int outputUnitTypeOrdinal = rdr.GetOrdinal("OutputUnitType");
            int setupTimeOrdinal = rdr.GetOrdinal("SetupTime");
            int maxTimeBeforeNextOpOrdinal = rdr.GetOrdinal("MaxTimeBeforeNextOp");
            int slackTimeAfterPrevOpOrdinal = rdr.GetOrdinal("SlackTimeAfterPrevOp");
            int maxOpSpanIncreaseOrdinal = rdr.GetOrdinal("MaxOpSpanIncrease");
            int slackTimeBeforeNextOpOrdinal = rdr.GetOrdinal("SlackTimeBeforeNextOp");
            int transferTypeOrdinal = rdr.GetOrdinal("TransferType");
            int transferQtyOrdinal = rdr.GetOrdinal("TransferQty");
            int spareStringField1Ordinal = rdr.GetOrdinal("SpareStringField1");
            int spareStringField2Ordinal = rdr.GetOrdinal("SpareStringField2");
            int spareNumberFieldOrdinal = rdr.GetOrdinal("SpareNumberField");
            int processTimeTypeOrdinal = rdr.GetOrdinal("ProcessTimeType");
            int outputUnitOrdinal = rdr.GetOrdinal("OutputUnit");
            int quantityOrdinal = rdr.GetOrdinal("Quantity");

            while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
            {
                ProcessEntry entry = returnValue.Find(x => x.Id == rdr[productIdOrdinal].ToStr());
                if (entry is not null)
                {
                    ProcessEntryProcess element = new()
                    {
                        ProcessId = rdr[operationNoOrdinal].ToStr(),
                        ProcessTypeId = rdr[operationTypeCodeOrdinal].ToStr(),
                        ProcessSubTypeId = rdr[operationSubtypeCodeOrdinal].ToStr(),
                        OperationClassId = rdr[operationClassIdOrdinal].ToInt32(),
                        Name = rdr[nameOrdinal].ToStr(),
                        Step = rdr[stepOrdinal].ToInt32(),
                        Sort = rdr[sortOrdinal].ToInt32(),
                        IsOutput = rdr[isOutputOrdinal].ToBool(),
                        Output = rdr[outputItemCodeOrdinal].ToStr(),
                        Time = rdr[operationTimeOrdinal].ToDouble(),
                        AvailableMachines = rdr[availableDevicesOrdinal].ToStr(),
                        DestFactor = rdr[destFactorOrdinal].ToDouble(),
                        TransformFactor = rdr[transformFactorOrdinal].ToDouble(),
                        OutputName = rdr[outputNameOrdinal].ToStr(),
                        OutputCode = rdr[outputCodeOrdinal].ToStr(),
                        OutputUnitId = rdr[outputUnitIdOrdinal].ToStr(),
                        OutputUnitType = rdr[outputUnitTypeOrdinal].ToInt32(),
                        SetupTime = rdr[setupTimeOrdinal].ToDouble(),
                        MaxTimeBeforeNextOp = rdr[maxTimeBeforeNextOpOrdinal].ToStr(),
                        SlackTimeAfterPrevOp = rdr[slackTimeAfterPrevOpOrdinal].ToStr(),
                        MaxOpSpanIncrease = rdr[maxOpSpanIncreaseOrdinal].ToStr(),
                        SlackTimeBeforeNextOp = rdr[slackTimeBeforeNextOpOrdinal].ToStr(),
                        TransferType = rdr[transferTypeOrdinal].ToStr(),
                        TransferQty = rdr[transferQtyOrdinal].ToDouble(),
                        SpareStringField1 = rdr[spareStringField1Ordinal].ToStr(),
                        SpareStringField2 = rdr[spareStringField2Ordinal].ToStr(),
                        SpareNumberField = rdr[spareNumberFieldOrdinal].ToDecimal(),
                        ProcessTimeType = rdr[processTimeTypeOrdinal].IsNull() ? null : rdr[processTimeTypeOrdinal].ToInt32(),

                        Unit = rdr[outputUnitOrdinal].ToStr(),
                        Quantity = rdr[quantityOrdinal].ToDouble(),
                    };

                    (entry.Processes ??= []).Add(element);
                }
            }
        }

        // Processing ProcessEntryComponent
        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
        {
            // Get ordinals for ProcessEntryComponent
            int productIdOrdinal = rdr.GetOrdinal("ProductId");
            int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
            int operationTypeCodeOrdinal = rdr.GetOrdinal("OperationTypeCode");
            int itemCodeOrdinal = rdr.GetOrdinal("ItemCode");
            int stepOrdinal = rdr.GetOrdinal("Step");
            int quantityOrdinal = rdr.GetOrdinal("Quantity");
            int unitOrdinal = rdr.GetOrdinal("Unit");
            int nameOrdinal = rdr.GetOrdinal("Name");
            int codeOrdinal = rdr.GetOrdinal("Code");
            int isBackflushOrdinal = rdr.GetOrdinal("IsBackflush");
            int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
            int inventoryUnitOrdinal = rdr.GetOrdinal("InventoryUnit");
            int lineNoOrdinal = rdr.GetOrdinal("LineNo");
            int lineUIDOrdinal = rdr.GetOrdinal("LineUID");
            int typeOrdinal = rdr.GetOrdinal("Type");
            int isScheduleOrdinal = rdr.GetOrdinal("IsSchedule");
            int sourceOrdinal = rdr.GetOrdinal("Source");
            int enableScheduleOrdinal = rdr.GetOrdinal("EnableSchedule");
            int commentsOrdinal = rdr.GetOrdinal("Comments");

            while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
            {
                ProcessEntry entry = returnValue.Find(x => x.Id == rdr[productIdOrdinal].ToStr());
                if (entry is not null)
                {
                    ProcessEntryComponent element = new()
                    {
                        ProcessId = rdr[operationNoOrdinal].ToStr(),
                        ProcessTypeId = rdr[operationTypeCodeOrdinal].ToStr(),
                        ComponentType = ComponentType.Material,
                        ComponentId = rdr[itemCodeOrdinal].ToStr(),
                        Step = rdr[stepOrdinal].ToInt32(),
                        Quantity = rdr[quantityOrdinal].ToDouble(),
                        UnitId = rdr[unitOrdinal].ToStr(),
                        Name = rdr[nameOrdinal].ToStr(),
                        Code = rdr[codeOrdinal].ToStr(),
                        IsBackflush = rdr[isBackflushOrdinal].ToBool(),
                        WarehouseCode = rdr[warehouseCodeOrdinal].ToStr(),
                        Alternatives = [],
                        ItemUnit = rdr[inventoryUnitOrdinal].ToStr(),
                        LineId = rdr[lineNoOrdinal].ToInt32().ToStr(),
                        LineUID = rdr[lineUIDOrdinal].ToStr(),
                        Class = rdr[typeOrdinal].ToInt32(),
                        IsSchedule = rdr[isScheduleOrdinal].ToBool(),
                        Source = rdr[sourceOrdinal].ToStr(),
                        EnableSchedule = rdr[enableScheduleOrdinal].ToInt32(),
                        Comments = rdr[commentsOrdinal].ToStr(),
                    };
                    (entry.Components ??= []).Add(element);
                }
            }
        }

        // Processing Task
        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
        {
            // Get ordinals for ProcessEntrTask
            int productIdOrdinal = rdr.GetOrdinal("ProductId");
            int activityIdOrdinal = rdr.GetOrdinal("ActivityId");
            int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
            int nameOrdinal = rdr.GetOrdinal("Name");
            int triggerIdOrdinal = rdr.GetOrdinal("TriggerId");
            int sortIdOrdinal = rdr.GetOrdinal("SortId");
            int isMandatoryOrdinal = rdr.GetOrdinal("IsMandatory");
            int versionProcedureOrdinal = rdr.GetOrdinal("VersionProcedure");
            int durationOrdinal = rdr.GetOrdinal("Duration");
            int durationUnitOrdinal = rdr.GetOrdinal("DurationUnit");
            int freqModeOrdinal = rdr.GetOrdinal("FreqMode");
            int freqValueOrdinal = rdr.GetOrdinal("FreqValue");
            while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
            {
                ProcessEntry entry = returnValue.Find(x => x.Id == rdr[productIdOrdinal].ToStr());
                if (entry is not null)
                {
                    Activity element = new()
                    {
                        Id = rdr[activityIdOrdinal].ToStr(),
                        ProcessId = rdr[operationNoOrdinal].ToStr(),
                        Name = rdr[nameOrdinal].ToStr(),
                        TriggerId = rdr[triggerIdOrdinal].ToInt32(),
                        SortId = rdr[sortIdOrdinal].ToInt32(1),
                        IsMandatory = rdr[isMandatoryOrdinal].ToBool(),
                        VersionProcedure = rdr[versionProcedureOrdinal].ToInt32(),
                        Schedule = new ActivitySchedule
                        {
                            DurationInSec = 0,// rdr["DurationInSec"].ToInt32(),
                            Duration = rdr[durationOrdinal].ToDouble(),
                            DurationUnit = rdr[durationUnitOrdinal].ToInt32(),
                            FrequencyMode = rdr[freqModeOrdinal].ToStr(),
                            FreqValue = rdr[freqValueOrdinal].ToDouble()
                        },
                    };
                    (entry.Tasks ??= []).Add(element);
                }
            }
        }

        // Processing Alternatives
        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
        {
            // Get ordinals for ProcessEntryComponent
            int productIdOrdinal = rdr.GetOrdinal("ProductId");
            int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
            int itemCodeOrdinal = rdr.GetOrdinal("ItemCode");
            int alternativeItemCodeOrdinal = rdr.GetOrdinal("AlternativeItemCode");
            int factorOrdinal = rdr.GetOrdinal("Factor");
            int unitOrdinal = rdr.GetOrdinal("Unit");
            int nameOrdinal = rdr.GetOrdinal("Name");
            int codeOrdinal = rdr.GetOrdinal("Code");
            int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
            int coincidenceOrdinal = rdr.GetOrdinal("Coincidence");
            int inventoryUnitOrdinal = rdr.GetOrdinal("InventoryUnit");
            int lineNoOrdinal = rdr.GetOrdinal("LineNo");
            int lineUidOrdinal = rdr.GetOrdinal("LineUID");

            while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
            {
                ProcessEntry entry = returnValue.Find(x => x.Id == rdr[productIdOrdinal].ToStr());
                if (entry is not null)
                {
                    string processId = rdr[operationNoOrdinal].ToStr();
                    string componentId = rdr[itemCodeOrdinal].ToStr();
                    ProcessEntryComponent component = entry.Components?.FirstOrDefault(x => x.ProcessId == processId && x.ComponentId == componentId);
                    if (component is not null)
                    {
                        AlternativeComponent element = new()
                        {
                            SourceId = rdr[alternativeItemCodeOrdinal].ToStr(),
                            Factor = rdr[factorOrdinal].ToDouble(),
                            UnitId = rdr[unitOrdinal].ToStr(),
                            Name = rdr[nameOrdinal].ToStr(),
                            Code = rdr[codeOrdinal].ToStr(),
                            WarehouseCode = rdr[warehouseCodeOrdinal].ToStr(),
                            Coincidence = rdr[coincidenceOrdinal].ToDouble(),
                            ItemUnit = rdr[inventoryUnitOrdinal].ToStr(),
                            LineId = rdr[lineNoOrdinal].ToInt32().ToStr(),
                            LineUID = rdr[lineUidOrdinal].ToStr()
                        };

                        (component.Alternatives ??= []).Add(element);
                    }
                }
            }
        }

        // Processing Subproducts
        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
        {
            // Get ordinals for ProcessEntryComponent
            int productIdOrdinal = rdr.GetOrdinal("ProductId");
            int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
            int itemCodeOrdinal = rdr.GetOrdinal("ItemCode");
            int factorOrdinal = rdr.GetOrdinal("Factor");
            int unitOrdinal = rdr.GetOrdinal("Unit");
            int nameOrdinal = rdr.GetOrdinal("Name");
            int unitTypeOrdinal = rdr.GetOrdinal("UnitType");
            int codeOrdinal = rdr.GetOrdinal("Code");
            int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
            int inventoryUnitOrdinal = rdr.GetOrdinal("InventoryUnit");
            int lineNoOrdinal = rdr.GetOrdinal("LineNo");
            int lineUidOrdinal = rdr.GetOrdinal("LineUID");
            int commentsOrdinal = rdr.GetOrdinal("Comments");

            while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
            {
                ProcessEntry entry = returnValue.Find(x => x.Id == rdr[productIdOrdinal].ToStr());
                if (entry is not null)
                {
                    string processid = rdr[operationNoOrdinal].ToStr();
                    ProcessEntryProcess process = entry.Processes.Find(x => x.ProcessId == processid);
                    if (process is not null)
                    {
                        SubProduct element = new()
                        {
                            ComponentId = rdr[itemCodeOrdinal].ToStr(),
                            Factor = rdr[factorOrdinal].ToDouble(),
                            Name = rdr[nameOrdinal].ToStr(),
                            Code = rdr[codeOrdinal].ToStr(),
                            UnitId = rdr[unitOrdinal].ToStr(),
                            UnitType = rdr[unitTypeOrdinal].ToInt32(),
                            WarehouseCode = rdr[warehouseCodeOrdinal].ToStr(),
                            ItemUnit = rdr[inventoryUnitOrdinal].ToStr(),
                            LineId = rdr[lineNoOrdinal].ToInt32().ToStr(),
                            LineUID = rdr[lineUidOrdinal].ToStr(),
                            Comments = rdr[commentsOrdinal].ToStr()
                        };

                        (process.Subproducts ??= []).Add(element);
                    }
                }
            }
        }

        // Processing ProcessEntryLabor
        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
        {
            // Get ordinals for ProcessEntryLabor
            int productIdOrdinal = rdr.GetOrdinal("ProductId");
            int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
            int positionCodeOrdinal = rdr.GetOrdinal("PositionCode");
            int lineNoOrdinal = rdr.GetOrdinal("LineNo");
            int machineCodeOrdinal = rdr.GetOrdinal("MachineCode");
            int nameOrdinal = rdr.GetOrdinal("Name");
            int quantityOrdinal = rdr.GetOrdinal("Quantity");
            int usageCodeOrdinal = rdr.GetOrdinal("UsageCode");
            int laborTimeOrdinal = rdr.GetOrdinal("LaborTime");
            int scheduleOrdinal = rdr.GetOrdinal("Schedule");
            int costOrdinal = rdr.GetOrdinal("Cost");
            int lineUIDOrdinal = rdr.GetOrdinal("LineUID");
            int sourceOrdinal = rdr.GetOrdinal("Source");
            int commentsOrdinal = rdr.GetOrdinal("Comments");
            int backflushOrdinal = rdr.GetOrdinal("IsBackflush");

            while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
            {
                ProcessEntry entry = returnValue.Find(x => x.Id == rdr[productIdOrdinal].ToStr());

                if (entry is not null)
                {
                    ProcessEntryLabor element = new()
                    {
                        ProcessId = rdr[operationNoOrdinal].ToStr(),
                        LaborId = rdr[positionCodeOrdinal].ToStr(),
                        Id = rdr[positionCodeOrdinal].ToStr(),
                        LineId = rdr[lineNoOrdinal].ToInt32().ToStr(),
                        MachineId = rdr[machineCodeOrdinal].ToStr(),
                        Name = rdr[nameOrdinal].ToStr(),
                        Quantity = rdr[quantityOrdinal].ToDouble(),
                        Usage = rdr[usageCodeOrdinal].ToStr(),
                        LaborTime = rdr[laborTimeOrdinal].ToStr(),
                        Schedule = rdr[scheduleOrdinal].ToBool(),
                        Cost = rdr[costOrdinal].ToDouble(),
                        LineUID = rdr[lineUIDOrdinal].ToStr(),
                        Source = rdr[sourceOrdinal].ToStr(),
                        Comments = rdr[commentsOrdinal].ToStr(),
                        IsBackflush = rdr[backflushOrdinal].ToBool(),
                    };

                    (entry.Labor ??= []).Add(element);
                }
            }
        }

        // Processing ProcessEntryTool
        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
        {
            // Get ordinals for ProcessEntryTool
            int productIdOrdinal = rdr.GetOrdinal("ProductId");
            int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
            int toolingTypeCodeOrdinal = rdr.GetOrdinal("ToolingTypeCode");
            int lineNoOrdinal = rdr.GetOrdinal("LineNo");
            int machineCodeOrdinal = rdr.GetOrdinal("MachineCode");
            int quantityOrdinal = rdr.GetOrdinal("Quantity");
            int usageCodeOrdinal = rdr.GetOrdinal("UsageCode");
            int scheduleOrdinal = rdr.GetOrdinal("Schedule");
            int costOrdinal = rdr.GetOrdinal("Cost");
            int lineUIDOrdinal = rdr.GetOrdinal("LineUID");
            int sourceOrdinal = rdr.GetOrdinal("Source");
            int commentsOrdinal = rdr.GetOrdinal("Comments");
            int backflushOrdinal = rdr.GetOrdinal("IsBackflush");

            while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
            {
                ProcessEntry entry = returnValue.Find(x => x.Id == rdr[productIdOrdinal].ToStr());

                if (entry is not null)
                {
                    ProcessEntryTool element = new()
                    {
                        ProcessId = rdr[operationNoOrdinal].ToStr(),
                        ToolId = rdr[toolingTypeCodeOrdinal].ToStr(),
                        Id = rdr[toolingTypeCodeOrdinal].ToStr(),
                        LineId = rdr[lineNoOrdinal].ToInt32().ToStr(),
                        MachineId = rdr[machineCodeOrdinal].ToStr(),
                        Quantity = rdr[quantityOrdinal].ToDouble(),
                        Usage = rdr[usageCodeOrdinal].ToStr(),
                        Schedule = rdr[scheduleOrdinal].ToBool(),
                        Cost = rdr[costOrdinal].ToDouble(),
                        LineUID = rdr[lineUIDOrdinal].ToStr(),
                        Source = rdr[sourceOrdinal].ToStr(),
                        Comments = rdr[commentsOrdinal].ToStr(),
                        IsBackflush = rdr[backflushOrdinal].ToBool()
                    };

                    (entry.Tools ??= []).Add(element);
                }
            }
        }

        // Processing ProcessEntryAttribute
        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
        {
            // Get ordinals for ProcessEntryAttribute
            int productIdOrdinal = rdr.GetOrdinal("ProductId");
            int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
            int attributeCodeOrdinal = rdr.GetOrdinal("AttributeCode");
            int isSelectedOrdinal = rdr.GetOrdinal("IsSelected");
            int valueOrdinal = rdr.GetOrdinal("Value");

            while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
            {
                ProcessEntry entry = returnValue.Find(x => x.Id == rdr[productIdOrdinal].ToStr());
                if (entry is not null)
                {
                    ProcessEntryAttribute element = new()
                    {
                        ProcessId = rdr[operationNoOrdinal].ToStr(),
                        AttributeId = rdr[attributeCodeOrdinal].ToStr(),
                        Id = rdr[attributeCodeOrdinal].ToStr(),
                        Selected = rdr[isSelectedOrdinal].ToBool(),
                        Value = rdr[valueOrdinal].ToStr()
                    };

                    ProcessEntryProcess process = entry.Processes?.Find(x => x.ProcessId == element.ProcessId);
                    if (process is not null)
                    {
                        (process.Attributes ??= []).Add(element);
                    }
                }
            }
        }
    }
    /// <summary>
    ///
    /// </summary>
    public bool MergeProcessEntryTools(string processEntryId, string JSON, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ProductTooling_MRG", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_ProductId", processEntryId, !string.IsNullOrEmpty(processEntryId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_JSON", JSON, !string.IsNullOrEmpty(JSON));

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
    // <summary>
    ///
    /// </summary>
    public bool MergeProcessEntryLabor(string processEntryId, string JSON, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ProductLabor_MRG", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_ProductId", processEntryId, !string.IsNullOrEmpty(processEntryId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_JSON", JSON, !string.IsNullOrEmpty(JSON));

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
    /// <summary>
	/// Saves the product details in the database.
	/// </summary>
	public bool SaveProductDetails(ProcessEntry entryInfo, string OperationsJSON, string MaterialJSON, string AlternativesJSON, string SubproductsJSON, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Product_Detail_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_ProductId", () => entryInfo.Id, !string.IsNullOrEmpty(entryInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_OperationJSON", OperationsJSON, !string.IsNullOrEmpty(OperationsJSON), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Processes"));
                command.Parameters.AddCondition("_MaterialJSON", MaterialJSON, !string.IsNullOrEmpty(MaterialJSON), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Components"));
                command.Parameters.AddCondition("_AlternativeMaterialJSON", AlternativesJSON, !string.IsNullOrEmpty(AlternativesJSON));
                command.Parameters.AddCondition("_SubproductsJSON", SubproductsJSON, !string.IsNullOrEmpty(SubproductsJSON));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

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
    /// <summary>
    /// Creates a new Product in the database.
    /// </summary>
    public ProcessEntry CreateProcessEntry(ProcessEntry entryInfo, User systemOperator, IntegrationSource intSrc = IntegrationSource.SF)
    {
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Product_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_Id", entryInfo.Id, !string.IsNullOrEmpty(entryInfo.Id));
                command.Parameters.AddCondition("_Code", entryInfo.Code, !string.IsNullOrEmpty(entryInfo.Code));
                command.Parameters.AddCondition("_Name", entryInfo.Name, !string.IsNullOrEmpty(entryInfo.Name));
                command.Parameters.AddWithValue("_Quantity", entryInfo.Quantity);
                command.Parameters.AddWithValue("_MinQuantity", entryInfo.MinQuantity);
                command.Parameters.AddWithValue("_MaxQuantity", entryInfo.MaxQuantity);
                command.Parameters.AddWithValue("_UnitId", entryInfo.UnitId);
                command.Parameters.AddWithValue("_Factor", entryInfo.Factor);
                command.Parameters.AddWithValue("_Time", entryInfo.Time);
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddWithValue("_Status", entryInfo.Status);
                command.Parameters.AddWithValue("_ValidFrom", entryInfo.ValidFrom);
                command.Parameters.AddWithValue("_ValidTo", entryInfo.ValidTo);
                command.Parameters.AddWithValue("_ProductType", entryInfo.ProductType.ToInt32());
                command.Parameters.AddWithValue("_InstrucctionByPh", entryInfo.InstrucctionByPh);
                command.Parameters.AddWithValue("_InstrucctionbyDensity", entryInfo.InstrucctionbyDensity);
                command.Parameters.AddWithValue("_WarehouseCode", entryInfo.Warehouse);
                command.Parameters.AddWithValue("_Version", entryInfo.Version > 0 ? entryInfo.Version : 1);
                command.Parameters.AddWithValue("_EarlierVersion", entryInfo.EarlierVersion > 0 ? entryInfo.EarlierVersion : 1);
                command.Parameters.AddWithValue("_Sequence", entryInfo.Sequence > 0 ? entryInfo.Sequence : 1);
                command.Parameters.AddWithValue("_Scrap", entryInfo.Scrap);
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
                command.Parameters.AddWithValue("_Origin", intSrc.ToInt32());
                command.Parameters.AddWithValue("_EnableSchedule", entryInfo.Schedule);
                command.Parameters.AddWithValue("_Comments", entryInfo.Comments);
                command.Parameters.AddWithValue("_Formula", entryInfo.Formula);
                command.Parameters.AddWithValue("_BomVersion", entryInfo.BomVersion);
                command.Parameters.AddWithValue("_BomSequence", entryInfo.BomSequence);
                command.Parameters.AddWithValue("_RouteVersion", entryInfo.RouteVersion);
                command.Parameters.AddWithValue("_RouteSequence", entryInfo.RouteSequence);
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    entryInfo.Id = rdr["ProductId"].ToStr();
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
        return entryInfo;
    }
    /// <summary>
	///
	/// </summary>
	public int VerifyProductVersion(ProcessEntry entryInfo)
    {
        int returnValue = 0;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Product_VerifyVersion", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddWithValue("_Code", entryInfo.Code);
                command.Parameters.AddWithValue("_WarehouseCode", entryInfo.Warehouse);
                command.Parameters.AddWithValue("_Version", entryInfo.Version);
                command.Parameters.AddWithValue("_Sequence", entryInfo.Sequence);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                returnValue = command.ExecuteScalarAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToInt32();
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
	/// Updates an existing Product in the database.
	/// </summary>
	public bool UpdateProcessEntry(ProcessEntry entryInfo, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Product_UPD", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_ProductId", entryInfo.Id, !string.IsNullOrEmpty(entryInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_Code", entryInfo.Code, !string.IsNullOrEmpty(entryInfo.Code));
                command.Parameters.AddCondition("_Name", entryInfo.Name, !string.IsNullOrEmpty(entryInfo.Name));
                command.Parameters.AddWithValue("_Quantity", entryInfo.Quantity);
                command.Parameters.AddWithValue("_MinQuantity", entryInfo.MinQuantity);
                command.Parameters.AddWithValue("_MaxQuantity", entryInfo.MaxQuantity);
                command.Parameters.AddWithValue("_UnitId", entryInfo.UnitId);
                command.Parameters.AddWithValue("_Factor", entryInfo.Factor);
                command.Parameters.AddWithValue("_Time", entryInfo.Time);
                command.Parameters.AddCondition("_ValidFrom", entryInfo.ValidFrom, entryInfo.ValidFrom.Year >= 1900);
                command.Parameters.AddCondition("_ValidTo", entryInfo.ValidTo, entryInfo.ValidTo.Year >= 1900);
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
                command.Parameters.AddWithValue("_Status", entryInfo.Status);
                command.Parameters.AddWithValue("_ProductType", entryInfo.ProductType.ToInt32());
                command.Parameters.AddCondition("_InstrucctionByPh", entryInfo.InstrucctionByPh, !string.IsNullOrEmpty(entryInfo.InstrucctionByPh));
                command.Parameters.AddCondition("_InstrucctionbyDensity", entryInfo.InstrucctionbyDensity, !string.IsNullOrEmpty(entryInfo.InstrucctionbyDensity));
                command.Parameters.AddCondition("_WarehouseCode", entryInfo.Warehouse, !string.IsNullOrEmpty(entryInfo.Warehouse));
                command.Parameters.AddWithValue("_Scrap", entryInfo.Scrap);
                command.Parameters.AddWithValue("_EnableSchedule", entryInfo.Schedule);
                command.Parameters.AddWithValue("_Comments", entryInfo.Comments);
                command.Parameters.AddWithValue("_Formula", entryInfo.Formula);
                command.Parameters.AddWithValue("_BomVersion", entryInfo.BomVersion);
                command.Parameters.AddWithValue("_BomSequence", entryInfo.BomSequence);
                command.Parameters.AddWithValue("_RouteVersion", entryInfo.RouteVersion);
                command.Parameters.AddWithValue("_RouteSequence", entryInfo.RouteSequence);
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
    /// <summary>
	///
	/// </summary>
	public int GetNextProductVersion(ProcessEntry entryInfo)
	{
		int returnValue = 0;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Product_Next_Version", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Code", entryInfo.Code);
				command.Parameters.AddWithValue("_WarehouseCode", entryInfo.Warehouse);
				command.Parameters.AddWithValue("_Version", entryInfo.Version);
				command.Parameters.AddWithValue("_Sequence", entryInfo.Sequence);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = command.ExecuteScalarAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToInt32();
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