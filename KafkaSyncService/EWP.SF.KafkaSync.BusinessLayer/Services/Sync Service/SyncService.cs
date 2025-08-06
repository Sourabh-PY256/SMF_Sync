// using System.Collections.Concurrent;
// using System.Text;
// using System.Text.Json;



// using DynamicExpresso;
// using EWP.SF.Common.Enumerators;
// using EWP.SF.Common.Models;
// using EWP.SF.Common.ResponseModels;
// using EWP.SF.Common.Models.Sensors;
// using EWP.SF.Common.EventHandlers;
// using EWP.SF.Common.Models.Operations;

// using EWP.SF.Helper;

// using NLog;

// using Machine = EWP.SF.Common.Models.Machine;
// using EWP.SF.Common.Models.NotificationSettings;

// namespace EWP.SF.KafkaSync.BusinessLayer;

// /// <summary>
// ///
// /// </summary>
// public static class SyncService
// {
// 	#region Events

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static EventHandler<string> OnMachineProcessed;

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static EventHandler<string> OnPrintLabel;

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static EventHandler<MachineProducedNotification> onMachineReportProduction;

// 	#endregion Events

// 	#region Attributes

// 	private static Operations BusinessProcess;
// 	private static List<Machine> Machines;
// 	private static Dictionary<string, Machine> UpdatedMachines;
// 	private static readonly Logger logger = LogManager.GetCurrentClassLogger();
// 	private static readonly TimeSpan delayBetweenMessages = TimeSpan.FromMilliseconds(50); // ~20 messages per second
// 	private static readonly BlockingCollection<MsTeamsNotification> messageQueue = [];

// 	private static ConcurrentDictionary<string, ConcurrentDictionary<string, object>> MachineValues { get; set; }
// 	private static ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>> MachineTimestamps { get; set; }

// 	#endregion Attributes

// 	#region Public Methods

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static async Task Initialize()
// 	{
// 		BusinessProcess = new Operations();
// 		UpdatedMachines = [];
// 		await GetActiveDevicesAsync().ConfigureAwait(false);

// 		logger.Debug("Sync Service Initialized");
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static bool SetDeviceDown(string machineId) => BusinessProcess.SetDeviceDown(machineId);

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static List<Machine> CurrentMachines
// 	{
// 		get
// 		{
// 			return [.. Machines];
// 		}
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static object GetStatus()
// 	{
// 		return new
// 		{
// 			MachineCount = Machines.Count,
// 			MachineDetails = Machines.Select(static x => new
// 			{
// 				x.Code,
// 				Name = x.Description,
// 				Status = x.Status.ToStr(),
// 				Errors = x.ConfigError,
// 				ProcessType = new { x.ProcessType?.Name, Details = x.ProcessType?.Details?.Select(static x => new { x.Name, x.Code, ValueType = x.ValueType.ToStr(), Source = x.ValueSourceId }).ToArray() },
// 				AvailabilityMode = x.OEEConfiguration?.AvailabilityMode.ToStr(),
// 				AvailabilityConfigured = x.OEEConfiguration is not null && (x.OEEConfiguration.AvailabilitySensor is Sensor || x.OEEConfiguration.AvailabilitySensor is MachineParam),
// 				AvailabilityTimeout = x.OEEConfiguration?.IdleSeconds,
// 				PerformanceMode = x.OEEConfiguration?.PerformanceMode.ToStr(),
// 				QualityMode = x.OEEConfiguration?.QualityMode.ToStr(),
// 				Sensors = x.Sensors?.Select(static p => new { p.Code, p.Status }).ToArray(),
// 				Parameters = x.Parameters?.Select(static p => new { p.Code, p.Status, p.Formula }).ToArray(),
// 				Environment = x.Environment.Values,
// 				RunningValues = MachineValues.TryGetValue(x.Id, out ConcurrentDictionary<string, object> value) ? value : null
// 			}).ToArray()
// 		};
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static async Task<int> ProcessMachine(string machineId, string sensorId, string value, bool ignoreProcess = false, string batchHistory = "")
// 	{
// 		const int returnValue = -1;
// 		try
// 		{
// 			Machine processedMachine = Machines.Find(x => x.Id == machineId && x.Status == Status.Active && !x.IsBusy);

// 			if (processedMachine is not null && string.IsNullOrEmpty(processedMachine.ConfigError))
// 			{
// 				try
// 				{
// 					processedMachine.IsBusy = true;

// 					await LoadMachineContext(processedMachine).ConfigureAwait(false);

// 					processedMachine.Online = value.ToBool();

// 					ExpressionResolver resolver = (ExpressionResolver)processedMachine.Tag;

