﻿using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using EWP.SF.Common.Models;
using EWP.SF.Common.Models.Sensors;

namespace EWP.SF.Item.BusinessLayer;

public class OEEOperation : IOEEOperation
{
	private readonly IOEERepo _oeeRepo;
	private const string noPermission = "User doesn't have permission for this action";
	private readonly IApplicationSettings _applicationSettings;

	private readonly IProcessTypeOperation _processTypeOperation;

	private readonly IAttachmentOperation _attachmentOperation;

	public OEEOperation(IOEERepo oeeRepo, IApplicationSettings applicationSettings
	, IProcessTypeOperation processTypeOperation, IAttachmentOperation attachmentOperation)
	{
		_oeeRepo = oeeRepo;
		_applicationSettings = applicationSettings;
		_processTypeOperation = processTypeOperation;
		_attachmentOperation = attachmentOperation;
	}
		/// <summary>
		///
		/// </summary>
		public double SaveSensorAverage(Sensor sensor, string WorkOrderId, double value) => _oeeRepo.SaveSensorAverage(sensor.Id, WorkOrderId, value);

		/// <summary>
		///
		/// </summary>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public MachineOEEConfiguration GetMachineOEEConfiguration(string machineId, User systemOperator)
		{
			#region Permission validation

			if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.CP_MACHINE_EDIT))
			{
				throw new UnauthorizedAccessException(noPermission);
			}

			#endregion Permission validation

			return _oeeRepo.GetMachineOeeConfiguration(machineId);
		}

		/// <summary>
		///
		/// </summary>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public MachineProgramming GetMachineProgramming(string machineId, User systemOperator)
		{
			#region Permission validation

			if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.CP_MACHINE_EDIT))
			{
				throw new UnauthorizedAccessException(noPermission);
			}

			#endregion Permission validation

			return _oeeRepo.GetMachineProgramming(machineId);
		}

		/// <summary>
		///
		/// </summary>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public bool SaveMachineOEEConfiguration(string machineId, MachineOEEConfiguration machineConfig, User systemOperator)
		{
			bool returnValue = false;

			#region Permission validation

			if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.CP_MACHINE_EDIT))
			{
				throw new UnauthorizedAccessException(noPermission);
			}

			#endregion Permission validation

			returnValue = _oeeRepo.SaveMachineOeeConfiguration(machineId, machineConfig, systemOperator);
			// if (returnValue)
			// {
			// 	Services.SyncInitializer.ForcePush(new MessageBroker
			// 	{
			// 		Type = MessageBrokerType.MachineUpdate,
			// 		ElementId = null,
			// 		ElementValue = null,
			// 		MachineId = machineId,
			// 		Aux = null
			// 	});
			// }
			return returnValue;
		}

		/// <summary>
		///
		/// </summary>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public bool SaveMachineProgramming(string machineId, MachineProgramming machineConfig, User systemOperator)
		{
			bool returnValue = false;

			#region Permission validation

			if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.CP_MACHINE_EDIT))
			{
				throw new UnauthorizedAccessException(noPermission);
			}

			#endregion Permission validation

			returnValue = _oeeRepo.SaveMachineProgramming(machineId, machineConfig, systemOperator);
			// if (returnValue)
			// {
			// 	Services.SyncInitializer.ForcePush(new MessageBroker
			// 	{
			// 		Type = MessageBrokerType.MachineUpdate,
			// 		ElementId = null,
			// 		ElementValue = null,
			// 		MachineId = machineId,
			// 		Aux = null
			// 	});
			// }
			return returnValue;
		}

		/// <summary>
		///
		/// </summary>
		public Task<OEEModel> GetLiveOEEAsync(string machineid, DateTime? startDate) => _oeeRepo.GetLiveOee(machineid, startDate);

		/// <summary>
		///
		/// </summary>
		public bool SaveMachineAvailability(string machineId, bool online, User systemOperator, OEEMode mode, string downtimeId = "") => _oeeRepo.SaveMachineAvailability(machineId, online, systemOperator, mode, downtimeId);

		/// <summary>
		///
		/// </summary>
		public bool SaveMachinePerformance(string machineId, string processId, bool isoutput, string workOrderId, double value, double factor, double deviceFactor, double processFactor, double orderFactor) => _oeeRepo.SaveMachinePerformance(machineId, processId, isoutput, workOrderId, value, factor, deviceFactor, processFactor, orderFactor);

		/// <summary>
		///
		/// </summary>
		public double SaveMachineQuality(string machineId, bool isoutput, string workOrderId, string processId, string shiftId, QualityType type, QualityMode mode, double sample, double rejected, User systemOPerator)
		{
			return _oeeRepo.SaveMachineQuality(machineId, isoutput, workOrderId, processId, shiftId, type, mode, string.Empty, string.Empty, sample, rejected, systemOPerator);
		}

	}

