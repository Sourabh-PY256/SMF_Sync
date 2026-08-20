// using System.Collections.Concurrent;
// using System.Data;
// using System.Diagnostics.CodeAnalysis;
// using System.Globalization;
// using System.Reflection;

// using EWP.SF.Common.Models;
// using EWP.SF.KafkaSync.BusinessEntities;
// using EWP.SF.KafkaSync.BusinessEntities.Kafka;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Text.Json;
// using System.Threading.Tasks;

// namespace EWP.SF.KafkaSync.BusinessLayer

// public static class ServiceManager
// {
// 	private static readonly Logger logger = LogManager.GetCurrentClassLogger();
// 	private static Action<Message> PostMessage;
// 	private static Action<IntegrationMessage> IntegrateMessage;
// 	private static Action<IntegrationMessage> DebugMessage;
// 	private static Operations BLL;
// 	public static ApplicationSettings appSettings;
// 	private static SessionManager SessionManager;
// 	public static ValidationResult SystemStatusCode;

// 	public static object SystemStatus =>
// #if DEBUG
// 		new { Status = "Valid", InstallationId = AppValidator.InstallId, Values = AppValidator.ValidatorResult };
// #else
// 		new { Status = SystemStatusCode.ToStr(), InstallationId = AppValidator.InstallId, Values = AppValidator.ValidatorResult };
// #endif

// 	public static async Task InitializeAsync(
// 		ApplicationSettings settings,
// 		Action<Message> publisher,
// 		Action<IntegrationMessage> integrator,
// 		Action<IntegrationMessage> debugger)
// 	{
// 		appSettings = settings ?? throw new ArgumentNullException(nameof(settings));
// 		PostMessage = publisher;
// 		IntegrateMessage = integrator;
// 		if (appSettings.ContainsKey("EnableDebugSSE") && appSettings.GetAppSetting("EnableDebugSSE").Equals("true", StringComparison.OrdinalIgnoreCase))
// 		{
// 			DebugMessage = debugger;
// 		}

// 		// Initialize synchronous components (no parallelization required here)
// 		BLL = new Operations();
// 		SessionManager = new SessionManager();
// 		// Define parallel tasks
// 		List<Task> tasks =
// 		[
// 			Task.Run(() =>
// 				{
// 					DeviceMonitorService.Initialize();
// 					DeviceMonitorService.onDeviceDown += DeviceMonitorService_onDeviceDown;
// 				}),

// 				Task.Run(() =>
// 				{
// 					SensorMonitoringService.Initialize();
// 					SensorMonitoringService.onSensorLiveViewer += SensorMonitorService_onSensorLiveViewer;
// 					SensorMonitoringService.onSensorWhen += SensorMonitorService_onSensorWhen;
// 					SensorMonitoringService.onSensorThen += SensorMonitorService_onSensorThen;
// 				}),

// 				Task.Run(async () =>
// 				{
// 					await SyncService.Initialize().ConfigureAwait(false);
// 					SyncService.OnMachineProcessed += SyncService_MachineProcessed_Handler;
// 					SyncService.OnPrintLabel += SyncInitializer_onPrintLabel_Handler;
// 					SyncService.onMachineReportProduction += SyncService_MachineReportProduction;
// 					_ = SyncService.ProcessMessages().ConfigureAwait(false);
// 				}),

// 				Task.Run(() =>
// 				{
// 					SyncInitializer.Initialize(InitType.Manual);
// 					SyncInitializer.onAlertReceived += SyncInitializer_onAlertReceived_Handler;
// 				}),

// 				  Task.Run(() =>
// 				  {
// 					int deviceInterval = appSettings.GetAppSetting("DeviceBusInterval").ToInt32(1);
// 					int brokerInterval = appSettings.GetAppSetting("BrokerBusInterval").ToInt32(2);
// 					int brokerBusInterval = appSettings.GetAppSetting("BrokerDataBusInterval").ToInt32(1);
// 					DataPickupService.Initialize();
// 					DataPickupService.onDataPickup += DataPickupService_OnDataPickup_Handler;
// 					_ = DataPickupService.AddSource(PickupEvent.BrokerDataProcessed,brokerInterval);
// 					_ = DataPickupService.AddSource(PickupEvent.BrokerData,brokerBusInterval);
// 					_ = DataPickupService.AddSource(PickupEvent.MachineProcessed, deviceInterval, true, (collection,obj) => {
// 						int index =  collection.Select((item, idx) => new { Item = item, Index = idx } ).FirstOrDefault(itm => itm.Item.MachineId == obj.MachineId)?.Index ?? -1;
// 						if(index >= 0)
// 						{
// 							DataPickupService.GetPick(PickupEvent.MachineProcessed)[index] = obj;
// 						}else
// 						{
// 						   DataPickupService.GetPick(PickupEvent.MachineProcessed).Add((object)obj);
// 						}
// 						return false;
// 					});
// 				})
// 		];