// 					// Use Task.Run to execute the async method safely
// 					_ = Task.Run(async () =>
// 					{
// 						try
// 						{
// 							await resolver.Resolve(sensorId, ignoreProcess, batchHistory).ConfigureAwait(false);
// 						}
// 						catch (Exception ex)
// 						{
// 							// Log the exception instead of crashing the process
// 							Console.WriteLine(ex);
// 						}
// 					});
// 				}
// 				catch (Exception ex)
// 				{
// 					processedMachine.IsBusy = false;
// 					logger.Error(string.Format("Error resolving Machine {0}: {1}. StackTrace::{2}", processedMachine.Description, ex.Message, ex.StackTrace), true);
// 				}
// 			}
// 			else if (OnMachineProcessed is not null && processedMachine is not null)
// 			{
// 				OnMachineProcessed(new Exception(processedMachine.ConfigError), machineId);
// 			}
// 		}
// 		catch (Exception ex)
// 		{
// 			logger.Error("Error in ProcessMachine() - " + ex.Message + "StackTrace:" + ex.StackTrace, true);
// 		}
// 		return returnValue;
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static Task RefreshMachine(string machineId) => ProcessMachine(machineId, null, null, true);

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static async Task<Machine> UpdateMachine(string machineId)
// 	{
// 		Machine updatedMachine = null;
// 		try
// 		{
// 			bool isNew = !Machines.Any(x => x.Id == machineId);
// 			updatedMachine = BusinessProcess.GetDevice(machineId, true);

// 			if (!updatedMachine.IsAuxiliar)
// 			{
// 				try
// 				{
// 					BusinessProcess.SetMachineSyncConfig(updatedMachine);
// 				}
// 				catch (Exception ex)
// 				{
// 					logger.Error(ex);
// 					updatedMachine.ConfigError = ex.Message;
// 				}
// 			}
// 			if (isNew && !MachineValues.ContainsKey(updatedMachine.Id))
// 			{
// 				_ = MachineValues.AddOrUpdate(updatedMachine.Id, new ConcurrentDictionary<string, object>(), (_, _) => new ConcurrentDictionary<string, object>());
// 				_ = MachineTimestamps.AddOrUpdate(updatedMachine.Id, new ConcurrentDictionary<string, DateTime>(), (_, _) => new ConcurrentDictionary<string, DateTime>());
// 				ExpressionResolver resolver = new(updatedMachine, BusinessProcess);
// 				resolver.OnResolve += Resolver_OnResolve_Handler;
// 				resolver.OnLog += Resolver_OnLog;
// 				resolver.onMachineProduced += Resolver_onMachineProduced;
// 				updatedMachine.Tag = resolver;
// 				Machines.Add(updatedMachine);
// 				// if (ServiceManager.appSettings.GetAppSetting("MemoryMode").Contains("read", StringComparison.OrdinalIgnoreCase))
// 				// {
// 				// 	string json = JsonSerializer.Serialize(Machines);
// 				// 	string base64 = Convert.ToBase64String(json.ToBytes());
// 				// 	GarnetConnectionClient.SetValue("CurrentMachines", base64);
// 				// }
// 			}
// 			else
// 			{
// 				Machine currentMachine = Machines.Find(x => x.Id == machineId);
// 				if (currentMachine?.IsBusy == true)
// 				{
// 					if (!UpdatedMachines.TryAdd(machineId, updatedMachine))
// 					{
// 						UpdatedMachines[machineId] = updatedMachine;
// 					}
// 				}
// 				else if (currentMachine is not null && updatedMachine is not null)
// 				{
// 					if (currentMachine.Tag is ExpressionResolver existingResolver)
// 					{
// 						existingResolver.SetMachine(updatedMachine);
// 						updatedMachine.Tag = existingResolver;
// 					}
// 					else
// 					{
// 						using ExpressionResolver resolver = new(updatedMachine, BusinessProcess);
// 						resolver.OnResolve += Resolver_OnResolve_Handler;
// 						resolver.OnLog += Resolver_OnLog;
// 						resolver.onMachineProduced += Resolver_onMachineProduced;

// 						updatedMachine.Tag = resolver;  // Important: Only assign after `using` if `Tag` doesn't maintain ownership
// 					}

// 					_ = Machines.Remove(currentMachine);
// 					currentMachine = null;
// 					Machines.Add(updatedMachine);
// 					// if (ServiceManager.appSettings.GetAppSetting("MemoryMode").Contains("read", StringComparison.OrdinalIgnoreCase))
// 					// {
// 					// 	string json = JsonSerializer.Serialize(Machines);
// 					// 	string base64 = Convert.ToBase64String(json.ToBytes());
// 					// 	GarnetConnectionClient.SetValue("CurrentMachines", base64);
// 					// }
// 				}
// 			}
// 			if (!MachineValues[updatedMachine.Id].ContainsKey("@ConfigError"))
// 			{
// 				_ = MachineValues[updatedMachine.Id].AddOrUpdate("@ConfigError", updatedMachine.ConfigError, (_, _) => updatedMachine.ConfigError);
// 			}
// 			else
// 			{
// 				MachineValues[updatedMachine.Id]["@ConfigError"] = updatedMachine.ConfigError;
// 			}
// 			await ProcessMachine(machineId, null, null, true).ConfigureAwait(false);
// 		}
// 		catch (Exception e)
// 		{
// 			logger.Error($"UpdateMachine error for machine {machineId}", e);
// 		}

