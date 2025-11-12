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
using NLog;

namespace EWP.SF.KafkaSync.DataAccess;

public class WorkOrderRepo : IWorkOrderRepo
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public WorkOrderRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    /// <summary>
	///
	/// </summary>
	public ResponseData MergeClockInOutBulk(string Json, User systemOperator, bool Validation)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_CheckInOut_BLK", connection)
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
    private static async Task ProcessToolValuesAsync(MySqlDataReader rdr, List<WorkOrder> returnValue, CancellationToken cancel)
    {
        // Get ordinals for ToolValues
        int orderCodeOrdinal = rdr.GetOrdinal("OrderCode");
        int toolIdOrdinal = rdr.GetOrdinal("ToolId");
        int valueOrdinal = rdr.GetOrdinal("Value");
        int codeOrdinal = rdr.GetOrdinal("Code");

        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
        {
            string id = rdr[orderCodeOrdinal].ToStr();
            WorkOrder element = returnValue.Find(x => x.Id == id);
            if (element is not null)
            {
                ToolValue toolValue = new()
                {
                    ToolId = rdr[toolIdOrdinal].ToStr(),
                    Value = rdr[valueOrdinal].ToStr(),
                    Code = rdr[codeOrdinal].ToStr()
                };

                element.ToolValues.Add(toolValue);
            }
        }
    }
    /// <summary>
    ///
    /// </summary>
    public async Task<List<WorkOrder>> GetWorkOrder(string workOrderId, CancellationToken cancel = default)
    {
        List<WorkOrder> returnValue = [];

        await using EWP_Connection connection = new(ConnectionString);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancel).ConfigureAwait(false);

            await using EWP_Command command = new("SP_SF_Order_SEL", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await using (command.ConfigureAwait(false))
            {
                command.Parameters.Clear();
                command.Parameters.AddCondition("_Order", workOrderId, !string.IsNullOrEmpty(workOrderId));
                command.Parameters.AddNull("_Status");
                command.Parameters.AddNull("_StartDate");
                command.Parameters.AddNull("_EndDate");
                command.Parameters.AddWithValue("_ListOnly", 0);
                command.Parameters.AddNull("_DeltaDate");

                try
                {
                    MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);
                    await using (rdr.ConfigureAwait(false))
                    {
                        // Get ordinals for efficient access
                        int orderCodeOrdinal = rdr.GetOrdinal("OrderCode");
                        int productNameOrdinal = rdr.GetOrdinal("ProductName");
                        int productIdOrdinal = rdr.GetOrdinal("ProductId");
                        int plannedQtyOrdinal = rdr.GetOrdinal("PlannedQty");
                        int plannedStartDateUTCOrdinal = rdr.GetOrdinal("PlannedStartDateUTC");
                        int plannedStartDateOrdinal = rdr.GetOrdinal("PlannedStartDate");
                        int plannedEndDateOrdinal = rdr.GetOrdinal("PlannedEndDate");
                        int dueDateOrdinal = rdr.GetOrdinal("DueDate");
                        int createDateOrdinal = rdr.GetOrdinal("CreateDate");
                        int createUserOrdinal = rdr.GetOrdinal("CreateUser");
                        int statusOrdinal = rdr.GetOrdinal("Status");
                        int acceptedQtyOrdinal = rdr.GetOrdinal("AcceptedQty");
                        int rejectedQtyOrdinal = rdr.GetOrdinal("RejectedQty");
                        int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
                        int productionLinesOrdinal = rdr.GetOrdinal("ProductionLines");
                        int hasAllocationOrdinal = rdr.GetOrdinal("HasAllocation");
                        int apsOrdinal = rdr.GetOrdinal("APS");
                        int orderGroupOrdinal = rdr.GetOrdinal("OrderGroup");
                        int orderTypeOrdinal = rdr.GetOrdinal("OrderType");
                        int formulaOrdinal = rdr.GetOrdinal("Formula");
                        int commentsOrdinal = rdr.GetOrdinal("Comments");
                        int salesOrderOrdinal = rdr.GetOrdinal("SalesOrder");
                        int unitOrdinal = rdr.GetOrdinal("Unit");
                        int schedulingReadyOrdinal = rdr.GetOrdinal("SchedulingReady");
                        int priorityOrdinal = rdr.GetOrdinal("Priority");
                        int orderSourceOrdinal = rdr.GetOrdinal("OrderSource");
                        int lotNoOrdinal = rdr.GetOrdinal("LotNo");
                        int logDetailIdOrdinal = rdr.GetOrdinal("LogDetailId");
                        int pathOrdinal = rdr.GetOrdinal("path");
                        int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                        int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");
                        int realStartDateOrdinal = rdr.GetOrdinal("RealStartDate");
                        int realStartDateUTCOrdinal = rdr.GetOrdinal("RealStartDateUTC");
                        int realEndDateOrdinal = rdr.GetOrdinal("RealEndDate");
                        int productCodeOrdinal = rdr.GetOrdinal("ProductCode");
                        int versionOrdinal = rdr.GetOrdinal("Version");
                        // Reading main WorkOrder data
                        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
                        {
                            WorkOrder element = new()
                            {
                                Id = rdr[orderCodeOrdinal].ToStr(),
                                ExternalId = rdr[orderCodeOrdinal].ToStr(),
                                OrderCode = rdr[orderCodeOrdinal].ToStr(),
                                ProductName = rdr[productNameOrdinal].ToStr(),
                                ProductCode = rdr[productCodeOrdinal].ToStr(),
                                ProcessEntryId = rdr[productIdOrdinal].ToStr(),
                                PlannedQty = rdr[plannedQtyOrdinal].ToDouble(),
                                PlannedStartUTC = rdr[plannedStartDateUTCOrdinal].ToDate(),
                                PlannedStart = rdr[plannedStartDateOrdinal].ToDate(),
                                PlannedEnd = rdr[plannedEndDateOrdinal].ToDate(),
                                DueDate = rdr[dueDateOrdinal].ToDate(),
                                CreationDate = rdr[createDateOrdinal].ToDate(),
                                CreatedBy = new User(rdr[createUserOrdinal].ToInt32()),
                                Status = (Status)rdr[statusOrdinal].ToInt32(),
                                ReceivedQty = rdr[acceptedQtyOrdinal].ToDouble() + rdr[rejectedQtyOrdinal].ToDouble(),
                                AcceptedQty = rdr[acceptedQtyOrdinal].ToDouble(),
                                RejectedQty = rdr[rejectedQtyOrdinal].ToDouble(),
                                WarehouseId = rdr[warehouseCodeOrdinal].ToStr(),
                                Version = rdr[versionOrdinal].ToInt32(),
                                ProductionLines = [.. rdr[productionLinesOrdinal].ToStr().Split(',')],
                                IsAllocated = rdr[hasAllocationOrdinal].ToBool(),
                                APS = rdr[apsOrdinal].ToBool(),
                                OrderGroup = rdr[orderGroupOrdinal].ToStr(),
                                OrderType = rdr[orderTypeOrdinal].ToStr(),
                                Formula = rdr[formulaOrdinal].ToStr(),
                                Comments = rdr[commentsOrdinal].ToStr(),
                                SalesOrder = rdr[salesOrderOrdinal].ToStr(),
                                UnitId = rdr[unitOrdinal].ToStr(),
                                SchedulingReady = rdr[schedulingReadyOrdinal].ToBool(),
                                Priority = rdr[priorityOrdinal].ToStr(),
                                OrderSource = rdr[orderSourceOrdinal].ToStr(),
                                LotNo = rdr[lotNoOrdinal].ToStr(),
                                LogDetailId = rdr[logDetailIdOrdinal].ToStr(),
                                JsPath = rdr[pathOrdinal].ToStr(),
                                Processes = [],
                                Components = [],
                                ToolValues = []
                            };

                            if (rdr[updateDateOrdinal].ToDate().Year > 1900)
                            {
                                element.ModifyDate = rdr[updateDateOrdinal].ToDate();
                                element.ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32());
                            }
                            if (rdr[realStartDateOrdinal].ToDate().Year > 1900)
                            {
                                element.RealStart = rdr[realStartDateOrdinal].ToDate();
                                element.RealStartUTC = rdr[realStartDateUTCOrdinal].ToDate();
                            }
                            if (rdr[realEndDateOrdinal].ToDate().Year > 1900)
                            {
                                element.RealEnd = rdr[realEndDateOrdinal].ToDate();
                            }

                            returnValue.Add(element);
                        }

                        // Processing the next result set for WorkOrder processes
                        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
                        {
                            await ProcessWorkOrderProcessesAsync(rdr, returnValue, cancel).ConfigureAwait(false);
                        }

                        // Processing the next result set for WorkOrder components
                        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
                        {
                            await ProcessWorkOrderComponentsAsync(rdr, returnValue, cancel).ConfigureAwait(false);
                        }

                        // Processing the next result set for ToolValues
                        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
                        {
                            await ProcessToolValuesAsync(rdr, returnValue, cancel).ConfigureAwait(false);
                        }

                        // Processing the next result set for tasks
                        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
                        {
                            await ProcessWorkOrderTasksAsync(rdr, returnValue, cancel).ConfigureAwait(false);
                        }

                        // Processing the next result set for SubProducts
                        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
                        {
                            await ProcessSubProductsAsync(rdr, returnValue, cancel).ConfigureAwait(false);
                        }

                        // Processing the next result set for Tools
                        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
                        {
                            await ProcessToolsAsync(rdr, returnValue, cancel).ConfigureAwait(false);
                        }

                        // Processing the next result set for Labor
                        if (await rdr.NextResultAsync(cancel).ConfigureAwait(false))
                        {
                            await ProcessLaborAsync(rdr, returnValue, cancel).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
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
    private static async Task ProcessWorkOrderTasksAsync(MySqlDataReader rdr, List<WorkOrder> returnValue, CancellationToken cancel)
    {
        // Get ordinals for WorkOrder tasks
        int orderCodeOrdinal = rdr.GetOrdinal("OrderCode");
        int activityIdOrdinal = rdr.GetOrdinal("ActivityId");
        int nameOrdinal = rdr.GetOrdinal("Name");
        int triggerIdOrdinal = rdr.GetOrdinal("TriggerId");
        int sortIdOrdinal = rdr.GetOrdinal("SortId");
        int isMandatoryOrdinal = rdr.GetOrdinal("IsMandatory");
        int requiresInstructionsOrdinal = rdr.GetOrdinal("RequiresInstructions");
        int durationOrdinal = rdr.GetOrdinal("Duration");
        int durationUnitOrdinal = rdr.GetOrdinal("DurationUnit");
        int machineCodeOrdinal = rdr.GetOrdinal("MachineCode");
        int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
        {
            string id = rdr[orderCodeOrdinal].ToStr();
            WorkOrder element = returnValue.Find(x => x.Id == id);
            if (element is not null)
            {
                Activity task = new()
                {
                    Id = rdr[activityIdOrdinal].ToStr(),
                    OperationNo = rdr[operationNoOrdinal].ToStr(),
                    AssetId = rdr[machineCodeOrdinal].ToStr(),
                    Name = rdr[nameOrdinal].ToStr(),
                    TriggerId = rdr[triggerIdOrdinal].ToInt32(),
                    SortId = rdr[sortIdOrdinal].ToInt32(1),
                    IsMandatory = rdr[isMandatoryOrdinal].ToBool(false),
                    RequiresInstructions = rdr[requiresInstructionsOrdinal].ToBool(),
                    Schedule = new ActivitySchedule
                    {
                        Duration = rdr[durationOrdinal].ToDouble(),
                        DurationUnit = rdr[durationUnitOrdinal].ToInt32()
                    },
                };

                (element.Tasks ??= []).Add(task);
            }
        }
    }

    private static async Task ProcessSubProductsAsync(MySqlDataReader rdr, List<WorkOrder> returnValue, CancellationToken cancel)
    {
        // Get ordinals for SubProducts
        int orderCodeOrdinal = rdr.GetOrdinal("OrderCode");
        int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
        int itemCodeOrdinal = rdr.GetOrdinal("ItemCode");
        int factorOrdinal = rdr.GetOrdinal("Factor");
        int acceptedQtyOrdinal = rdr.GetOrdinal("AcceptedQty");
        int rejectedQtyOrdinal = rdr.GetOrdinal("RejectedQty");
        int nameOrdinal = rdr.GetOrdinal("Name");
        int codeOrdinal = rdr.GetOrdinal("Code");
        int unitTypeOrdinal = rdr.GetOrdinal("UnitType");
        int unitOrdinal = rdr.GetOrdinal("Unit");
        int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
        int itemImageOrdinal = rdr.GetOrdinal("ItemImage");
        int lineUIDOrdinal = rdr.GetOrdinal("LineUID");
        int lineIdOrdinal = rdr.GetOrdinal("LineNo");
        int commentsOrdinal = rdr.GetOrdinal("Comments");

        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
        {
            string id = rdr[orderCodeOrdinal].ToStr();
            WorkOrder element = returnValue.Find(x => x.Id == id);
            if (element is not null)
            {
                SubProduct subproduct = new()
                {
                    OperationNo = rdr[operationNoOrdinal].ToStr(),
                    ComponentId = rdr[itemCodeOrdinal].ToStr(),
                    Factor = rdr[factorOrdinal].ToDouble(),
                    Quantity = rdr[acceptedQtyOrdinal].ToDouble(),
                    Rejected = rdr[rejectedQtyOrdinal].ToDouble(),
                    Name = rdr[nameOrdinal].ToStr(),
                    Code = rdr[codeOrdinal].ToStr(),
                    UnitType = rdr[unitTypeOrdinal].ToInt32(),
                    UnitId = rdr[unitOrdinal].ToStr(),
                    WarehouseCode = rdr[warehouseCodeOrdinal].ToStr(),
                    ItemImage = rdr[itemImageOrdinal].ToStr(),
                    LineUID = rdr[lineUIDOrdinal].ToStr(),
                    LineId = rdr[lineIdOrdinal].ToStr(),
                    Comments = rdr[commentsOrdinal].ToStr(),
                };

                (element.Subproducts ??= []).Add(subproduct);
            }
        }
    }

    private static async Task ProcessLaborAsync(MySqlDataReader rdr, List<WorkOrder> returnValue, CancellationToken cancel)
    {
        // Get ordinals for Labor
        int orderCodeOrdinal = rdr.GetOrdinal("OrderCode");
        int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
        int positionCodeOrdinal = rdr.GetOrdinal("PositionCode");
        int lineNoOrdinal = rdr.GetOrdinal("LineNo");
        int machineCodeOrdinal = rdr.GetOrdinal("MachineCode");
        int quantityOrdinal = rdr.GetOrdinal("Quantity");
        int sourceOrdinal = rdr.GetOrdinal("Source");
        int commentsOrdinal = rdr.GetOrdinal("Comments");
        int lineUIDOrdinal = rdr.GetOrdinal("LineUID");
        int plannedqtyOrdinal = rdr.GetOrdinal("PlannedQty");
        int usageOrdinal = rdr.GetOrdinal("UsageCode");
        int isBackflushOrdinal = rdr.GetOrdinal("IsBackflush");
        int issuedTimeOrdinal = rdr.GetOrdinal("IssuedTime");

        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
        {
            string id = rdr[orderCodeOrdinal].ToStr();
            WorkOrder element = returnValue.Find(x => x.Id == id);
            if (element is not null)
            {
                WorkOrderLabor labor = new()
                {
                    OperationNo = rdr[operationNoOrdinal].ToStr(),
                    LaborId = rdr[positionCodeOrdinal].ToStr(),
                    LineId = rdr[lineNoOrdinal].ToStr(),
                    MachineId = rdr[machineCodeOrdinal].ToStr(),
                    Quantity = rdr[quantityOrdinal].ToDouble(),
                    Source = rdr[sourceOrdinal].ToStr(),
                    Comments = rdr[commentsOrdinal].ToStr(),
                    Usage = rdr[usageOrdinal].ToStr(),
                    PlannedQty = rdr[plannedqtyOrdinal].ToDouble(),
                    LineUID = rdr[lineUIDOrdinal].ToStr(),
                    IsBackflush = rdr[isBackflushOrdinal].ToBool(),
                    IssuedTime = rdr[issuedTimeOrdinal].ToDouble()
                };

                (element.Labor ??= []).Add(labor);
            }
        }
    }

    private static async Task ProcessToolsAsync(MySqlDataReader rdr, List<WorkOrder> returnValue, CancellationToken cancel)
    {
        // Get ordinals for Tools
        int orderCodeOrdinal = rdr.GetOrdinal("OrderCode");
        int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
        int toolIdOrdinal = rdr.GetOrdinal("ToolId");
        int plannedQtyOrdinal = rdr.GetOrdinal("PlannedQty");
        int quantityOrdinal = rdr.GetOrdinal("Quantity");
        int lineUIDOrdinal = rdr.GetOrdinal("LineUID");
        int sourceOrdinal = rdr.GetOrdinal("Source");
        int commentsOrdinal = rdr.GetOrdinal("Comments");
        int lineNoOrdinal = rdr.GetOrdinal("LineNo");
        int machineCodeOrdinal = rdr.GetOrdinal("MachineCode");
        int usageOrdinal = rdr.GetOrdinal("UsageCode");
        int isBackflushOrdinal = rdr.GetOrdinal("IsBackflush");
        int issuedTimeOrdinal = rdr.GetOrdinal("IssuedTime");

        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
        {
            string id = rdr[orderCodeOrdinal].ToStr();
            WorkOrder element = returnValue.Find(x => x.Id == id);
            if (element is not null)
            {
                WorkOrderTool tool = new()
                {
                    OperationNo = rdr[operationNoOrdinal].ToStr(),
                    ToolId = rdr[toolIdOrdinal].ToStr(),
                    PlannedQty = rdr[plannedQtyOrdinal].ToDouble(),
                    Quantity = rdr[quantityOrdinal].ToDouble(),
                    LineUID = rdr[lineUIDOrdinal].ToStr(),
                    Source = rdr[sourceOrdinal].ToStr(),
                    Comments = rdr[commentsOrdinal].ToStr(),
                    Usage = rdr[usageOrdinal].ToStr(),
                    LineId = rdr[lineNoOrdinal].ToStr(),
                    MachineId = rdr[machineCodeOrdinal].ToStr(),
                    IsBackflush = rdr[isBackflushOrdinal].ToBool(),
                    IssuedTime = rdr[issuedTimeOrdinal].ToDouble()
                };

                (element.Tools ??= []).Add(tool);
            }
        }
    }
    private static async Task ProcessWorkOrderProcessesAsync(MySqlDataReader rdr, List<WorkOrder> returnValue, CancellationToken cancel)
    {
        // Get ordinals for WorkOrder Processes
        int orderCodeOrdinal = rdr.GetOrdinal("OrderCode");
        int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
        int operationTypeCodeOrdinal = rdr.GetOrdinal("OperationTypeCode");
        int operationSubtypeCodeOrdinal = rdr.GetOrdinal("OperationSubtypeCode");
        int stepOrdinal = rdr.GetOrdinal("Step");
        int parentCodeOrdinal = rdr.GetOrdinal("ParentCode");
        int machineCodeOrdinal = rdr.GetOrdinal("MachineCode");
        int nameOrdinal = rdr.GetOrdinal("Name");
        int isOutputOrdinal = rdr.GetOrdinal("IsOutput");
        int outputItemCodeOrdinal = rdr.GetOrdinal("OutputItemCode");
        int statusOrdinal = rdr.GetOrdinal("Status");
        int totalOrdinal = rdr.GetOrdinal("Total");
        int acceptedOrdinal = rdr.GetOrdinal("Accepted");
        int rejectedOrdinal = rdr.GetOrdinal("Rejected");
        int machineAcceptedOrdinal = rdr.GetOrdinal("MachineAccepted");
        int machineRejectedOrdinal = rdr.GetOrdinal("MachineRejected");
        int commentsOrdinal = rdr.GetOrdinal("Comments");
        int originalMachineCodeOrdinal = rdr.GetOrdinal("OriginalMachineCode");
        int machineStatuOrdinal = rdr.GetOrdinal("MachineStatus");
        int lineNoOrdinal = rdr.GetOrdinal("LineNo");
        int lineUIDOrdinal = rdr.GetOrdinal("LineUID");
        int plannedSetupStartOrdinal = rdr.GetOrdinal("PlannedSetupStartDate");
        int plannedSetupEndOrdinal = rdr.GetOrdinal("PlannedSetupEndDate");
        int plannedStartDateOrdinal = rdr.GetOrdinal("PlannedStartDate");
        int plannedEndDateOrdinal = rdr.GetOrdinal("PlannedEndDate");
        int realStartDateOrdinal = rdr.GetOrdinal("RealStartDate");
        int realEndDateOrdinal = rdr.GetOrdinal("RealEndDate");
        int realStartDateUTCOrdinal = rdr.GetOrdinal("RealStartDateUTC");
        int realEndDateUTCOrdinal = rdr.GetOrdinal("RealEndDateUTC");
        int setupTimeOrdinal = rdr.GetOrdinal("SetupTime");
        int execTimeOrdinal = rdr.GetOrdinal("ExecTime");
        int waitTimeOrdinal = rdr.GetOrdinal("WaitTime");
        int isBackflushOrdinal = rdr.GetOrdinal("IsBackflush");
        int issuedTimeOrdinal = rdr.GetOrdinal("IssuedTime");
        int classIdOrdinal = rdr.GetOrdinal("OperationClassId");
        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
        {
            string id = rdr[orderCodeOrdinal].ToStr();
            WorkOrder element = returnValue.Find(x => x.Id == id);
            if (element is not null)
            {
                OrderProcess process = new()
                {
                    ProcessTypeId = rdr[operationTypeCodeOrdinal].ToStr(),
                    ProcessSubTypeId = rdr[operationSubtypeCodeOrdinal].ToStr(),
                    OperationNo = rdr[operationNoOrdinal].ToStr(),
                    Step = rdr[stepOrdinal].ToInt32(),
                    ProductionLineId = rdr[parentCodeOrdinal].ToStr(),
                    MachineId = rdr[machineCodeOrdinal].ToStr(),
                    OperationName = rdr[nameOrdinal].ToStr(),
                    IsOutput = rdr[isOutputOrdinal].ToBool(),
                    Output = rdr[outputItemCodeOrdinal].ToStr(),
                    Status = (Status)rdr[statusOrdinal].ToInt32(),
                    Total = rdr[totalOrdinal].ToDouble(),
                    Received = rdr[acceptedOrdinal].ToDouble(),
                    Rejected = rdr[rejectedOrdinal].ToDouble(),
                    MachineReceived = rdr[machineAcceptedOrdinal].ToDouble(),
                    MachineRejected = rdr[machineRejectedOrdinal].ToDouble(),
                    Comments = rdr[commentsOrdinal].ToStr(),
                    OriginalMachineId = rdr[originalMachineCodeOrdinal].ToStr(),
                    MachineStatus = (Status)rdr[machineStatuOrdinal].ToInt32(),
                    //LineId = rdr[lineNoOrdinal].ToStr(),
                    LineId = rdr[lineNoOrdinal].ToInt32(),
                    LineUID = rdr[lineUIDOrdinal].ToStr(),
                    PlannedSetupStart = rdr[plannedSetupStartOrdinal].ToDate(),
                    PlannedSetupEnd = rdr[plannedSetupEndOrdinal].ToDate(),
                    SetupTime = rdr[setupTimeOrdinal].ToDouble(),
                    ExecTime = rdr[execTimeOrdinal].ToDouble(),
                    WaitTime = rdr[waitTimeOrdinal].ToDouble(),
                    IssuedTime = rdr[issuedTimeOrdinal].ToDouble(),
                    IsBackflush = rdr[isBackflushOrdinal].ToBool(),
                    Class = rdr[classIdOrdinal].ToStr()
                };
                if (process.PlannedSetupEnd.HasValue && process.PlannedSetupEnd.Value.Year <= 1900)
                {
                    process.PlannedSetupEnd = null;
                }

                if (process.PlannedSetupStart.HasValue && process.PlannedSetupStart.Value.Year <= 1900)
                {
                    process.PlannedSetupStart = null;
                }
                if (rdr[plannedStartDateOrdinal].ToDate().Year > 1900)
                {
                    process.PlannedStart = rdr[plannedStartDateOrdinal].ToDate();
                }
                if (rdr[plannedEndDateOrdinal].ToDate().Year > 1900)
                {
                    process.PlannedEnd = rdr[plannedEndDateOrdinal].ToDate();
                }

                if (rdr[realStartDateOrdinal].ToDate().Year > 1900)
                {
                    process.RealStart = rdr[realStartDateOrdinal].ToDate();
                    process.RealStartUTC = rdr[realStartDateUTCOrdinal].ToDate();
                }
                if (rdr[realEndDateOrdinal].ToDate().Year > 1900)
                {
                    process.RealEnd = rdr[realEndDateOrdinal].ToDate();
                    process.RealEndUTC = rdr[realEndDateUTCOrdinal].ToDate();
                }
                element.Processes.Add(process);
            }
        }
    }

    private static async Task ProcessWorkOrderComponentsAsync(MySqlDataReader rdr, List<WorkOrder> returnValue, CancellationToken cancel)
    {
        // Get ordinals for WorkOrder Components
        int orderCodeOrdinal = rdr.GetOrdinal("OrderCode");
        int operationNoOrdinal = rdr.GetOrdinal("OperationNo");
        int itemCodeOrdinal = rdr.GetOrdinal("ItemCode");
        int itemNameOrdinal = rdr.GetOrdinal("ItemName");
        int targetQtyOrdinal = rdr.GetOrdinal("TargetQty");
        int targetUnitOrdinal = rdr.GetOrdinal("TargetUnit");
        int inputQtyOrdinal = rdr.GetOrdinal("InputQty");
        int inputUnitOrdinal = rdr.GetOrdinal("InputUnit");
        int statusOrdinal = rdr.GetOrdinal("Status");
        int warehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
        int commentsOrdinal = rdr.GetOrdinal("Comments");
        int operationTypeCodeOrdinal = rdr.GetOrdinal("OperationTypeCode");
        int StepOrdinal = rdr.GetOrdinal("Step");
        int LineNoOrdinal = rdr.GetOrdinal("LineNo");
        int originalItemOrdinal = rdr.GetOrdinal("OriginalItemCode");
        int isbackflushOrdinal = rdr.GetOrdinal("IsBackFlush");
        int typeOrdinal = rdr.GetOrdinal("Type");
        int materialImageOrdinal = rdr.GetOrdinal("MaterialImage");
        int lineUIDOrdinal = rdr.GetOrdinal("LineUID");
        int sourceOrdinal = rdr.GetOrdinal("Source");
        int managedByOrdinal = rdr.GetOrdinal("ManagedBy");

        while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
        {
            string id = rdr[orderCodeOrdinal].ToStr();
            WorkOrder element = returnValue.Find(x => x.Id == id);
            if (element is not null)
            {
                OrderComponent component = new()
                {
                    OperationNo = rdr[operationNoOrdinal].ToStr(),
                    ComponentType = ComponentType.Material,
                    SourceId = rdr[itemCodeOrdinal].ToStr(),
                    ComponentName = rdr[itemNameOrdinal].ToStr(),
                    ComponentCode = rdr[itemCodeOrdinal].ToStr(),
                    TargetQty = rdr[targetQtyOrdinal].ToDouble(),
                    TargetUnitId = rdr[targetUnitOrdinal].ToStr(),
                    InputQty = rdr[inputQtyOrdinal].ToDouble(),
                    InputUnitId = rdr[inputUnitOrdinal].ToStr(),
                    Status = (Status)rdr[statusOrdinal].ToInt32(),
                    WarehouseCode = rdr[warehouseCodeOrdinal].ToStr(),
                    Comments = rdr[commentsOrdinal].ToStr(),
                    ProcessTypeId = rdr[operationTypeCodeOrdinal].ToStr(),
                    Step = rdr[StepOrdinal].ToInt32(),
                    SourceTypeId = "2",
                    LineId = rdr[LineNoOrdinal].ToStr(),
                    OriginalSourceId = rdr[originalItemOrdinal].ToStr(),
                    IsBackflush = rdr[isbackflushOrdinal].ToBool(),
                    MaterialType = rdr[typeOrdinal].ToInt32(),
                    MaterialImage = rdr[materialImageOrdinal].ToStr(),
                    LineUID = rdr[lineUIDOrdinal].ToStr(),
                    Source = rdr[sourceOrdinal].ToStr(),
                    ManagedBy = rdr[managedByOrdinal].ToStr(),
                };

                element.Components.Add(component);
            }
        }
    }
    /// <summary>
	///
	/// </summary>
	public List<ReturnMaterialContext> GetProductReturnContext(string workOrderId)
    {
        List<ReturnMaterialContext> returnValue = [];
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ProductReturnContext", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddCondition("_OrderCode", workOrderId, !string.IsNullOrEmpty(workOrderId));
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int OrderCodeOrdinal = rdr.GetOrdinal("OrderCode");
                int OperationNoOrdinal = rdr.GetOrdinal("OperationNo");
                int MachineCodeOrdinal = rdr.GetOrdinal("MachineCode");
                int OriginalMachineCodeOrdinal = rdr.GetOrdinal("OriginalMachineCode");
                int ItemCodeOrdinal = rdr.GetOrdinal("ItemCode");
                int StepOrdinal = rdr.GetOrdinal("Step");
                int NameOrdinal = rdr.GetOrdinal("Name");
                int ItemImageOrdinal = rdr.GetOrdinal("ItemImage");
                int UnitOrdinal = rdr.GetOrdinal("Unit");
                int BatchNumberOrdinal = rdr.GetOrdinal("BatchNumber");
                int BinLocationCodeOrdinal = rdr.GetOrdinal("BinLocationCode");
                int PalletOrdinal = rdr.GetOrdinal("Pallet");
                int LineNoOrdinal = rdr.GetOrdinal("LineNo");
                int QuantityOrdinal = rdr.GetOrdinal("Quantity");
                int InventoryStatusCodeOrdinal = rdr.GetOrdinal("InventoryStatusCode");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    ReturnMaterialContext element = new()
                    {
                        WorkOrderId = rdr[OrderCodeOrdinal].ToStr(),
                        ProcessId = rdr[OperationNoOrdinal].ToStr(),
                        MachineId = rdr[MachineCodeOrdinal].ToStr(),
                        OriginalMachineId = rdr[OriginalMachineCodeOrdinal].ToStr(),
                        ComponentId = rdr[ItemCodeOrdinal].ToStr(),
                        Step = rdr[StepOrdinal].ToInt32(),
                        ComponentName = rdr[NameOrdinal].ToStr(),
                        ComponentCode = rdr[ItemCodeOrdinal].ToStr(),
                        ComponentImage = rdr[ItemImageOrdinal].ToStr(),
                        ComponentUnit = rdr[UnitOrdinal].ToStr(),
                        Lot = rdr[BatchNumberOrdinal].ToStr(),
                        Location = rdr[BinLocationCodeOrdinal].ToStr(),
                        Pallet = rdr[PalletOrdinal].ToStr(),
                        LineId = rdr[LineNoOrdinal].ToStr(),
                        Quantity = rdr[QuantityOrdinal].ToDouble(),
                        InventoryStatus = rdr[InventoryStatusCodeOrdinal].ToStr()
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
    /// <summary>
	///
	/// </summary>
	public WorkOrderResponse MergeWorkOrderChangeStatus(WorkOrderChangeStatus workorderInfo, User systemOperator, bool Validation, LevelMessage Level)
    {
        WorkOrderResponse returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Order_ChangeStatus_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddWithValue("_IsValidation", Validation);
                command.Parameters.AddCondition("_Id", workorderInfo.OrderId, !string.IsNullOrEmpty(workorderInfo.OrderId));
                command.Parameters.AddCondition("_Code", workorderInfo.OrderCode, !string.IsNullOrEmpty(workorderInfo.OrderCode));
                command.Parameters.AddWithValue("_Status", workorderInfo.Status);
                command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int ActionOrdinal = rdr.GetOrdinal("Action");
                int IsSuccessOrdinal = rdr.GetOrdinal("IsSuccess");
                int CodeOrdinal = rdr.GetOrdinal("Code");
                int MessageOrdinal = rdr.GetOrdinal("Message");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new WorkOrderResponse
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
    /// <summary>
	///
	/// </summary>
	public WorkOrder GetWorkOrderByCode(string workOrderCode)
    {
        WorkOrder returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Order_Code_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_Order", workOrderCode, !string.IsNullOrEmpty(workOrderCode));
                /*command.Parameters.AddNull("_Status");
                command.Parameters.AddNull("_StartDate");
                command.Parameters.AddNull("_EndDate");
                command.Parameters.AddWithValue("_ListOnly", 0);*/

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int OrderCodeOrdinal = rdr.GetOrdinal("OrderCode");

                int ProductNameOrdinal = rdr.GetOrdinal("ProductName");
                int ProductCodeOrdinal = rdr.GetOrdinal("ProductCode");
                int ProductIdOrdinal = rdr.GetOrdinal("ProductId");
                int PlannedQtyOrdinal = rdr.GetOrdinal("PlannedQty");
                int PlannedStartDateUTCOrdinal = rdr.GetOrdinal("PlannedStartDateUTC");
                int PlannedStartDateOrdinal = rdr.GetOrdinal("PlannedStartDate");
                int PlannedEndDateOrdinal = rdr.GetOrdinal("PlannedEndDate");
                int DueDateOrdinal = rdr.GetOrdinal("DueDate");
                int CreateDateOrdinal = rdr.GetOrdinal("CreateDate");
                int CreateUserOrdinal = rdr.GetOrdinal("CreateUser");
                int StatusOrdinal = rdr.GetOrdinal("Status");
                int AcceptedQtyOrdinal = rdr.GetOrdinal("AcceptedQty");
                int RejectedQtyOrdinal = rdr.GetOrdinal("RejectedQty");
                int ProductionLinesOrdinal = rdr.GetOrdinal("ProductionLines");
                int HasAllocationOrdinal = rdr.GetOrdinal("HasAllocation");
                int APSOrdinal = rdr.GetOrdinal("APS");
                int WarehouseCodeOrdinal = rdr.GetOrdinal("WarehouseCode");
                int OrderGroupOrdinal = rdr.GetOrdinal("OrderGroup");
                int OrderTypeOrdinal = rdr.GetOrdinal("OrderType");
                int FormulaOrdinal = rdr.GetOrdinal("Formula");
                int CommentsOrdinal = rdr.GetOrdinal("Comments");
                int SalesOrderOrdinal = rdr.GetOrdinal("SalesOrder");
                int UnitOrdinal = rdr.GetOrdinal("Unit");
                int UpdateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                int UpdateUserOrdinal = rdr.GetOrdinal("UpdateUser");
                int RealStartDateOrdinal = rdr.GetOrdinal("RealStartDate");
                int RealStartDateUTCOrdinal = rdr.GetOrdinal("RealStartDateUTC");
                int RealEndDateOrdinal = rdr.GetOrdinal("RealEndDate");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new WorkOrder
                    {
                        Id = rdr[OrderCodeOrdinal].ToStr(),
                        ExternalId = rdr[OrderCodeOrdinal].ToStr(),
                        OrderCode = rdr[OrderCodeOrdinal].ToStr(),
                        ProductName = rdr[ProductNameOrdinal].ToStr(),
                        ProductCode = rdr[ProductCodeOrdinal].ToStr(),
                        ProcessEntryId = rdr[ProductIdOrdinal].ToStr(),
                        PlannedQty = rdr[PlannedQtyOrdinal].ToDouble(),
                        PlannedStartUTC = rdr[PlannedStartDateUTCOrdinal].ToDate(),
                        PlannedStart = rdr[PlannedStartDateOrdinal].ToDate(),
                        PlannedEnd = rdr[PlannedEndDateOrdinal].ToDate(),
                        DueDate = rdr[DueDateOrdinal].ToDate(),
                        CreationDate = rdr[CreateDateOrdinal].ToDate(),
                        CreatedBy = new User(rdr[CreateUserOrdinal].ToInt32()),
                        Status = (Status)rdr[StatusOrdinal].ToInt32(),
                        ReceivedQty = rdr[AcceptedQtyOrdinal].ToDouble() + rdr[RejectedQtyOrdinal].ToDouble(),
                        AcceptedQty = rdr[AcceptedQtyOrdinal].ToDouble(),
                        RejectedQty = rdr[RejectedQtyOrdinal].ToDouble(),
                        ProductionLines = [.. rdr[ProductionLinesOrdinal].ToStr().Split(',')],
                        IsAllocated = rdr[HasAllocationOrdinal].ToBool(),
                        APS = rdr[APSOrdinal].ToBool(),
                        WarehouseId = rdr[WarehouseCodeOrdinal].ToStr(),
                        OrderGroup = rdr[OrderGroupOrdinal].ToStr(),
                        OrderType = rdr[OrderTypeOrdinal].ToStr(),
                        Formula = rdr[FormulaOrdinal].ToStr(),
                        Comments = rdr[CommentsOrdinal].ToStr(),
                        SalesOrder = rdr[SalesOrderOrdinal].ToStr(),
                        UnitId = rdr[UnitOrdinal].ToStr(),
                        Processes = [],
                        Components = [],
                        ToolValues = []
                    };

                    if (rdr[UpdateDateOrdinal].ToDate().Year > 1900)
                    {
                        returnValue.ModifyDate = rdr[UpdateDateOrdinal].ToDate();
                        returnValue.ModifiedBy = new User(rdr[UpdateUserOrdinal].ToInt32());
                    }
                    if (rdr[RealStartDateOrdinal].ToDate().Year > 1900)
                    {
                        returnValue.RealStartUTC = rdr[RealStartDateUTCOrdinal].ToDate();
                        returnValue.RealStart = rdr[RealStartDateOrdinal].ToDate();
                    }
                    if (rdr[RealEndDateOrdinal].ToDate().Year > 1900)
                    {
                        returnValue.RealEnd = rdr[RealEndDateOrdinal].ToDate();
                    }

                    returnValue ??= new WorkOrder();
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
	public bool MergeWorkOrderProcesses(WorkOrder workorderInfo, string processXML, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Order_Operation_MRG", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_OrderCode", workorderInfo.Id, !string.IsNullOrEmpty(workorderInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_JSON", processXML, !string.IsNullOrEmpty(processXML));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

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

    /// <summary>
    ///
    /// </summary>
    public bool MergeWorkOrderComponents(WorkOrder workorderInfo, string componentJson, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Order_Op_Items_MRG", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120000
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_OrderCode", workorderInfo.Id, !string.IsNullOrEmpty(workorderInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_JSON", componentJson, !string.IsNullOrEmpty(componentJson));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

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

    /// <summary>
    ///
    /// </summary>
    public bool MergeWorkOrderTooling(WorkOrder workorderInfo, string toolingJson, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_OrderTooling_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_OrderCode", workorderInfo.Id, !string.IsNullOrEmpty(workorderInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_JSON", toolingJson, !string.IsNullOrEmpty(toolingJson));

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

    /// <summary>
    ///
    /// </summary>
    public bool MergeWorkOrderByProducts(WorkOrder workorderInfo, string subproductXML, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Order_ByProduct_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_OrderCode", workorderInfo.Id, !string.IsNullOrEmpty(workorderInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_JSON", subproductXML, !string.IsNullOrEmpty(subproductXML));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

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

    /// <summary>
    ///
    /// </summary>
    public bool MergeWorkOrderToolValues(WorkOrder workorderInfo, string toolValuesXML, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Order_ToolValue_MRG", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_OrderCode", workorderInfo.Id, !string.IsNullOrEmpty(workorderInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Id"));
                command.Parameters.AddCondition("_XML", toolValuesXML, !string.IsNullOrEmpty(toolValuesXML));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));

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
    /// <summary>
	///
	/// </summary>
	public bool MergeWorkOrderLabor(WorkOrder workorderInfo, string JSONData, User systemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_OrderLabor_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddCondition("_OrderCode", workorderInfo.Id, !string.IsNullOrEmpty(workorderInfo.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Order Id"));
                command.Parameters.AddWithValue("_JSON", JSONData);

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
    /// <summary>
	///
	/// </summary>
	public WorkOrderResponse MergeWorkOrder(WorkOrder workorderInfo, User systemOperator, bool Validation, LevelMessage Level, ActionDB? mode = null, IntegrationSource intSrc = IntegrationSource.SF)
	{
		WorkOrderResponse returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Order_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_OrderCode", workorderInfo.OrderCode, !string.IsNullOrEmpty(workorderInfo.OrderCode));
				command.Parameters.AddCondition("_ProductId", workorderInfo.ProcessEntryId, !string.IsNullOrEmpty(workorderInfo.ProcessEntryId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Product"));
				command.Parameters.AddCondition("_PlannedStart", workorderInfo.PlannedStart, workorderInfo.PlannedStart.Year > 1900);
				command.Parameters.AddCondition("_PlannedEnd", workorderInfo.PlannedEnd, workorderInfo.PlannedEnd.Year > 1900);
				command.Parameters.AddWithValue("_PlannedQty", workorderInfo.PlannedQty);
				command.Parameters.AddCondition("_RealStart", workorderInfo.RealStart, workorderInfo.RealStart.Year > 1900);
				command.Parameters.AddCondition("_RealEnd", workorderInfo.RealEnd, workorderInfo.RealEnd.Year > 1900);
				command.Parameters.AddWithValue("_Status", workorderInfo.Status.ToInt32());
				command.Parameters.AddWithValue("_OrderType", workorderInfo.OrderType);
				command.Parameters.AddWithValue("_Formula", workorderInfo.Formula);
				command.Parameters.AddWithValue("_OrderGroup", workorderInfo.OrderGroup);
				command.Parameters.AddWithValue("_SalesOrder", workorderInfo.SalesOrder);
				command.Parameters.AddWithValue("_Comments", workorderInfo.Comments);
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddWithValue("_Level", Level);
				command.Parameters.AddCondition("_DueDate", workorderInfo.DueDate, workorderInfo.DueDate.Year > 1900);
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				command.Parameters.AddCondition("_Priority", workorderInfo.Priority, !string.IsNullOrEmpty(workorderInfo.Priority));
				command.Parameters.AddWithValue("_Origin", intSrc.ToInt32());
				command.Parameters.AddCondition("_OrderSource", workorderInfo.OrderSource, !string.IsNullOrEmpty(workorderInfo.OrderSource));
				command.Parameters.AddCondition("_LotNo", workorderInfo.LotNo, !string.IsNullOrEmpty(workorderInfo.LotNo));
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new WorkOrderResponse
					{
						Action = (ActionDB)rdr["Action"].ToInt32(),
						IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
						Code = rdr["Code"].ToStr(),
						Message = rdr["Message"].ToStr(),
					};
				}

				if (mode == ActionDB.Create)
				{
					workorderInfo.Id = workorderInfo.OrderCode;
					returnValue.WorkOrder = workorderInfo;
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
	public string UpdateMaterialManual(string transactionId, OrderComponent request, string employeeId, string workOrderId, string externalId, User systemOperator)
	{
		string returnValue = string.Empty;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Order_Trans_Material_INS", connection)
				{
					CommandType = CommandType.StoredProcedure,
					CommandTimeout = 120000
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_TransactionId", transactionId, !string.IsNullOrEmpty(transactionId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Transaction"));
				command.Parameters.AddCondition("_OrderCode", workOrderId, !string.IsNullOrEmpty(workOrderId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Work Order"));
				//  command.Parameters.AddCondition("_MachineCode", request.MachineId, !string.IsNullOrEmpty(request.MachineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine Id"));
				command.Parameters.AddWithValue("_MachineCode", request.MachineId);
				command.Parameters.AddCondition("_OperationNo", request.OperationNo, !string.IsNullOrEmpty(request.OperationNo), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Process Id"));
				command.Parameters.AddWithValue("_ItemCode", request.SourceId);
				command.Parameters.AddWithValue("_LineId", request.LineId);
				command.Parameters.AddWithValue("_Quantity", request.InputQty);
				command.Parameters.AddWithValue("_EmployeeId", employeeId);
				command.Parameters.AddCondition("_OriginalSourceId", request.OriginalSourceId, !string.IsNullOrEmpty(request.OriginalSourceId));
				command.Parameters.AddCondition("_NewFactor", request.NewFactor, !string.IsNullOrEmpty(request.OriginalSourceId));
				command.Parameters.AddCondition("_Operator", systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_BatchesJSON", request.BatchesJson, !string.IsNullOrEmpty(request.BatchesJson));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				command.Parameters.AddWithValue("_ExternalId", externalId);
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

}