// 		// Await the completion of all tasks
// 		await Task.WhenAll(tasks).ConfigureAwait(false);

// 		// Initialize the ContextCache event after all the others are done
// 		ContextCache.OnDelete += ContextCache_OnDelete;

// 		// Validate after all initializations are complete
// 		await Validate().ConfigureAwait(false);
// 	}

// 	private static void DataPickupService_OnDataPickup_Handler(object sender, string e)
// 	{
// 		_ = DataPickupService_OnDataPickup(sender, e).ContinueWith(x =>
// 		{
// 			if (x.Exception is not null)
// 			{
// 				// Log or handle the exception
// 				Console.WriteLine(x.Exception);
// 			}
// 		}, TaskScheduler.Default);
// 	}

// 	private static async Task DataPickupService_OnDataPickup(object sender, string eventId)
// 	{
// 		if (eventId == PickupEvent.MachineProcessed)
// 		{
// 			DebugMessage?.Invoke(new IntegrationMessage { Code = "Output Values", Value = sender });
// 			PostMessage(new Message
// 			{
// 				Type = Convert.ToInt32(MessageBrokerType.SensorData, CultureInfo.InvariantCulture),
// 				Value = sender
// 			});
// 		}
// 		else if (eventId == PickupEvent.BrokerDataProcessed)
// 		{
// 			// Cae info al bus
// 			DebugMessage?.Invoke(new IntegrationMessage { Code = "Inserting Broker Data to DB" });
// 			BLL.BulkSensorDataRaw(JsonConvert.SerializeObject(sender));
// 		}
// 		else if (eventId == PickupEvent.BrokerData)
// 		{
// 			List<DataCollector> dcd = JsonConvert.DeserializeObject<List<DataCollector>>(JsonConvert.SerializeObject(sender));
// 			string[] machines = [.. dcd.Select(x => x.Machine)];
// 			await SyncService.MergeMachinesFromMemory(machines).ConfigureAwait(false);
// 			_ = await Operations.BulkSensorData(dcd, new User(-1), DebugMessage).ConfigureAwait(false);
// 		}
// 	}

// 	private static void ContextCache_OnDelete(object sender, System.Runtime.Caching.CacheItem e)
// 	{
// 		try
// 		{
// 			RequestContext context = (RequestContext)e.Value;
// 			if (context.ExpirationDate >= DateTime.UtcNow)
// 			{
// 				if (context.User.Status != Status.Deleted)
// 				{
// 					_ = SessionManager.RestoreToken(context);
// 				}
// 			}
// 			else
// 			{
// 				NotifySessionCheck(context.User.Id);
// 			}
// 		}
// 		catch
// 		{
// 		}
// 	}

// 	public static void NotifySessionCheck(int UserId)
// 	{
// 		PostMessage(new Message
// 		{
// 			Type = 5,
// 			Value = new { Type = "U", Id = UserId }
// 		});
// 	}

// 	public static async Task<bool> UpdateValidation(string licVal)
// 	{
// 		string key = "LastValidateExec";
// 		bool returnValue = false;
// 		if (!string.IsNullOrEmpty(licVal))
// 		{
// 			if (await Validate(licVal).ConfigureAwait(false) == ValidationResult.Valid)
// 			{
// 				Config.Configuration.UpdateConfiguration("PrivateSettings", licVal);
// 				await Validate().ConfigureAwait(false);
// 				returnValue = true;
// 			}
// 			if (CacheManager.Cache.Get(key) is not null)
// 			{
// 				CacheManager.Cache.Remove(key);
// 			}
// 		}
// 		else
// 		{
// 			if (CacheManager.Cache.Get(key) is null)
// 			{
// 				await Validate().ConfigureAwait(false);
// 				returnValue = SystemStatusCode == ValidationResult.Valid;
// #if DEBUG
// 				returnValue = true;
// #endif
// 				CacheManager.Cache.Set(key, returnValue, DateTime.Now.AddHours(6));
// 			}
// 			else
// 			{
// 				returnValue = CacheManager.Cache.Get(key).ToBool();
// 			}
// 		}
// 		return returnValue;
// 	}