// 		return updatedMachine;
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static void UpdateAttendingNotify(string sensorId, bool value)
// 	{
// 		Machine tempMachine = Machines.Find(x => x.Sensors.Any(x => x.Code == sensorId));
// 		if (tempMachine is not null)
// 		{
// 			Sensor sensor = tempMachine.Sensors.Find(x => x.Code == sensorId);
// 			if (sensor is not null)
// 			{
// 				sensor.AttendingNotify = value;
// 			}
// 		}
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static string GetMachineValue(string MachineId, string key)
// 	{
// 		string returnValue = string.Empty;
// 		if (MachineValues?.ContainsKey(MachineId) == true && MachineValues[MachineId].TryGetValue(key, out object value))
// 		{
// 			returnValue = value.ToStr();
// 		}
// 		return returnValue;
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static List<Machine> GetMachine(string MachineCode) => string.IsNullOrEmpty(MachineCode) ? Machines : [.. Machines.Where(x => x.Code == MachineCode)];

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static async Task<Dictionary<string, object>> GetMachineValues(string MachineId, bool ignoreIO = false)
// 	{
// 		Dictionary<string, object> tempValues = [];
// 		Machine tempMachine = Machines.Find(x => x.Id == MachineId);
// 		if (tempMachine is not null && MachineValues[MachineId]?.Count > 0)
// 		{
// 			//Environment Variables
// 			foreach (string key in tempMachine.Environment.Values.Keys)
// 			{
// 				if (!MachineValues[MachineId].ContainsKey(key))
// 				{
// 					_ = MachineValues[MachineId].AddOrUpdate(key, tempMachine.Environment.Values[key], (k, _) => tempMachine.Environment.Values[k]);
// 				}
// 				else
// 				{
// 					MachineValues[MachineId][key] = tempMachine.Environment.Values[key];
// 				}
// 			}

// 			if (tempMachine.ProcessType?.Details is not null)
// 			{
// 				foreach (ProcessTypeDetail key in tempMachine.ProcessType.Details)
// 				{
// 					if (!MachineValues[MachineId].ContainsKey(key.Code))
// 					{
// 						_ = MachineValues[MachineId].AddOrUpdate(key.Code, key.Value, (_, _) => key.Value);
// 					}
// 					else
// 					{
// 						MachineValues[MachineId][key.Code] = key.Value;
// 					}
// 				}
// 			}

// 			string[] keys = [.. MachineValues[MachineId].Keys];
// 			//Sensor and Parameter Variables
// 			for (int i = 0; i < keys.Length; i++)
// 			{
// 				if ((
// 					keys[i].StartsWith('$') || keys[i].StartsWith('@') ||
// 					(keys[i] == "IO" && !ignoreIO) || keys[i].Contains("BkColor")
// 					|| keys[i].Contains("Flicker") || keys[i].Contains("DateCollector")
// 					|| keys[i].Contains("ReadDateCollector") || keys[i].Contains("ProcessDateCollector") || keys[i].Contains("DateSendApi")
// 					|| keys[i].Contains("SendDateCollector") || keys[i].Contains("DateReadApi") || keys[i].Contains("DateInsert")
// 					|| (tempMachine.Sensors?.Any(x => x.Status == Status.Active && x.Code == keys[i] && x.LiveScreen) == true)
// 					|| (tempMachine.Parameters?.Any(x => x.Status == Status.Active && x.Code == keys[i] && x.LiveScreen) == true)
// 					) && !tempValues.ContainsKey(keys[i]))
// 				{
// 					tempValues.Add(keys[i], MachineValues[MachineId][keys[i]]);
// 				}
// 			}
// 			if (!tempValues.ContainsKey("IO"))
// 			{
// 				tempValues.Add("IO", 1);
// 			}
// 			if (!MachineValues[MachineId].ContainsKey("IO"))
// 			{
// 				_ = MachineValues[MachineId].AddOrUpdate("IO", 1, (_, _) => 1);
// 			}
// 			if (!tempValues.ContainsKey("IO"))
// 			{
// 				tempValues.Add("IO", 1);
// 			}
// 			if (tempValues.ContainsKey("IO") && !string.IsNullOrEmpty(tempMachine.Environment.DowntimeDate) && !string.IsNullOrEmpty(tempMachine.Environment.DowntimeId))
// 			{
// 				tempValues["IO"] = 0;
// 			}
// 			else
// 			{
// 				tempValues["IO"] = 1;
// 			}
// 			MachineValues[MachineId]["IO"] = tempValues["IO"];
// 		}

// 		// if (ServiceManager.appSettings.GetAppSetting("MemoryMode").Contains("write", StringComparison.OrdinalIgnoreCase))
// 		// {
// 		// 	string json = JsonSerializer.Serialize(MachineValues);
// 		// 	string base64 = Convert.ToBase64String(json.ToBytes());
// 		// 	GarnetConnectionClient.SetValue("MachineData", base64);
// 		// }
// 		return tempValues;
// 	}

