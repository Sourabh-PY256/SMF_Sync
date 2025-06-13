using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using EWP.SF.Common.Models.Sensors;
using EWP.SF.Common.Constants;


namespace EWP.SF.KafkaSync.BusinessLayer;

public class DeviceOperation : IDeviceOperation
{
    
    private readonly IOEERepo _oeeRepo;
    private readonly ISkillRepo _skillRepo;
    private readonly IProcessTypeRepo _processTypeRepo;
    private readonly IMachineRepo _machineRepo;
    private readonly IProductionLinesOperation _productionLinesOperation;
    private readonly IAttachmentOperation _attachmentOperation;
    private readonly IActivityOperation _activityOperation;
    private readonly ISchedulingCalendarShiftsOperation _schedulingCalendarShiftsOperation;
    private readonly IProcessTypeOperation _processTypeOperation;



    public DeviceOperation( IAttachmentOperation attachmentOperation,
     IActivityOperation activityOperation, ISchedulingCalendarShiftsOperation schedulingCalendarShiftsOperation
     , IProcessTypeOperation processTypeOperation
     , IOEERepo oeeRepo, ISkillRepo skillRepo, IProcessTypeRepo processTypeRepo, IMachineRepo machineRepo,
     IProductionLinesOperation productionLinesOperation)
    {
      
        _attachmentOperation = attachmentOperation;
        _activityOperation = activityOperation;
        _schedulingCalendarShiftsOperation = schedulingCalendarShiftsOperation;
        _processTypeOperation = processTypeOperation;
        _oeeRepo = oeeRepo;
        _skillRepo = skillRepo;
        _processTypeRepo = processTypeRepo;
        _machineRepo = machineRepo;
        _productionLinesOperation = productionLinesOperation;
        ;
    }
    /// <summary>
	///
	/// </summary>
	public Machine[] ListDevices(
      bool deleted = false,
      bool listOnly = false,
      bool onlyActive = false,
      DateTime? DeltaDate = null,
      bool showDisabled = false)
    {
        List<Machine> devices = null;
        List<Machine> devicesAll = null;
        if (listOnly && !showDisabled)
        {
            onlyActive = true;
        }
        devices = _machineRepo.ListMachines(null, onlyActive, DeltaDate);

        if (devices is null)
        {
            return [];
        }
        if (listOnly)
        {
            List<MachineOEEConfiguration> oeeConfigs = _oeeRepo.GetMachineOeeConfiguration();
            List<ProcessType> listProcessType = _processTypeRepo.GetProcessType(null);
            List<ProcessTypeDetail> listProcessDetail = _processTypeRepo.ListMachineProcessTypeDetails(null);

            //Ensure code
            return [.. from d in devices
                 join cm in SyncService.CurrentMachines
                 on d.Id equals cm.Id
                 into tempCM
                 join ooe in oeeConfigs
                 on d.Id equals ooe.MachineId
                 into temp
                 from ooeExist in temp.DefaultIfEmpty()
                 from cmExist in tempCM.DefaultIfEmpty()
                 select new Machine
                 {
                   Description = d.Description,
                   HasTool = d.HasTool,
                   Id = d.Id,
                   ParentCode = d.ParentCode,
                   ManufactureDate = d.ManufactureDate,
                   TypeId = d.TypeId,
                   Code = d.Code, //Ensure code
								   ConfigError = d.ConfigError,
                   CreationDate = d.CreationDate,
                   CtrlModel = d.CtrlModel,
                   CtrlSerial = d.CtrlSerial,
                   Environment = d.Environment,
                   FacilityId = d.FacilityId,
                   FacilityCode = d.FacilityCode,
                   FloorId = d.FloorId,
                   LiveIconId = d.LiveIconId,
                   Image = d.Image,
                   IsAuxiliar = d.IsAuxiliar,
                   IsBusy = d.IsBusy,
                   Location = d.Location,
                   MaximumCapacity = d.MaximumCapacity,
                   MinimumCapacity = d.MinimumCapacity,
                   LotCalculation = d.LotCalculation,
                   ProductionType = d.ProductionType,
                   OEEHistory = d.OEEHistory,
                   Parameters = d.Parameters,
                   PLCManufacturer = d.PLCManufacturer,
                   PLCSerial = d.PLCSerial,
                   ProductionLineId = d.ProductionLineId,
                   Programming = d.Programming,
                   PwrSourceModel = d.PwrSourceModel,
                   RobotArmModel = d.RobotArmModel,
                   Skills = d.Skills,
                   Status = d.Status,
                   Tag = d.Tag,

                   Online = ooeExist is not null && SyncService.GetMachineValue(ooeExist.MachineId, "IO").ToBool(),
                   DownSince = ooeExist is not null && cmExist is not null ? cmExist.Environment.DowntimeDate : null,
                   OEEConfiguration = ooeExist,
                   Warehouse = d.Warehouse,
                   WarehouseCode = d.WarehouseCode,
                   BinLocations = d.BinLocations,
                   ProcessType = (from l in listProcessType
                          where l.Id == d.TypeId
                          && ooeExist is not null
                          select new ProcessType
                          {
                            Id = l.Id,
                            Details = listProcessDetail?.Where(x => x.MachineId == d.Id)?.ToList()
                          }).FirstOrDefault(),
                   LogDetailId = d.LogDetailId
                 }];
        }

        List<MachineParam> parameters = _machineRepo.ListMachineParams();
        List<Skill> skills = _skillRepo.ListMachineSkills();

        skills ??= [];
        parameters ??= [];

        devicesAll = [.. from d in devices
              join dp in parameters
              on d.Id equals dp.MachineId
              into tempP
              join dss in skills
              on d.Id equals dss.ParentId
              into tempSS
              select new Machine
              {
                Description = d.Description,
                Id = d.Id,
                ManufactureDate = d.ManufactureDate,
                TypeId = d.TypeId,
                Code = d.Code,
                ConfigError = d.ConfigError,
                CreationDate = d.CreationDate,
                CtrlModel = d.CtrlModel,
                CtrlSerial = d.CtrlSerial,
                Environment = new MachineEnvironment(),
                FacilityId = d.FacilityId,
                FloorId = d.FloorId,
                LiveIconId = d.LiveIconId,
                Image = d.Image,
                IsAuxiliar = d.IsAuxiliar,
                IsBusy = d.IsBusy,
                Location = d.Location,
                MaximumCapacity = d.MaximumCapacity,
                MinimumCapacity = d.MinimumCapacity,
                LotCalculation = d.LotCalculation,
                OEEHistory = d.OEEHistory,
                PLCManufacturer = d.PLCManufacturer,
                PLCSerial = d.PLCSerial,
                ProductionLineId = d.ProductionLineId,
                ParentCode = d.ParentCode,
                Programming = d.Programming,
                PwrSourceModel = d.PwrSourceModel,
                RobotArmModel = d.RobotArmModel,
                Status = d.Status,
                Tag = d.Tag,
                ProductionType = d.ProductionType,
                Skills = tempSS?.Select(x => x.Id).ToList(),
                Parameters = tempP?.ToList(),
                Warehouse = d.Warehouse,
                WarehouseCode = d.WarehouseCode,
                BinLocations = d.BinLocations,
                LogDetailId = d.LogDetailId
              }];

        return devicesAll?.Where(x => (deleted && x.Status == Status.Deleted) || (!deleted && x.Status != Status.Deleted)).ToArray();
    }
    /// <summary>
    ///
    /// </summary>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<ResponseData> CreateMachine(
      Machine machineInfo,
      User systemOperator,
      bool validate = false,
      string level = "Success",
      bool notifyOnce = true)
    {
        ResponseData returnValue = null;

        #region Permission validation

        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_MACHINE_CREATE))
        {
            throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        returnValue = _machineRepo.CreateMachine(machineInfo, systemOperator, validate, level);
        if (!validate)
        {
            if (!machineInfo.Skills.IsNull())
            {
                _skillRepo.SaveMachineSkills(returnValue.Id, machineInfo.Skills, systemOperator);
            }
            if (!string.IsNullOrEmpty(returnValue.Id))
            {
                SyncInitializer.ForcePush(new MessageBroker
                {
                    Type = MessageBrokerType.MachineUpdate,
                    ElementId = null,
                    ElementValue = null,
                    MachineId = returnValue.Id,
                    Aux = null
                });
            }

            List<Machine> listMachine = [.. ListDevices()];
            Machine ObjMachine = listMachine.Where(x => x.Id == returnValue.Id).FirstOrDefault(x => x.Status != Status.Failed);
            await ObjMachine.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            if (notifyOnce)
            {
                await _attachmentOperation.SaveImageEntity("Machine", machineInfo.Image, machineInfo.Code, systemOperator).ConfigureAwait(false);
                if (machineInfo.AttachmentIds is not null)
                {
                    foreach (string attachment in machineInfo.AttachmentIds)
                    {
                        await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                    }
                }
                if (machineInfo.Skills?.Count > 0)
                {
                    ObjMachine.Skills = machineInfo.Skills;
                }
                if (machineInfo.Activities?.Count > 0)
                {
                    foreach (Activity activity in machineInfo.Activities)
                    {
                        if (string.IsNullOrEmpty(activity.Id))
                        {
                            await _activityOperation.CreateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ManualDelete)
                        {
                            await _activityOperation.DeleteActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ActivityClassId > 0)
                        {
                            await _activityOperation.UpdateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                    }
                }
                if (machineInfo.Shift?.CodeShift is not null && !string.IsNullOrEmpty(machineInfo.Shift.CodeShift))
                {
                    machineInfo.Shift.Validation = false;
                    machineInfo.Shift.IdAsset = machineInfo.Code;
                    _schedulingCalendarShiftsOperation.UpdateSchedulingCalendarShifts(machineInfo.Shift, systemOperator);
                }
                if (machineInfo.ShiftDelete?.Id is not null && !string.IsNullOrEmpty(machineInfo.ShiftDelete.Id))
                {
                    machineInfo.ShiftDelete.Validation = false;
                    _schedulingCalendarShiftsOperation.DeleteSchedulingCalendarShifts(machineInfo.ShiftDelete, systemOperator);
                }

                // ServiceManager.SendMessage(
                // 	MessageBrokerType.CatalogChanged,
                // 	new
                // 	{
                // 		Catalog = Entities.Machine,
                // 		returnValue.Action,
                // 		Data = ObjMachine
                // 	}
                //   );
            }
        }

        return returnValue;
    }
    /// <summary>
    ///
    /// </summary>
    public async Task<List<ResponseData>> ListUpdateMachine(
      List<MachineExternal> listMachines,
      List<MachineExternal> listMachinesOriginal,
      User systemOperator,
      bool validate,
      string level
    )
    {
        List<ResponseData> returnValue = [];
        ResponseData messageError;
        bool notifyOnce = listMachines.Count == 1;
        List<ProcessType> processTypes = _processTypeOperation.GetProcessTypes(null, systemOperator);
        int line = 0;
        foreach (MachineExternal cycleMachine in listMachines)
        {
            line++;
            MachineExternal machine = cycleMachine;
            try
            {
                Machine originalMachine = GetDevice(machine.MachineCode);
                bool editMode = originalMachine is not null;
                if (editMode)
                {
                    machine = listMachinesOriginal.Find(x => x.MachineCode == cycleMachine.MachineCode);
                    machine ??= cycleMachine;
                }
                Status status;
                try
                {
                    status = Enum.Parse<Status>(machine.Status);
                }
                catch
                {
                    if (!string.IsNullOrEmpty(machine.Status))
                    {
                        status = string.Equals(machine.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled;
                    }
                    else
                    {
                        status = Status.Disabled;
                    }
                }
                if (!editMode && status == Status.Disabled)
                {
                    throw new Exception("Cannot import Machines in a disabled status");
                }
                if (!editMode || (!string.IsNullOrEmpty(machine.Type) && string.Equals(machine.Type, "PROCESS", StringComparison.OrdinalIgnoreCase)) || !string.IsNullOrEmpty(machine.OperationType))
                {
                    machine.Type = "Process";
                    if (string.IsNullOrEmpty(machine.OperationType) && !editMode)
                    {
                        throw new Exception("Process Machines require an Operation Type");
                    }
                    else if (!string.IsNullOrEmpty(machine.OperationType))
                    {
                        _ = processTypes.Find(x => x.Id == machine.OperationType) ?? throw new Exception("Process Machines Operation Type is invalid");
                    }
                }

                if (!editMode || !string.IsNullOrEmpty(machine.ParentCode))
                {
                    _ = _productionLinesOperation.GetProductionLine(machine.ParentCode, systemOperator) ?? throw new Exception("Machine parent code is invalid");
                }

                Machine machineInfo = null;
                if (!editMode)
                {
                    machineInfo = new Machine
                    {
                        Code = machine.MachineCode,
                        Description = machine.MachineName,
                        ParentCode = machine.ParentCode,
                        Status = (Status)status.ToInt32(),
                        IsAuxiliar = machine.Type != "Process",
                        TypeId = machine.OperationType,
                        MinimumCapacity = machine.MinimumCapacity.ToInt32(),
                    };
                    if (machine.MaximumCapacity.HasValue)
                    {
                        machineInfo.MaximumCapacity = machine.MaximumCapacity.ToInt32();
                    }

                    machineInfo.Programming = new MachineProgramming
                    {
                        Attribute2 = machine.Attribute2,
                        Attribute3 = machine.Attribute3,
                        CapacityMode = machine.CapacityMode,
                        ConcurrentSetupTime = machine.ConcurrentSetupTime?.ToUpperInvariant() == "YES",
                        Schedule = machine.Schedule?.ToUpperInvariant() == "YES",
                        GroupChange = machine.GroupChange,
                        InfinityModeBehaviur = machine.InfiniteModeBehavior,
                        Planning = machine.Planning?.ToUpperInvariant() == "YES",
                        ScheduleLevel = string.IsNullOrEmpty(machine.ScheduleLevel) ? Convert.ToInt16(0) : Convert.ToInt16(machine.ScheduleLevel?.ToUpperInvariant() == "PRIMARY" ? 1 : 2),
                        CostPerHour = machine.CostPerHour,
                        TheoricEfficiency = machine.TheoricEfficiency,
                        GanttPosition = machine.GanttPosition
                    };
                    machineInfo.OEEConfiguration = new MachineOEEConfiguration
                    {
                        ProductionType = string.Equals(machine.ProductionType.ToStr(), "PIECES", StringComparison.OrdinalIgnoreCase) ? "Pieces" : "Batch",
                        PerformanceDefaultUnit = machine.EfficiencyUnit
                    };

                    if (machine.RunTime.HasValue)
                    {
                        machineInfo.OEEConfiguration.PerformanceDefaultTimeQty = machine.RunTime.Value;
                    }
                }
                else
                {
                    machineInfo = originalMachine;

                    if (!string.IsNullOrEmpty(machine.EfficiencyUnit))
                    {
                        machineInfo.OEEConfiguration.PerformanceDefaultUnit = machine.EfficiencyUnit;
                    }
                    if (machine.MaximumCapacity.HasValue)
                    {
                        machineInfo.MaximumCapacity = machine.MaximumCapacity.ToInt32();
                    }
                    if (!string.IsNullOrEmpty(machine.Status))
                    {
                        machineInfo.Status = status;
                    }
                    if (!string.IsNullOrEmpty(machine.MachineName))
                    {
                        machineInfo.Description = machine.MachineName;
                    }
                    if (machine.Attribute2.HasValue)
                    {
                        machineInfo.Programming.Attribute2 = machine.Attribute2;
                    }
                    if (machine.Attribute3.HasValue)
                    {
                        machineInfo.Programming.Attribute3 = machine.Attribute3;
                    }
                    if (!string.IsNullOrEmpty(machine.CapacityMode))
                    {
                        machineInfo.Programming.CapacityMode = machine.CapacityMode;
                    }
                    if (!string.IsNullOrEmpty(machine.ConcurrentSetupTime))
                    {
                        machineInfo.Programming.ConcurrentSetupTime = machine.ConcurrentSetupTime?.ToUpperInvariant() == "YES";
                    }
                    if (!string.IsNullOrEmpty(machine.Schedule))
                    {
                        machineInfo.Programming.Schedule = machine.Schedule?.ToUpperInvariant() == "YES";
                    }
                    if (!string.IsNullOrEmpty(machine.GroupChange))
                    {
                        machineInfo.Programming.GroupChange = machine.GroupChange;
                    }
                    if (!string.IsNullOrEmpty(machine.ConcurrentSetupTime))
                    {
                        machineInfo.Programming.ConcurrentSetupTime = machine.ConcurrentSetupTime?.ToUpperInvariant() == "YES";
                    }
                    if (!string.IsNullOrEmpty(machine.InfiniteModeBehavior))
                    {
                        machineInfo.Programming.InfinityModeBehaviur = machine.InfiniteModeBehavior;
                    }
                    if (!string.IsNullOrEmpty(machine.Planning))
                    {
                        machineInfo.Programming.Planning = machine.Planning?.ToUpperInvariant() == "YES";
                    }
                    if (!string.IsNullOrEmpty(machine.ScheduleLevel))
                    {
                        machineInfo.Programming.ScheduleLevel = Convert.ToInt16(machine.ScheduleLevel?.ToUpperInvariant() == "PRIMARY" ? 1 : 2);
                    }
                    if (!string.IsNullOrEmpty(machine.TheoricEfficiency))
                    {
                        machineInfo.Programming.TheoricEfficiency = machine.TheoricEfficiency;
                    }
                    if (!string.IsNullOrEmpty(machine.GanttPosition))
                    {
                        machineInfo.Programming.GanttPosition = machine.GanttPosition;
                    }
                    if (machine.RunTime.HasValue && machineInfo.OEEConfiguration is not null)
                    {
                        machineInfo.OEEConfiguration.PerformanceDefaultTimeQty = machine.RunTime.Value;
                    }

                    machineInfo.Programming.CostPerHour = machine.CostPerHour;
                    machineInfo.OEEConfiguration = originalMachine.OEEConfiguration;

                    if (!string.IsNullOrEmpty(machine.ProductionType))
                    {
                        machineInfo.OEEConfiguration.ProductionType = string.Equals(machine.ProductionType.ToStr(), "PIECES", StringComparison.OrdinalIgnoreCase) ? "Pieces" : "Batch";
                    }

                    machineInfo.ProcessTypeDetails = originalMachine.ProcessTypeDetails;
                    machineInfo.Skills = originalMachine.Skills;

                    if (string.IsNullOrEmpty(machineInfo.ParentCode))
                    {
                        machineInfo.ParentCode = null;
                    }
                    if (string.IsNullOrEmpty(machineInfo.TypeId))
                    {
                        machineInfo.IsAuxiliar = originalMachine.IsAuxiliar;
                        machineInfo.TypeId = null;
                    }
                    else
                    {
                        machineInfo.IsAuxiliar = false;
                        machineInfo.TypeId = machine.OperationType;
                    }
                }
                ResponseData result = await CreateMachine(machineInfo, systemOperator, validate, level, notifyOnce).ConfigureAwait(false);
                returnValue.Add(result);
                if (!validate && result.Action == ActionDB.Create && result.IsSuccess && !editMode && machineInfo.Programming is null)
                {
                    MachineProgramming machineProgramming = new()
                    {
                        GanttPosition = "0",
                        TheoricEfficiency = "0",
                        CostPerHour = machine.CostPerHour,
                        Schedule = false
                    };
                    _oeeRepo.SaveMachineProgramming(result.Id, machineProgramming, systemOperator);
                }
            }
            catch (Exception ex)
            {
                messageError = new ResponseData
                {
                    Message = ex.Message,
                    Code = "Line:" + line.ToStr()
                };
                returnValue.Add(messageError);
            }
        }

        // if (!validate && !notifyOnce)
        // {
        // 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Machine, Action = ActionDB.IntegrateAll.ToStr() });
        // }
        if (returnValue.Count > 0)
        {
            switch (Enum.Parse<LevelMessage>(level))
            {
                case LevelMessage.Warning:
                    returnValue = [.. returnValue.Where(x => !string.IsNullOrEmpty(x.Message))];
                    break;
                case LevelMessage.Error:
                    returnValue = [.. returnValue.Where(x => !x.IsSuccess)];
                    break;
                case LevelMessage.Success:
                    break;
            }
        }

        return returnValue;
    }

    /// <summary>
    ///
    /// </summary>
    public Machine GetDevice(string machineId, bool whenThen = false)
    {
        Sensor sensorTemp = null;
        Machine returnValue = _machineRepo.ListMachines(machineId)?.FirstOrDefault();
        returnValue.Name = returnValue.Description;
        if (returnValue is not null && returnValue.Status != Status.Deleted)
        {
            returnValue.Environment = new MachineEnvironment();
            returnValue.ProcessType = _processTypeRepo.GetProcessType(returnValue.TypeId).FirstOrDefault();
            returnValue.OEEConfiguration = _oeeRepo.GetMachineOeeConfiguration(returnValue.Id);
            returnValue.Programming = _oeeRepo.GetMachineProgramming(returnValue.Id);
            if (returnValue.ProcessType is not null && returnValue.OEEConfiguration is not null)
            {
                returnValue.ProcessType.Details = _processTypeRepo.ListMachineProcessTypeDetails(machineId);
            }
            List<Sensor> sensors = _machineRepo.ListSensors(null, machineId);
            List<Sensor> sensorsDetails = _machineRepo.GetSensors(null, machineId);
            if (whenThen)
            {
                try
                {
                    sensorTemp = _machineRepo.GetSensorsDetails(null);
                }
                catch (Exception ex)
                {
                    //logger.Error(ex);
                }
            }
            List<MachineParam> parameters = _machineRepo.ListMachineParams(null, machineId);

            if (sensors is not null)
            {
                if (sensorTemp is not null)
                {
                    foreach (Sensor sensor in sensorsDetails)
                    {
                        sensor.SensorsWhen = [.. sensorTemp.SensorsWhen.Where(x => x.SensorId == sensor.Code)];
                        sensor.SensorLiveViewer = [.. sensorTemp.SensorLiveViewer.Where(x => x.SensorId == sensor.Code)];
                    }
                }
            }
            else
            {
                sensors = [];
            }
            parameters ??= [];
            returnValue.Parameters = parameters;
            returnValue.Sensors = sensors;
            returnValue.SensorDetails = sensorsDetails;
        }

        return returnValue;
    }

}