// 	public static async Task<ValidationResult> Validate(string externalSetting = "")
// 	{
// 		string settings = string.Empty;
// 		string installId = string.Empty;
// 		if (Config.Configuration.TryGetValue("PrivateSettings", out string valuePrivate))
// 		{
// 			settings = valuePrivate;
// 		}
// 		if (Config.Configuration.TryGetValue("SystemInstanceId", out string valueInstance))
// 		{
// 			installId = valueInstance;
// 		}
// 		if (string.IsNullOrEmpty(externalSetting))
// 		{
// 			SystemStatusCode = await AppValidator.Validate(settings, installId).ConfigureAwait(false);
// 			if (string.IsNullOrEmpty(installId) && !string.IsNullOrEmpty(AppValidator.InstallId))
// 			{
// 				Config.Configuration.UpdateConfiguration("SystemInstanceId", AppValidator.InstallId);
// 			}
// #if DEBUG
// 			return ValidationResult.Valid;
// #else
// 			return SystemStatusCode;
// #endif
// 		}
// 		else
// 		{
// 			return await AppValidator.Validate(externalSetting, installId).ConfigureAwait(false);
// 		}
// 	}

// 	private static void DeviceMonitorService_onDeviceDown(object sender, string e)
// 	{
// 		_ = SyncService.SetDeviceDown(e);
// 	}

// 	private static void SensorMonitorService_onSensorLiveViewer(object sender, string e)
// 	{
// 		SensorThread thread = (SensorThread)sender;
// 		SyncService.SetSensorLiveViewer(thread.machineId, e, thread.currentLiveViewer);
// 	}

// 	private static async void SensorMonitorService_onSensorWhen(object sender, string e)
// 	{
// 		SensorWhenThread thread = (SensorWhenThread)sender;
// 		if (thread is not null)
// 		{
// 			await SyncService.SetSensorNotificationsThen(thread.machineId, e, thread.sensor, thread.lectorSensor, thread.currentSensorWhen).ConfigureAwait(false);
// 		}
// 	}

// 	private static async void SensorMonitorService_onSensorThen(object sender, string e)
// 	{
// 		SensorThenThread thread = (SensorThenThread)sender;
// 		if (thread is not null)
// 		{
// 			await SyncService.SetSensorNotificationsThen(thread.machineId, e, thread.sensor, thread.lectorSensor, thread.currentSensorWhen, thread.currentSensorThen.Order).ConfigureAwait(false);
// 		}
// 	}

// 	private static void SyncService_MachineReportProduction(object sender, MachineProducedNotification e)
// 	{
// 		PostMessage(new Message
// 		{
// 			Type = 9,
// 			Value = e
// 		});
// 	}

// 	private static void SyncInitializer_onAlertReceived_Handler(object sender, MessageBroker e)
// 	{
// 		_ = SyncInitializer_onAlertReceived(e).ContinueWith(x =>
// 		{
// 			if (x.Exception is not null)
// 			{
// 				// Log or handle the exception
// 				Console.WriteLine(x.Exception);
// 			}
// 		}, TaskScheduler.Default);
// 	}

// 	private static async Task SyncInitializer_onAlertReceived(MessageBroker e)
// 	{
// 		switch (e.Type)
// 		{
// 			case MessageBrokerType.Downtime:
// 				{
// 					ConcurrentDictionary<string, ConcurrentDictionary<string, object>> tempValues = await SyncService.GetAllValues().ConfigureAwait(false);
// 					if (tempValues.TryGetValue(e.MachineId, out ConcurrentDictionary<string, object> value) && value is not null)
// 					{
// 						if (!value.ContainsKey("IO"))
// 						{
// 							_ = value.AddOrUpdate("IO", 0, (_, _) => 0);
// 						}
// 						else
// 						{
// 							value["IO"] = 0;
// 						}