// 	/// <summary>
// 	/// Merge the machines from memory.
// 	/// </summary>
// 	// public static async Task MergeMachinesFromMemory(string[] machineList)
// 	// {
// 	// 	if (ServiceManager.appSettings.GetAppSetting("MemoryMode").Contains("write", StringComparison.OrdinalIgnoreCase))
// 	// 	{
// 	// 		try
// 	// 		{
// 	// 			string base64 = GarnetConnectionClient.GetValue("CurrentMachines");
// 	// 			byte[] data = base64 is not null ? Convert.FromBase64String(base64) : null;
// 	// 			string dataDecoded = Encoding.UTF8.GetString(data);

// 	// 			List<Machine> tempValue = JsonSerializer.Deserialize<List<Machine>>(dataDecoded);

// 	// 			if (tempValue?.Count > 0)
// 	// 			{
// 	// 				await Parallel.ForEachAsync(machineList, async (machineCode, _) =>
// 	// 				{
// 	// 					Machine localDevice = Machines.Find(x => x.Id == machineCode);
// 	// 					Machine sharedDevice = tempValue.Find(x => x.Id == machineCode);

// 	// 					if (sharedDevice is not null)
// 	// 					{
// 	// 						if (localDevice is null)
// 	// 						{
// 	// 							await UpdateMachine(machineCode).ConfigureAwait(false);
// 	// 						}
// 	// 						else
// 	// 						{
// 	// 							localDevice.Status = sharedDevice.Status;
// 	// 							localDevice.BinLocations = sharedDevice.BinLocations;
// 	// 							localDevice.LotCalculation = sharedDevice.LotCalculation;
// 	// 							localDevice.Location = sharedDevice.Location;
// 	// 							localDevice.CtrlModel = sharedDevice.CtrlModel;
// 	// 							localDevice.Description = sharedDevice.Description;
// 	// 							localDevice.FacilityCode = sharedDevice.FacilityCode;
// 	// 							localDevice.FacilityId = sharedDevice.FacilityId;
// 	// 							localDevice.FloorId = sharedDevice.FloorId;
// 	// 							localDevice.LiveIconId = sharedDevice.LiveIconId;
// 	// 							localDevice.Location = sharedDevice.Location;
// 	// 							localDevice.LotCalculation = sharedDevice.LotCalculation;
// 	// 							localDevice.LotCapacity = sharedDevice.LotCapacity;
// 	// 							localDevice.HasTool = sharedDevice.HasTool;
// 	// 							localDevice.IsAuxiliar = sharedDevice.IsAuxiliar;
// 	// 							localDevice.OEEConfiguration = sharedDevice.OEEConfiguration;
// 	// 							localDevice.Skills = sharedDevice.Skills;
// 	// 							localDevice.OperationTypeCode = sharedDevice.OperationTypeCode;
// 	// 							localDevice.Programming = sharedDevice.Programming;

// 	// 							foreach (Sensor os in sharedDevice.Sensors)
// 	// 							{
// 	// 								Sensor localSensor = localDevice.Sensors.Find(x => x.Id == os.Id);
// 	// 								if (localSensor is null)
// 	// 								{
// 	// 									localDevice.Sensors.Add(os);
// 	// 								}
// 	// 								else
// 	// 								{
// 	// 									int index = localDevice.Sensors.IndexOf(localSensor);
// 	// 									if (index >= 0)
// 	// 									{
// 	// 										os.Value = localSensor.Value;
// 	// 										os.AvgValue = localSensor.AvgValue;
// 	// 										os.Data = localSensor.Data;
// 	// 										os.LastRead = localSensor.LastRead;
// 	// 										os.LastValue = localSensor.LastValue;
// 	// 										localDevice.Sensors[index] = os;
// 	// 									}
// 	// 								}
// 	// 							}

// 	// 							foreach (MachineParam op in sharedDevice.Parameters)
// 	// 							{
// 	// 								MachineParam localParam = localDevice.Parameters.Find(x => x.Id == op.Id);
// 	// 								if (localParam is null)
// 	// 								{
// 	// 									localDevice.Parameters.Add(localParam);
// 	// 								}
// 	// 								else
// 	// 								{
// 	// 									int index = localDevice.Parameters.IndexOf(localParam);
// 	// 									if (index >= 0)
// 	// 									{
// 	// 										op.Value = localParam.Value;
// 	// 										localDevice.Parameters[index] = op;
// 	// 									}
// 	// 								}
// 	// 							}
// 	// 						}
// 	// 					}
// 	// 				}).ConfigureAwait(false);
// 	// 			}
// 	// 		}
// 	// 		catch { }
// 	// 	}
// 	// }

