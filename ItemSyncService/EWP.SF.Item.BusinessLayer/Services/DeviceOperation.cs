using EWP.SF.Item.DataAccess;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	


namespace EWP.SF.Item.BusinessLayer;

public class DeviceOperation : IDeviceOperation
{
    private readonly IDeviceRepo _deviceRepo;
    private readonly IApplicationSettings _applicationSettings;
    private readonly IAttachmentOperation _attachmentOperation;
    private readonly IActivityOperation _activityOperation;
    private readonly ISchedulingCalendarShiftsOperation _schedulingCalendarShiftsOperation;

    public DeviceOperation(IDeviceRepo deviceRepo, IApplicationSettings applicationSettings, IAttachmentOperation attachmentOperation,
     IActivityOperation activityOperation, ISchedulingCalendarShiftsOperation schedulingCalendarShiftsOperation)
    {
        _deviceRepo = deviceRepo;
        _applicationSettings = applicationSettings;
        _attachmentOperation = attachmentOperation;
        _activityOperation = activityOperation;
        _schedulingCalendarShiftsOperation = schedulingCalendarShiftsOperation;
        ;
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
        List<ProcessType> processTypes = GetProcessTypes(null, systemOperator);
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
                    _ = GetProductionLine(machine.ParentCode, systemOperator) ?? throw new Exception("Machine parent code is invalid");
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
                    _deviceRepo.SaveMachineProgramming(result.Id, machineProgramming, systemOperator);
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
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<ResponseData> CreateMachineModel(
	  MachineModel machineInfo,
	  User systemOperator,
	  bool validate = false,
	  string level = "Success",
	  bool notifyOnce = true)
	{
		ResponseData returnValue = null;

		#region Permission validation

		// if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_MACHINE_CREATE))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		returnValue = BrokerDAL.CreateMachineModel(machineInfo, systemOperator, validate, level);
		if (!validate)
		{
			if (!machineInfo.Skill.IsNull())
			{
				BrokerDAL.SaveMachineSkills(returnValue.Id, machineInfo.Skill, systemOperator);
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
				await SaveImageEntity("Machine", machineInfo.Image, machineInfo.Code, systemOperator).ConfigureAwait(false);
				if (machineInfo.AttachmentIds is not null)
				{
					foreach (string attachment in machineInfo.AttachmentIds)
					{
						await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
					}
				}

				if (machineInfo.Activities?.Count > 0)
				{
					foreach (Activity activity in machineInfo.Activities)
					{
						if (string.IsNullOrEmpty(activity.Id))
						{
							await CreateActivity(activity, systemOperator).ConfigureAwait(false);
						}
						else if (activity.ManualDelete)
						{
							await DeleteActivity(activity, systemOperator).ConfigureAwait(false);
						}
						else if (activity.ActivityClassId > 0)
						{
							await UpdateActivity(activity, systemOperator).ConfigureAwait(false);
						}
					}
				}
				if (machineInfo.Shift?.CodeShift is not null && !string.IsNullOrEmpty(machineInfo.Shift.CodeShift))
				{
					machineInfo.Shift.Validation = false;
					machineInfo.Shift.IdAsset = machineInfo.Code;
					UpdateSchedulingCalendarShifts(machineInfo.Shift, systemOperator);
				}
				if (machineInfo.ShiftDelete?.Id is not null && !string.IsNullOrEmpty(machineInfo.ShiftDelete.Id))
				{
					machineInfo.ShiftDelete.Validation = false;
					DeleteSchedulingCalendarShifts(machineInfo.ShiftDelete, systemOperator);
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

}