// 						if (!string.IsNullOrEmpty(e.Aux) && e.Aux == "1")
// 						{
// 							value["IO"] = 1;
// 						}
// 					}
// 					if (tempValues.TryGetValue(e.MachineId, out ConcurrentDictionary<string, object> valueTemp))
// 					{
// 						PostMessage(new Message
// 						{
// 							Type = Convert.ToInt32(e.Type, CultureInfo.InvariantCulture),
// 							MachineId = e.MachineId,
// 							Value = valueTemp["IO"]
// 						});
// 					}
// 					break;
// 				}
// 			case MessageBrokerType.SensorData:
// 				{
// 					int timeout = await SyncService.ProcessMachine(e.MachineId, e.ElementId, e.ElementValue, false, e.Aux).ConfigureAwait(false);
// 					if (timeout >= 0)
// 					{
// 						DeviceMonitorService.SetInitialize(e.MachineId, timeout);
// 					}
// 					break;
// 				}
// 			case MessageBrokerType.MachineUpdate:
// 				{
// 					if (string.IsNullOrEmpty(e.Aux))
// 					{
// 						if (!ContextCache.ProcessingDevices.Contains(e.MachineId))
// 						{
// 							ContextCache.ProcessingDevices.Add(e.MachineId);
// 							Machine device = await SyncService.UpdateMachine(e.MachineId).ConfigureAwait(false);
// 							if (!device.IsAuxiliar)
// 							{
// 								int? seconds = device.OEEConfiguration?.IdleSeconds.ToInt32();
// 								DeviceMonitorService.Set(e.MachineId, seconds);
// 							}
// 							_ = ContextCache.ProcessingDevices.Remove(e.MachineId);
// 						}
// 					}
// 					else if (e.Aux == "refresh")
// 					{
// 						Machine device = await SyncService.UpdateMachine(e.MachineId).ConfigureAwait(false);
// 					}
// 					break;
// 				}
// 			case MessageBrokerType.Alert:
// 				{
// 					PostMessage(new Message
// 					{
// 						Type = Convert.ToInt32(e.Type, CultureInfo.InvariantCulture),
// 						MachineId = e.MachineId,
// 						Value = e
// 					});
// 					break;
// 				}
// 			case MessageBrokerType.Permission:
// 				{
// 					string type = "A";
// 					if (!string.IsNullOrEmpty(e.Aux))
// 					{
// 						type = e.Aux.Substr(0, 1).ToUpperInvariant();
// 					}
// 					if (type == "R" || type == "U")
// 					{
// 						ContextCache.RemoveContext(type, e.ElementValue.ToInt32());
// 					}
// 					PostMessage(new Message
// 					{
// 						Type = Convert.ToInt32(e.Type, CultureInfo.InvariantCulture),
// 						Value = new { Type = type, Id = e.ElementValue },
// 					});
// 					break;
// 				}
// 			case MessageBrokerType.WorkOrder:
// 				{
// 					if (e.Aux != "S")
// 					{
// 						PostMessage(new Message
// 						{
// 							Type = Convert.ToInt32(e.Type, CultureInfo.InvariantCulture),
// 							Value = new { Type = e.Aux, Id = e.ElementValue, UID = e.ElementId }
// 						});
// 					}
// 					else
// 					{
// 						PostMessage(new Message
// 						{
// 							Type = Convert.ToInt32(e.Type, CultureInfo.InvariantCulture),
// 							Value = new { Type = e.Aux, Values = e.ElementId }
// 						});
// 					}
// 					break;
// 				}
// 			case MessageBrokerType.ProcessStatus:
// 				{
// 					string WorkOrderId = e.ElementId;
// 					string[] ProcessId = e.ElementValue.Split(',');
// 					string Status = e.Aux; // "2" disabled/new, 1 "Running" , "6" Finished

// 					try
// 					{
// 						await ActivityMonitorService.StartMonitorRoutinesInstances(Convert.ToInt32(Status, CultureInfo.InvariantCulture), WorkOrderId, ProcessId).ConfigureAwait(false);
// 					}
// 					catch
// 					{
// 					}
// 					PostMessage(new Message
// 					{
// 						Type = Convert.ToInt32(e.Type, CultureInfo.InvariantCulture),
// 						Value = new { WorkOrderId, Processes = ProcessId, Status = Status.ToInt32() }
// 					});
// 					break;
// 				}
// 			case MessageBrokerType.ActivitySchedule:
// 				break;
// 			case MessageBrokerType.ManualProductIsssue:
// 				{
// 					string WorkOrderId = e.ElementId;
// 					string ProcessId = e.ElementValue;
// 					string MachineId = e.MachineId;
// 					double Quantity = e.Aux.ToDouble();
// 					double Reject = e.Aux2.ToDouble();