// 	/// <summary>
// 	/// Get all values from the memory or the current machines.
// 	/// </summary>
// 	// public static async Task<ConcurrentDictionary<string, ConcurrentDictionary<string, object>>> GetAllValues()
// 	// {
// 	// 	if (ServiceManager.appSettings.GetAppSetting("MemoryMode").Contains("read", StringComparison.OrdinalIgnoreCase))
// 	// 	{
// 	// 		try
// 	// 		{
// 	// 			string base64 = GarnetConnectionClient.GetValue("MachineData");
// 	// 			byte[] data = base64 is not null ? Convert.FromBase64String(base64) : null;
// 	// 			string dataDecoded = Encoding.UTF8.GetString(data);
// 	// 			ConcurrentDictionary<string, ConcurrentDictionary<string, object>> tempValue = JsonSerializer.Deserialize<ConcurrentDictionary<string, ConcurrentDictionary<string, object>>>(dataDecoded);

// 	// 			if (tempValue?.IsEmpty == false)
// 	// 			{
// 	// 				MachineValues = tempValue;
// 	// 			}
// 	// 		}
// 	// 		catch { }
// 	// 	}
// 	// 	else
// 	// 	{
// 	// 		foreach (string m in MachineValues.Keys.ToArray())
// 	// 		{
// 	// 			Machine machine = Machines?.Find(x => x.Id == m);
// 	// 			if (machine is not null)
// 	// 			{
// 	// 				await LoadMachineContext(machine).ConfigureAwait(false);
// 	// 				await GetMachineValues(machine.Id, true).ConfigureAwait(false);
// 	// 			}
// 	// 		}
// 	// 	}
// 	// 	return MachineValues;
// 	// }

// 	#endregion Public Methods

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public static async Task RefreshOEEAsync(string MachineId = "")
// 	{
// 		if (!string.IsNullOrEmpty(MachineId))
// 		{
// 			Machine machine = Machines.Find(x => string.Equals(x.Id, MachineId, StringComparison.OrdinalIgnoreCase));
// 			if (machine is not null)
// 			{
// 				try
// 				{
// 					ExpressionResolver resolver = (ExpressionResolver)machine.Tag;
// 					await resolver.CalculateDeviceOEEAsync().ConfigureAwait(false);
// 				}
// 				catch
// 				{
// 				}
// 			}
// 		}
// 		else
// 		{
// 			foreach (Machine machine in Machines)
// 			{
// 				try
// 				{
// 					ExpressionResolver resolver = (ExpressionResolver)machine.Tag;
// 					await resolver.CalculateDeviceOEEAsync().ConfigureAwait(false);
// 				}
// 				catch { }
// 			}
// 		}
// 	}

// 	#region Private Methods

// 	internal static void ForceReportProduction(string machineId, string processId, string workOrderId, double quantity)
// 	{
// 		onMachineReportProduction?.Invoke(null, new MachineProducedNotification
// 		{
// 			MachineId = machineId,
// 			ProcessId = processId,
// 			WorkOrderId = workOrderId,
// 			Quantity = quantity
// 		});
// 	}

// 	private static async Task GetActiveDevicesAsync()
// 	{
// 		try
// 		{
// 			MachineValues = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
// 			MachineTimestamps = new ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>>();
// 			Machines = [.. BusinessProcess.ListDevices(false, false, false, null, false).Where(x => x.Status == Status.Active)];
// 			// if (ServiceManager.appSettings.GetAppSetting("MemoryMode").Contains("read", StringComparison.OrdinalIgnoreCase))
// 			// {
// 			// 	string json = JsonSerializer.Serialize(Machines);
// 			// 	string base64 = Convert.ToBase64String(json.ToBytes());
// 			// 	GarnetConnectionClient.SetValue("CurrentMachines", base64);
// 			// }
// 			for (int i = 0; i < Machines.Count; i++)
// 			{
// 				Machine m = Machines[i];

// 				if (!MachineValues.ContainsKey(m.Id))
// 				{
// 					_ = MachineValues.AddOrUpdate(m.Id, new ConcurrentDictionary<string, object>(), (_, _) => new ConcurrentDictionary<string, object>());
// 				}
// 				if (!MachineTimestamps.ContainsKey(m.Id))
// 				{
// 					_ = MachineTimestamps.AddOrUpdate(m.Id, new ConcurrentDictionary<string, DateTime>(), (_, _) => new ConcurrentDictionary<string, DateTime>());
// 				}