// 					PostMessage(new Message
// 					{
// 						Type = 9,
// 						Value = new
// 						{
// 							WorkOrderId,
// 							ProcessId,
// 							MachineId,
// 							Quantity,
// 							Reject
// 						}
// 					});

// 					await SyncService.RefreshMachine(MachineId).ConfigureAwait(false);

// 					break;
// 				}
// 			case MessageBrokerType.ManualMaterialIssue:
// 				{
// 					string WorkOrderId = e.ElementId;
// 					string ProcessId = e.ElementValue;
// 					string MachineId = e.MachineId;
// 					string[] componentValue = e.Aux.Split('|');
// 					string ComponentId = componentValue[0];
// 					double quantity = componentValue[1].ToDouble();
// 					PostMessage(new Message
// 					{
// 						Type = Convert.ToInt32(e.Type, CultureInfo.InvariantCulture),
// 						Value = new { WorkOrderId, ProcessId, MachineId, ComponentId, Quantity = quantity },
// 					});
// 					break;
// 				}
// 			case MessageBrokerType.ActivityExecuted:
// 				{
// 					PostMessage(new Message
// 					{
// 						Type = Convert.ToInt32(e.Type, CultureInfo.InvariantCulture),
// 						Value = e.ElementId,
// 					});

// 					break;
// 				}
// 			case MessageBrokerType.TriggerPrint:
// 				{
// 					string LogDate = e.ElementId;
// 					string WorkOrderId = e.ElementValue;
// 					string MachineId = e.MachineId;
// 					string ProcessId = e.Aux;

// 					string PrinterTerminalResult = SyncService.SetPrintQueue(MachineId, ProcessId, WorkOrderId, LogDate);
// 					if (Config.Configuration["DisablePrintService"] != "true")
// 					{
// 						IntegrateMessage(new IntegrationMessage
// 						{
// 							Code = IntegrationConstants.EVENT_CODE_PRINT_LABEL,
// 							Value = new
// 							{
// 								PrinterTerminal = PrinterTerminalResult
// 							}
// 						});
// 					}
// 					break;
// 				}
// 			case MessageBrokerType.ExternalProductReceipt:
// 				{
// 					try
// 					{
// 						BMMReceiptResponse message = JsonConvert.DeserializeObject<BMMReceiptResponse>(e.ElementId);
// 						if (message is not null && !string.IsNullOrEmpty(message.ExternalOrderId))
// 						{
// 							IntegrateMessage(new IntegrationMessage
// 							{
// 								Code = IntegrationConstants.EVENT_CODE_RECEIVE_PRODUCT,
// 								Value = message
// 							});
// 						}
// 					}
// 					catch
// 					{
// 					}
// 					break;
// 				}
// 			case MessageBrokerType.ExternalMaterialIssue:
// 				{
// 					try
// 					{
// 						BMMIssueMaterialResponse message = BLL.GetIntegrationIssueDetails(e.ElementId);
// 						if (message is not null && !string.IsNullOrEmpty(message.ExternalOrderId))
// 						{
// 							IntegrateMessage(new IntegrationMessage
// 							{
// 								Code = IntegrationConstants.EVENT_CODE_ISSUE_MATERIAL,
// 								Value = message
// 							});
// 						}
// 					}
// 					catch
// 					{
// 					}
// 					break;
// 				}

// 			case MessageBrokerType.TagData:
// 				break;
// 			case MessageBrokerType.Email:
// 				break;
// 			case MessageBrokerType.CatalogChanged:
// 				break;
// 			case MessageBrokerType.SecondaryContrainstGroup:
// 				break;
// 		}
// 	}
// 	private static void SyncService_MachineProcessed_Handler(object sender, string e)
// 	{
// 		_ = SyncService_MachineProcessed(sender, e).ContinueWith(x =>
// 		{
// 			if (x.Exception is not null)
// 			{
// 				// Log or handle the exception
// 				Console.WriteLine(x.Exception);
// 			}
// 		}, TaskScheduler.Default);
// 	}