// 				ExpressionResolver resolver = new(m, BusinessProcess);
// 				resolver.OnResolve += Resolver_OnResolve_Handler;
// 				resolver.OnLog += Resolver_OnLog;
// 				resolver.onMachineProduced += Resolver_onMachineProduced;
// 				m.Tag = resolver;
// 				m.Sensors = m.Sensors?.Where(x => x.Status == Status.Active).ToList();
// 				m.Parameters = m.Parameters?.Where(x => x.Status == Status.Active).ToList();
// 				try
// 				{
// 					BusinessProcess.SetMachineSyncConfig(m);
// 				}
// 				catch (Exception ex)
// 				{
// 					m.ConfigError = ex.Message;
// 					if (!MachineValues[m.Id].ContainsKey("@ConfigError"))
// 					{
// 						_ = MachineValues[m.Id].AddOrUpdate("@ConfigError", m.ConfigError, (_, _) => m.ConfigError);
// 					}
// 					else
// 					{
// 						MachineValues[m.Id]["@ConfigError"] = m.ConfigError;
// 					}
// 				}
// 				// if (ServiceManager.appSettings.GetAppSetting("MemoryMode").Contains("read", StringComparison.OrdinalIgnoreCase))
// 				// {
// 				// 	string json = JsonSerializer.Serialize(Machines);
// 				// 	string base64 = Convert.ToBase64String(json.ToBytes());
// 				// 	GarnetConnectionClient.SetValue("CurrentMachines", base64);
// 				// }
// 				try
// 				{
// 					await ProcessMachine(m.Id, null, null, true).ConfigureAwait(false);
// 					await resolver.CalculateDeviceOEEAsync().ConfigureAwait(false);
// 					await LoadMachineContext(m).ConfigureAwait(false);
// 					Dictionary<string, object> tempValues = await GetMachineValues(m.Id, true).ConfigureAwait(false);
// 					if (tempValues.TryGetValue("IO", out object value))
// 					{
// 						MachineValues[m.Id]["IO"] = value;
// 					}
// 				}
// 				catch (Exception ex1)
// 				{
// 					logger.Error("Error in Machine Initialization in GetActiveDevices() - " + ex1.Message + "StackTrace:" + ex1.StackTrace, true);
// 				}
// 			}
// 		}
// 		catch (Exception ex)
// 		{
// 			logger.Error("Error in GetActiveDevices() - " + ex.Message + "StackTrace:" + ex.StackTrace, true);
// 		}
// 	}

// 	/// <summary>
// 	/// Process messages from the queue.
// 	/// </summary>
// 	// public static async Task ProcessMessages()
// 	// {
// 	// 	foreach (MsTeamsNotification message in messageQueue.GetConsumingEnumerable())
// 	// 	{
// 	// 		await Operations.SendMessageMSteamsAsync(message).ConfigureAwait(false);
// 	// 		await Task.Delay(delayBetweenMessages).ConfigureAwait(false); // Control the rate of sending
// 	// 	}
// 	// }

// 	private static void Resolver_OnLog(object sender, CustomActionEventArgs<string, bool> e)
// 	{
// 		logger.Debug(e.Request, true);
// 	}

// 	private static void Resolver_onMachineProduced(object sender, CustomActionEventArgs<MachineProducedNotification, bool> e)
// 	{
// 		onMachineReportProduction?.Invoke(null, e.Request);
// 	}

// 	private static async Task LoadMachineContext(Machine machine)
// 	{
// 		using MachineEnvironment tempResult = await BusinessProcess.GetMachineEnvironment(machine).ConfigureAwait(false);
// 		machine.Environment.IsOutput = tempResult.IsOutput;
// 		machine.Environment.Output = tempResult.Output;
// 		machine.Environment.WorkOrderId = tempResult.WorkOrderId;
// 		machine.Environment.WorkOrderNo = tempResult.WorkOrderNo;
// 		machine.Environment.PerformanceFactor = tempResult.PerformanceFactor;
// 		machine.Environment.Received = tempResult.Received;
// 		machine.Environment.Total = tempResult.Total;
// 		machine.Environment.Modifiers = tempResult.Modifiers;
// 		machine.Environment.Rejected = tempResult.Rejected;
// 		machine.Environment.Product = tempResult.Product;
// 		machine.Environment.StartDate = tempResult.StartDate;
// 		machine.Environment.ProcessId = tempResult.ProcessId;
// 		machine.Environment.ProcessPerformanceFactor = tempResult.ProcessPerformanceFactor;
// 		machine.Environment.ProcessReceived = tempResult.ProcessReceived;
// 		machine.Environment.ProcessStartDate = tempResult.ProcessStartDate;
// 		machine.Environment.ProcessTotal = tempResult.ProcessTotal;
// 		machine.Environment.DowntimeDate = tempResult.DowntimeDate;
// 		machine.Environment.DowntimeId = tempResult.DowntimeId;
// 		machine.Environment.IsCycle = "false";
// 	}

// 	/// <summary>
// 	/// Set the sensor notifications.
// 	/// </summary>
// 	public static async Task SetSensorNotificationsThen(string machineId, string code, Sensor lectorSensor, SensorData SensorRead, SensorWhen sensorWhen, int index = 0)
// 	{
// 		try
// 		{
// 			if (sensorWhen.SensorsThen.Count > 0)
// 			{
// 				string guid = Guid.NewGuid().ToStr();
// 				MessageNotificationRequest messageRequest = null;
// 				Machine machine = Machines?.Find(x => x.Id == machineId);
// 				List<RecipientNotification> Recipients = RecipientsMessage(
// 					sensorWhen.SensorsThen[index].Type,
// 					sensorWhen.SensorsThen[index].SensorsRecipients
// 				);
// 				if (Recipients is null && machine is not null && lectorSensor is not null)
// 				{
// 					logger.Error($"There are no users configured to send the sensor notification. Machine: {machine.Code} Sensor: {lectorSensor.Code}-{lectorSensor.Description} ");
// 				}

// 				// foreach (RecipientNotification recipient in Recipients)
// 				// {
// 				// 	//Se genera el registro con el proceso que detona la notificación y que será leida posteriormente para crear el mensaje
// 				// 	//sustituyendo los valores por los campos dinámicamente
// 				// 	NotificationsTemplates template = await BusinessProcess.GetTemplate(sensorWhen.SensorsThen[index].TemplateId).ConfigureAwait(false);
// 				// 	if (template is not null)
// 				// 	{
// 				// 		messageRequest = new MessageNotificationRequest
// 				// 		{
// 				// 			Placeholders = [
// 				// 			   new() { Code = "sensor_name", Value = $"{lectorSensor.Code}-{lectorSensor.Description}" },
// 				// 				   new() { Code = "machine_name", Value = $"{machine.Code}-{machine.Description}" },
// 				// 				   new() { Code = "sensor_reading", Value = SensorRead.Value },
// 				// 				   new() { Code = "user_name", Value = string.IsNullOrEmpty(recipient.displayName) ? "User" : recipient.displayName },
// 				// 				   new() { Code = "event_date", Value = DateTime.UtcNow.ToString() },
// 				// 				   new() { Code = "event_hour", Value = DateTime.UtcNow.Hour.ToString() },
// 				// 				   new() { Code = "event_type", Value = "Unknown" },
// 				// 				   new() { Code = "name_explode", Value = index > 0 ? sensorWhen.SensorsThen[index - 1].SensorsRecipients[0].Value : "System Automatic" }
// 				// 			],
// 				// 			ToEmp = [recipient.employeeId],
// 				// 			TemplateCode = template.Code,
// 				// 			TargetType = sensorWhen.SensorsThen[index].Action,
// 				// 			Subject = template.Subject,
// 				// 			KeyConfirm = guid,
// 				// 			RequiresConfirm = sensorWhen.SensorsThen[index].RequiresConfirm.ToBool(),
// 				// 			Priority = template.Priority,
// 				// 		};
// 				// 		if (messageRequest.TargetType == "MsTeams")
// 				// 		{
// 				// 			foreach (MessageNotification notify in await BusinessProcess.CreateMessageNotification(messageRequest).ConfigureAwait(false))
// 				// 			{
// 				// 				messageQueue.Add(new MsTeamsNotification
// 				// 				{
// 				// 					Message = notify.Message,
// 				// 					RequiresConfirm = notify.RequiresConfirm,
// 				// 					Subject = notify.Subject,
// 				// 					Disabled = false,
// 				// 					ProcessId = notify.keyConfirm,
// 				// 					To = notify.To
// 				// 				});
// 				// 			}
// 				// 		}
// 				// 		else
// 				// 		{
// 				// 			_ = Task.Run(async () =>
// 				// 			{
// 				// 				ResponseModel response = await BusinessProcess.SendMessageNotification(messageRequest, recipient, null, "System Automatic").ConfigureAwait(false);
// 				// 				if (!response.IsSuccess)
// 				// 				{
// 				// 					logger.Error(response.Message);
// 				// 				}
// 				// 			});
// 				// 		}
// 				// 	}
// 				// 	else
// 				// 	{
// 				// 		logger.Error("Template " + sensorWhen.SensorsThen[index].TemplateId + " doesn't exist");
// 				// 	}
// 				// }
// 			}
// 			// if (sensorWhen.SensorsThen[index].RequiresConfirm == 1 && sensorWhen.SensorsThen.Count > index + 1)
// 			// {
// 			// 	SensorMonitoringService.SetSensorThen(
// 			// 		code,
// 			// 		machineId,
// 			// 		lectorSensor,
// 			// 		SensorRead,
// 			// 		sensorWhen,
// 			// 		sensorWhen.SensorsThen[index + 1],
// 			// 		sensorWhen.SensorsThen[index + 1].IdleTimeout.ToInt32()
// 			// 	);
// 			// }
// 		}
// 		catch (Exception ex)
// 		{
// 			logger.Error(ex);
// 		}
// 	}

// 	private static List<RecipientNotification> RecipientsMessage(int type, List<SensorRecipient> sensorRecipients)
// 	{
// 		List<RecipientNotification> recipients = BusinessProcess.GetRecipientsNotification(type);

// 		Func<RecipientNotification, string> getId = type switch
// 		{
// 			1 => x => x.userId.ToStr(),
// 			2 => x => x.profileId.ToStr(),
// 			3 => x => x.employeeId.ToStr(),
// 			_ => null // type 0 or unsupported
// 		};

// 		if (getId is null)
// 			return [];

// 		return [.. from sr in sensorRecipients
// 			join r in recipients on sr.Value equals getId(r)
// 			select r];
// 	}

// 	/// <summary>
// 	/// Set the sensor live viewer properties.
// 	/// </summary>
// 	public static void SetSensorLiveViewer(string machineId, string code, SensorLiveViewer sensorLiveViewer)
// 	{
// 		MachineValues[machineId][code + "BkColor"] = sensorLiveViewer.Color;
// 		MachineValues[machineId][code + "Flicker"] = sensorLiveViewer.Flicker;
// 		OnMachineProcessed(null, machineId);
// 	}