// 	private static async Task SyncService_MachineProcessed(object sender, string e)
// 	{
// 		if (sender is Exception exception)
// 		{
// 			Dictionary<string, string> error = new()
// 				{
// 					{ "@ConfigError", exception.Message }
// 				};
// 			DebugMessage?.Invoke(new IntegrationMessage { Code = "Machine config Error", Value = error });
// 			DataPickupService.AddPick(PickupEvent.MachineProcessed, new { MachineId = e, Value = error });
// 		}
// 		else
// 		{
// 			Dictionary<string, object> machineValues = await SyncService.GetMachineValues(e, sender.ToBool()).ConfigureAwait(false);
// 			string io = SyncService.GetMachineValue(e, "IO");
// 			bool assigned = DeviceMonitorService.IsDeviceAssigned(e);
// 			if (!machineValues.ContainsKey("IO") && !string.IsNullOrEmpty(io) && io.ToInt32().ToBool() != assigned)
// 			{
// 				machineValues.Add("IO", assigned ? "1" : "0");
// 			}
// 			foreach (string value in machineValues.Keys.Where(x => x.Contains("DateSendApi")))
// 			{
// 				machineValues[value] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
// 			}
// 			DebugMessage?.Invoke(new IntegrationMessage { Code = "Machine Processed successfully", Value = e });
// 			DataPickupService.AddPick(PickupEvent.MachineProcessed, new { MachineId = e, Value = machineValues });
// 		}
// 	}

// 	private static void SyncInitializer_onPrintLabel_Handler(object sender, string e)
// 	{
// 		_ = SyncInitializer_onPrintLabel(sender, e).ContinueWith(x =>
// 		{
// 			if (x.Exception is not null)
// 			{
// 				// Log or handle the exception
// 				Console.WriteLine(x.Exception);
// 			}
// 		}, TaskScheduler.Default);
// 	}

// 	private static async Task SyncInitializer_onPrintLabel(object sender, string e)
// 	{
// 		if (sender is Exception exception)
// 		{
// 			Dictionary<string, string> error = new()
// 				{
// 					{ "@ConfigError", exception.Message }
// 				};

// 			DataPickupService.AddPick(PickupEvent.MachineProcessed, new { MachineId = e, Value = error });
// 		}
// 		else
// 		{
// 			Dictionary<string, object> machineValues = await SyncService.GetMachineValues(e, sender.ToBool()).ConfigureAwait(false);
// 			string io = SyncService.GetMachineValue(e, "IO");
// 			bool assigned = DeviceMonitorService.IsDeviceAssigned(e);
// 			if (!machineValues.ContainsKey("IO") && !string.IsNullOrEmpty(io) && io.ToInt32().ToBool() != assigned)
// 			{
// 				machineValues.Add("IO", assigned ? "1" : "0");
// 			}
// 			DataPickupService.AddPick(PickupEvent.MachineProcessed, new { MachineId = e, Value = machineValues });
// 		}
// 	}

// 	internal static string SetPrintQueue(PrintRequest request, User systemOperator)
// 	{
// 		string returnValue = string.Empty;
// 		if (Config.Configuration["DisablePrintService"] != "true")
// 		{
// 			string IdTerminal = SyncService.SetManualPrintQueue(request, systemOperator.Id);
// 			if (request.IsPreview)
// 			{
// 				returnValue = IdTerminal;
// 			}
// 			else
// 			{
// 				IntegrateMessage(new IntegrationMessage
// 				{
// 					Code = IntegrationConstants.EVENT_CODE_PRINT_LABEL,
// 					Value = new
// 					{
// 						PrinterTerminal = IdTerminal
// 					}
// 				});
// 			}
// 		}
// 		return returnValue;
// 	}

// 	public static void NotifyServicesChanged(object response)
// 	{
// 		PostMessage(new Message
// 		{
// 			Type = 998,
// 			Value = response
// 		});
// 	}

// 	public static void Datasync_NotifyStart(this DataSyncService serviceData, string entityCode)
// 	{
// 		if (!string.Equals(serviceData.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
// 		{
// 			return;
// 		}