// 	internal static string SetPrintQueue(string machineId, string processId, string workOrderId, string LogDate) => BusinessProcess.SetPrintQueue(machineId, processId, workOrderId, LogDate);

// 	//internal static string SetManualPrintQueue(PrintRequest request, int IdUser) => BusinessProcess.SetManualPrintQueue(request, IdUser);

// 	private static void Resolver_OnResolve_Handler(object sender, CustomActionEventArgs<ResolvedMachine, bool> e)
// 	{
// 		_ = Resolver_OnResolve(e).ContinueWith(static x =>
// 		{
// 			if (x.Exception is not null)
// 			{
// 				// Log or handle the exception
// 				Console.WriteLine(x.Exception);
// 			}
// 		}, TaskScheduler.Default);
// 	}

// 	private static async Task Resolver_OnResolve(CustomActionEventArgs<ResolvedMachine, bool> e)
// 	{
// 		string machineId = e.Request.Machine.Id;

// 		foreach (MachineParam param in e.Request.Machine.Parameters.Where(x => x.Status == Status.Active).ToList())
// 		{
// 			if (!string.IsNullOrEmpty(param.Value.ToStr()))
// 			{
// 				bool tryInsert = true;
// 				if (e.Request.IgnoreInsert)
// 				{
// 					tryInsert = false;
// 				}
// 				string paramValue = param.Value.ToStr();
// 				string paramCode = param.Code;
// 				try
// 				{
// 					if (MachineValues[machineId].ContainsKey(param.Code))
// 					{
// 						if (MachineValues[machineId][paramCode].ToStr() == paramValue && (DateTime.UtcNow - MachineTimestamps[machineId][paramCode]).TotalMinutes < 1)
// 						{
// 							tryInsert = false;
// 						}
// 						else
// 						{
// 							MachineTimestamps[machineId][paramCode] = DateTime.UtcNow;
// 						}

// 						MachineValues[machineId][paramCode] = paramValue;
// 					}
// 					else
// 					{
// 						MachineValues[machineId].AddOrUpdate(paramCode, paramValue, (_, _) => paramValue);
// 						if (!MachineTimestamps[machineId].ContainsKey(paramCode))
// 						{
// 							MachineTimestamps[machineId].AddOrUpdate(paramCode, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
// 						}
// 						else
// 						{
// 							MachineTimestamps[machineId][paramCode] = DateTime.UtcNow;
// 						}
// 					}
// 				}
// 				catch { }

// 				if (param.StoreValue && !string.IsNullOrEmpty(param.Signature) && param.Signature != "-" && tryInsert)
// 				{
// 					BusinessProcess.PutParameterData(new SensorData
// 					{
// 						SensorId = param.Id,
// 						ValueDate = DateTime.UtcNow,
// 						Name = paramCode,
// 						Value = paramValue
// 					});
// 				}
// 				else
// 				{
// 					BusinessProcess.SetLastValue(new SensorData
// 					{
// 						SensorId = param.Id,
// 						Value = paramValue
// 					});
// 				}
// 			}
// 		}

// 		if (e.Request.Machine is null || (e.Request.Machine?.OEEConfiguration is not null && e.Request.Machine?.OEEConfiguration.AvailabilityMode != OEEMode.AutomaticSwitch))
// 		{
// 			if (MachineValues[machineId].ContainsKey("IO"))
// 			{
// 				MachineValues[machineId]["IO"] = 1;
// 			}
// 			else
// 			{
// 				MachineValues[machineId].AddOrUpdate("IO", 1, (_, _) => 1);
// 			}
// 		}
// 		Machine tmpMachineResolved = Machines.Find(x => x.Id == machineId);
// 		if (tmpMachineResolved is not null)
// 		{
// 			tmpMachineResolved.IsBusy = false;
// 		}

// 		if (UpdatedMachines.TryGetValue(machineId, out Machine value))
// 		{
// 			Machine current = Machines.Find(x => x.Id == machineId);
// 			if (current is not null && value is not null)
// 			{
// 				ExpressionResolver resolver = (ExpressionResolver)current.Tag;
// 				resolver.SetMachine(value);
// 				value.Tag = resolver;
// 				Machines.Remove(current);
// 				Machines.Add(value);
// 				UpdatedMachines.Remove(machineId);
// 				// if (ServiceManager.appSettings.GetAppSetting("MemoryMode").Contains("read", StringComparison.OrdinalIgnoreCase))
// 				// {
// 				// 	string json = JsonSerializer.Serialize(Machines);
// 				// 	string base64 = Convert.ToBase64String(json.ToBytes());
// 				// 	GarnetConnectionClient.SetValue("CurrentMachines", base64);
// 				// }
// 			}
// 		}
// 		OnMachineProcessed?.Invoke(e.Request.IgnoreInsert, machineId);
// 	}

// 	#endregion Private Methods
// }