// 		PostMessage(new Message
// 		{
// 			Type = 999,
// 			Value = new { Event = serviceData.EntityId, Entity = entityCode, Status = 1 }
// 		});
// 	}

// 	public static void Datasync_NotifyStop(this DataSyncService serviceData, string entityCode, DateTime initDate)
// 	{
// 		if (!string.Equals(serviceData.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
// 		{
// 			return;
// 		}

// 		PostMessage(new Message
// 		{
// 			Type = 999,
// 			Value = new { Event = serviceData.EntityId, Entity = entityCode, Status = 0, LastExecDate = initDate }
// 		});
// 	}

// 	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
// 	public static void ApplyOffsetFromUTC(object src, double ctxOffset)
// 	{
// 		const int direction = -1;
// 		if (src is null)
// 		{
// 			return;
// 		}

// 		CultureInfo enUS = new("en-US");
// 		Type paramType = src.GetType();
// 		if (paramType.IsGenericType && (paramType.GetGenericTypeDefinition() == typeof(List<>)))
// 		{
// 			if (!paramType.FullName.Contains("KeyValuePair", StringComparison.InvariantCultureIgnoreCase))
// 			{
// 				foreach (object o in (IEnumerable<object>)src)
// 				{
// 					ApplyOffsetFromUTC(o, ctxOffset);
// 				}
// 			}
// 		}
// 		else if (string.IsNullOrEmpty(paramType.Namespace) || !paramType.Namespace.StartsWith("System", StringComparison.InvariantCultureIgnoreCase))
// 		{
// 			if (paramType?.Name == "ReportRequest")
// 			{
// 				ReportRequest reportRequest = (ReportRequest)src;
// 				reportRequest.Parameters?.ForEach(x =>
// 					{
// 						if (DateTime.TryParseExact(x.Value, "yyyy/MM/dd", enUS, DateTimeStyles.AdjustToUniversal, out DateTime tempValue))
// 						{
// 							tempValue = tempValue.AddMinutes(ctxOffset * direction.ToInt32());
// 							x.Value = tempValue.ToString("s");
// 						}
// 					});
// 			}
// 			else
// 			{
// 				foreach (PropertyInfo pi in paramType.GetProperties())
// 				{
// 					Type propType = pi.PropertyType;
// 					bool ignoreOffset = pi.GetCustomAttribute<OffsetIgnore>() is not null;
// 					try
// 					{
// 						if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
// 						{
// 							propType = Nullable.GetUnderlyingType(propType);
// 						}
// 					}
// 					catch { }

// 					if (propType.Name == "DateTime" && !ignoreOffset)
// 					{
// 						object value = pi.GetValue(src);
// 						if (value is not null)
// 						{
// 							DateTime currentValue = (DateTime)value;
// 							if (currentValue.Year >= 1900)
// 							{
// 								currentValue = currentValue.AddMinutes(ctxOffset * direction.ToInt32());
// 								pi.SetValue(src, currentValue);
// 							}
// 						}
// 					}
// 					if (propType.IsGenericType && (propType.GetGenericTypeDefinition() == typeof(List<>)))
// 					{
// 						object list = pi.GetValue(src);
// 						if (list is not null)
// 						{
// 							foreach (object o in (IEnumerable<object>)list)
// 							{
// 								ApplyOffsetFromUTC(o, ctxOffset);
// 							}
// 						}
// 					}
// 					else if (!propType.Namespace.StartsWith("System") && !propType.IsEnum)
// 					{
// 						ApplyOffsetFromUTC(pi.GetValue(src), ctxOffset);
// 					}
// 				}
// 			}
// 		}
// 	}

// 	public static bool SendMessage(int type, object value, double offset = 0)
// 	{
// 		if (Math.Abs(offset) > 1e-10)
// 		{
// 			ApplyOffsetFromUTC(value, offset);
// 		}
// 		PostMessage(new Message
// 		{
// 			Type = type,
// 			Value = value
// 		});
// 		return true;
// 	}

// 	public static bool SendMessage(MessageBrokerType type, object value, double offset = 0)
// 	{
// 		if (Math.Abs(offset) > 1e-10)
// 		{
// 			ApplyOffsetFromUTC(value, offset);
// 		}
// 		PostMessage(new Message
// 		{
// 			Type = type.ToInt32(),
// 			Value = value
// 		});
// 		return true;
// 	}
// }
