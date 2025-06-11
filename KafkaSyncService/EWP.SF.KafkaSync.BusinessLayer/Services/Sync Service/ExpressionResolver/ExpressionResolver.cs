// using System.Globalization;
// using System.Text.RegularExpressions;

// using DynamicExpresso;
// using EWP.SF.Common.Enumerators;
// using EWP.SF.Common.Models;
// using EWP.SF.Common.ResponseModels;
// using EWP.SF.Common.Models.Sensors;
// using EWP.SF.Common.EventHandlers;
// using EWP.SF.Common.Models.Operations;

// using EWP.SF.Helper;

// using Newtonsoft.Json;

// using NLog;
// namespace EWP.SF.KafkaSync.BusinessLayer;

// /// <summary>
// ///
// /// </summary>
// public class FormulaParam
// {
// 	/// <summary>
// 	///
// 	/// </summary>
// 	public string Name { get; set; }

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public string Type { get; set; }

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public string Property { get; set; }

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public string String { get; set; }
// }

// public partial class ExpressionResolver : IDisposable
// {
// 	/// <summary>
// 	///
// 	/// </summary>
// 	public Dictionary<string, int> ErrorCount;
// 	private static readonly Logger logger = LogManager.GetCurrentClassLogger();
// 	private Interpreter _interpreter;
// 	private readonly Operations BLL;

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public event CustomActionHandler<MachineProducedNotification, bool> onMachineProduced;

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public event CustomActionHandler<ResolvedMachine, bool> OnResolve;

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public event CustomActionHandler<string, bool> OnLog;

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public Machine Machine { get; private set; }

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public ExpressionResolver(Machine machine, Operations operationsClass)
// 	{
// 		Machine = machine;
// 		ErrorCount = [];
// 		if (operationsClass is null)
// 		{
// 			BLL = new Operations();
// 		}
// 		else
// 		{
// 			BLL = operationsClass;
// 		}
// 	}

// 	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
// 	public void Dispose()
// 	{
// 		Dispose(true);
// 		GC.SuppressFinalize(this);
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	protected virtual void Dispose(bool disposing)
// 	{
// 		if (disposing)
// 		{
// 			// Free any other managed objects here.
// 			BLL?.Dispose();
// 		}

// 		// Free any unmanaged objects here.
// 		//
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public void SetMachine(Machine m)
// 	{
// 		Machine = m;
// 	}

// 	private void Log(string message)
// 	{
// 		if (OnLog is null)
// 		{
// 			return;
// 		}

// 		OnLog(this, new CustomActionEventArgs<string, bool>
// 		{
// 			Request = message
// 		});
// 	}

// 	private List<FormulaParam> ParseFormula(string formula)
// 	{
// 		List<FormulaParam> returnValue = null;
// 		Regex expression = MyRegex();
// 		foreach (Match match in expression.Matches(formula))
// 		{
// 			FormulaParam param = new();
// 			int count = 0;
// 			foreach (object group in match.Groups)
// 			{
// 				if (count == 0)
// 				{
// 					param.String = group.ToString();
// 				}
// 				if (count == 1)
// 				{
// 					param.Name = group.ToString();
// 				}
// 				if (count > 1)
// 				{
// 					param.Property = group.ToString();
// 				}
// 				count++;
// 			}
// 			returnValue ??= [];
// 			if (!returnValue.Any(x => x.String == param.String))
// 			{
// 				bool isSensor = Machine.Sensors.Any(x => x.Status == Status.Active && x.Code == param.Name);
// 				bool isParameter = Machine.Parameters.Any(x => x.Status == Status.Active && x.Code == param.Name);
// 				bool isEnvironment = Machine.Environment.Values.Keys.Any(x => x == param.Name);
// 				if (isSensor)
// 				{
// 					param.Type = "Sensor";
// 				}
// 				else if (isParameter)
// 				{
// 					param.Type = "Parameter";
// 				}
// 				else if (isEnvironment)
// 				{
// 					param.Type = "Environment";
// 				}
// 				else
// 				{
// 					param.Type = "Error";
// 				}
// 				returnValue.Add(param);
// 			}
// 		}

// 		return returnValue;
// 	}

// 	private bool ValidateParameterFormula(List<FormulaParam> parameters)
// 	{
// 		bool returnValue = true;
// 		if (parameters.IsNull())
// 		{
// 			returnValue = true;
// 		}
// 		else
// 		{
// 			foreach (FormulaParam fParam in parameters)
// 			{
// 				if (fParam.Type == "Sensor")
// 				{
// 					if (fParam.Property == "Value")
// 					{
// 						bool hasValue = Machine.Sensors.Any(x => x.Status == Status.Active && x.Code == fParam.Name && x.Value is not null);
// 						if (!hasValue)
// 						{
// 							returnValue = false;
// 						}
// 					}
// 				}
// 				else if (fParam.Type == "Parameter")
// 				{
// 					bool hasValue = Machine.Parameters.Any(x => x.Status == Status.Active && x.Code == fParam.Name && x.Value is not null);
// 					if (!hasValue)
// 					{
// 						returnValue = false;
// 					}
// 				}
// 				else if (fParam.Type == "Environment")
// 				{
// 					bool hasValue = Machine.Environment.Values.ContainsKey(fParam.Name);
// 					if (!hasValue)
// 					{
// 						returnValue = false;
// 					}
// 				}
// 				else
// 				{
// 					returnValue = false;
// 				}
// 			}
// 		}
// 		return returnValue;
// 	}

// 	private string ImplementFormula(string formula, List<FormulaParam> parameters)
// 	{
// 		string newFormula = formula;
// 		parameters?.ForEach(param =>
// 		{
// 			if (param.Type == "Sensor")
// 			{
// 				if (param.Property == "Value")
// 				{
// 					string value = Machine.Sensors.Find(x => x.Status == Status.Active && x.Code == param.Name).Value.ToStr();
// 					newFormula = newFormula.Replace(param.String, value);
// 				}
// 				if (param.Property == "MaximumValue")
// 				{
// 					string value = Machine.Sensors.Find(x => x.Status == Status.Active && x.Code == param.Name).MaximumValue.ToStr();
// 					newFormula = newFormula.Replace(param.String, value);
// 				}
// 				if (param.Property == "MinimumValue")
// 				{
// 					string value = Machine.Sensors.Find(x => x.Status == Status.Active && x.Code == param.Name).MinimumValue.ToStr();
// 					newFormula = newFormula.Replace(param.String, value);
// 				}
// 				if (param.Property == "Average")
// 				{
// 					string value = Machine.Sensors.Find(x => x.Status == Status.Active && x.Code == param.Name).AvgValue.ToStr();
// 					newFormula = newFormula.Replace(param.String, value);
// 				}
// 				else if (param.Property == "Aux")
// 				{
// 					Sensor sensor = Machine.Sensors.Find(x => x.Status == Status.Active && x.Code == param.Name);
// 					newFormula = newFormula.Replace(param.String, "\"" + sensor.Aux1.ToStr() + "\"");
// 				}
// 			}
// 			else if (param.Type == "Parameter")
// 			{
// 				if (param.Property == "Value")
// 				{
// 					string value = Machine.Parameters.Find(x => x.Status == Status.Active && x.Code == param.Name).Value.ToStr();
// 					newFormula = newFormula.Replace(param.String, value);
// 				}
// 				else if (param.Property == "IsCurrent")
// 				{
// 					newFormula = newFormula.Replace(param.String, "true");
// 				}
// 			}
// 			else if (param.Type == "Environment")
// 			{
// 				if (param.Property == "Value")
// 				{
// 					string value = Machine.Environment.Values[param.Name];
// 					if (string.IsNullOrEmpty(value))
// 					{
// 						value = "";
// 					}
// 					bool isNumber = double.TryParse(value, out double numericValue);
// 					if (!isNumber)
// 					{
// 						value = "\"" + value + "\"";
// 					}
// 					newFormula = newFormula.Replace(param.String, value);
// 				}
// 			}
// 		});
// 		return newFormula;
// 	}

// 	private Interpreter Interpreter
// 	{
// 		get
// 		{
// 			if (_interpreter is null)
// 			{
// 				Func<object, double> absFunction = Cast_abs;
// 				Func<object, double> floatFunction = Cast_float;
// 				Func<object, int> integerFunction = Cast_integer;
// 				Func<object, string> textFunction = Cast_text;
// 				Func<object, int, double> roundFunction = Cast_round;
// 				Func<bool, object, object, object> iifFunction = Cast_iif;
// 				Func<object, int> randomFunction = Cast_random;
// 				_interpreter = new Interpreter();
// 				_ = _interpreter.SetFunction("abs", absFunction);
// 				_ = _interpreter.SetFunction("float", floatFunction);
// 				_ = _interpreter.SetFunction("integer", integerFunction);
// 				_ = _interpreter.SetFunction("text", textFunction);
// 				_ = _interpreter.SetFunction("round", roundFunction);
// 				_ = _interpreter.SetFunction("iif", iifFunction);
// 				_ = _interpreter.SetFunction("random", randomFunction);
// 			}

// 			return _interpreter;
// 		}
// 	}

// 	private string GetSignature(Machine machine, List<FormulaParam> parameters)
// 	{
// 		string returnValue = string.Empty;
// 		parameters?.ForEach(param =>
// 		{
// 			if (param.Type == "Sensor")
// 			{
// 				string value = Machine.Sensors.Find(x => x.Status == Status.Active && x.Code == param.Name).Data.FirstOrDefault().LogDate.ToEpoch().ToStr().Md5();
// 				returnValue += value;
// 			}
// 			else if (param.Type == "Parameter")
// 			{
// 				string value = Machine.Parameters.Find(x => x.Status == Status.Active && x.Code == param.Name).Value.ToStr().Md5();
// 				returnValue += value;
// 			}
// 		});
// 		return returnValue.Md5().ToUpperInvariant();
// 	}

// 	private void ProcessAverage(string sensorId)
// 	{
// 		try
// 		{
// 			Sensor triggerSensor = Machine.Sensors.Find(x => x.Id == sensorId);
// 			if (triggerSensor is not null)
// 			{
// 				bool canConvert = double.TryParse(triggerSensor.Value.ToStr(), out double convertedDouble);
// 				if (canConvert)
// 				{
// 					triggerSensor.AvgValue = BLL.SaveSensorAverage(triggerSensor, Machine.Environment.WorkOrderId, convertedDouble);
// 					Machine.Environment.AvgCycle = triggerSensor.AvgValue.ToStr();
// 				}
// 			}
// 		}
// 		catch (Exception ex)
// 		{
// 			Log(ex.Message);
// 		}
// 	}

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public async Task Resolve(string triggerId, bool ignoreSave = false, string batchHistory = "")
// 	{
// 		try
// 		{
// 			if (!string.IsNullOrEmpty(triggerId) && !ignoreSave)
// 			{
// 				ProcessAverage(triggerId);
// 			}
// 			else
// 			{
// 				triggerId = string.Empty;
// 			}

// 			OrderParametersByDependency();

// 			Machine.Parameters?.ToArray()?.Where(prm => prm.Status == Status.Active && (prm.DependsSensorId.Equals(triggerId, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(prm.DependsSensorId))).ToList()?.ForEach(parameter =>
// 			{
// 				if (string.IsNullOrEmpty(parameter.CustomBehaviorId))
// 				{
// 					List<FormulaParam> formulaResult = ParseFormula(parameter.Formula);
// 					if (ValidateParameterFormula(formulaResult))
// 					{
// 						string formulaToEvaluate = ImplementFormula(parameter.Formula, formulaResult);

// 						try
// 						{
// 							object result = Interpreter.Eval(formulaToEvaluate);
// 							if (result is not null)
// 							{
// 								if (parameter.EditableScalar)
// 								{
// 									parameter.Signature = DateTime.UtcNow.ToString("yyyy/MM/ddTHH:mm:ss", CultureInfo.InvariantCulture).Md5();
// 								}
// 								else
// 								{
// 									parameter.Signature = GetSignature(Machine, formulaResult);
// 								}
// 								parameter.Value = result.ToStr();

// 								if (!ignoreSave && parameter.OutOfRangeAlert && (parameter.ApplicationAlert || parameter.EmailAlert))
// 								{
// 									EvaluateParameterAlert(parameter);
// 								}
// 							}
// 						}
// 						catch
// 						{
// 							parameter.Value = parameter.FallbackValue;
// 							parameter.Signature = "-";
// 						}
// 					}
// 				}
// 				else
// 				{
// 					if (parameter.CustomBehavior.IsNull())
// 					{
// 						/* parameter.CustomBehavior = BehaviorManager.Behaviors.Where(b => b.Id == parameter.CustomBehaviorId).FirstOrDefault();
//                          if (!parameter.CustomBehavior.isInitialized)
//                          {
//                              parameter.CustomBehavior.OnRequestData += CustomBehavior_OnRequestData;
//                              parameter.CustomBehavior.OnRequestSummary += CustomBehavior_OnRequestSummary;
//                              parameter.CustomBehavior.OnRequestCustom += CustomBehavior_OnRequestCustom;
//                              parameter.CustomBehavior.OnLog += CustomBehavior_OnLog;
//                              parameter.CustomBehavior.OnNotify += CustomBehavior_OnNotify;
//                              parameter.CustomBehavior.isInitialized = true;
//                          }*/
// 					}
// 					CustomBehaviorResult evalResult = parameter.CustomBehavior.Resolve(Machine, parameter.Code, parameter.FallbackValue);
// 					parameter.Value = evalResult.Value;
// 					parameter.Signature = evalResult.Signature.ToUpperInvariant();
// 				}
// 			});
// 			// Get Process Parameter Values
// 			if (Machine.ProcessType?.Details is not null && !Machine.IsAuxiliar)
// 			{
// 				foreach (ProcessTypeDetail processDetail in Machine.ProcessType.Details)
// 				{
// 					try
// 					{
// 						switch (processDetail.ValueType)
// 						{
// 							case ProcessTypeDetailSourceType.Scalar:
// 								{
// 									if (Machine.Environment.Modifiers is not null && Machine.Environment.Modifiers.TryGetValue(processDetail.Code, out string value))
// 									{
// 										processDetail.Value = value;
// 									}
// 									else
// 									{
// 										processDetail.Value = processDetail.ValueSourceId;
// 									}
// 									break;
// 								}
// 							case ProcessTypeDetailSourceType.Sensor:
// 								{
// 									if (Machine.Environment.Modifiers is not null && Machine.Environment.Modifiers.TryGetValue(processDetail.Code, out string value))
// 									{
// 										processDetail.Value = value;
// 									}
// 									else
// 									{
// 										Sensor s = Machine.Sensors.Find(x => x.Id == processDetail.ValueSourceId);
// 										if (s is not null)
// 										{
// 											processDetail.Value = s.Value.ToStr();
// 										}
// 									}
// 									break;
// 								}
// 							case ProcessTypeDetailSourceType.Parameter:
// 								{
// 									if (Machine.Environment.Modifiers is not null && Machine.Environment.Modifiers.TryGetValue(processDetail.Code, out string value))
// 									{
// 										processDetail.Value = value;
// 									}
// 									else
// 									{
// 										MachineParam s = Machine.Parameters.Find(x => x.Id == processDetail.ValueSourceId);
// 										if (s is not null)
// 										{
// 											processDetail.Value = s.Value.ToStr();
// 										}
// 									}
// 									break;
// 								}

// 							case ProcessTypeDetailSourceType.Nothing:
// 								break;
// 						}
// 					}
// 					catch
// 					{
// 					}
// 				}
// 			}
// 			// If Machine is output to the work order
// 			if (!ignoreSave && !Machine.IsAuxiliar)
// 			{
// 				if (Machine.ProcessType?.Details is not null && Machine.OEEConfiguration is not null)
// 				{
// 					// Massive processing preparation
// 					List<SensorData> batch = null;
// 					if (!string.IsNullOrEmpty(batchHistory) && Services.ContextCache.IncomingContext.TryGetValue(batchHistory, out string value))
// 					{
// 						batch = JsonConvert.DeserializeObject<List<SensorData>>(value);
// 						_ = Services.ContextCache.IncomingContext.Remove(batchHistory);
// 						if (batch?.Count > 1)
// 						{
// 							batch = [.. batch.OrderBy(x => x.LogDate)];
// 							MachineContextHistory context = BLL.GetMachineContextHistory(Machine.Id, batch[0].ValueDate, batch[^1].ValueDate);
// 							foreach (SensorData data in batch)
// 							{
// 								List<ProcessTypeDetail> toolValues = [];
// 								MachineOEEConfiguration OEEConfig = null;
// 								foreach (ProcessTypeDetail x in Machine.ProcessType?.Details)
// 								{
// 									toolValues.Add((ProcessTypeDetail)x.Clone());
// 								}

// 								if (context.OeeContext is not null)
// 								{
// 									KeyValuePair<DateTime, MachineOEEConfiguration> currentContext = context.OeeContext.Where(x => x.Key < data.ValueDate).OrderByDescending(z => z.Key).FirstOrDefault();
// 									if (!currentContext.Equals(default(KeyValuePair<DateTime, MachineOEEConfiguration>)))
// 									{
// 										OEEConfig = currentContext.Value;
// 									}
// 									else
// 									{
// 										OEEConfig = Machine.OEEConfiguration;
// 									}
// 								}
// 								else
// 								{
// 									OEEConfig = Machine.OEEConfiguration;
// 								}
// 								if (context.ToolContext is not null)
// 								{
// 									WorkOrderToolContext[] currentContext = [.. context.ToolContext.Where(x => x.Key <= data.ValueDate).Select(x => x.Value)];
// 									foreach (ProcessTypeDetail x in toolValues)
// 									{
// 										WorkOrderToolContext currentToolValue = currentContext.Where(y => y.Code == x.Code).OrderByDescending(z => z.Date).FirstOrDefault();
// 										if (currentToolValue is not null)
// 										{
// 											x.Value = currentToolValue.Value;
// 										}
// 									}
// 								}
// 								data.Tag = new { OEEConfiguration = OEEConfig, ProcessDetails = toolValues };
// 							}
// 						}
// 					}
// 					//End of Massive Processing preparation

// 					MachineOEEConfiguration OEEConfiguration = Machine.OEEConfiguration;
// 					List<ProcessTypeDetail> ProcessTypeDetails = Machine.ProcessType.Details;
// 					if (OEEConfiguration.PerformanceMode == OEEMode.Automatic && (OEEConfiguration.AvailabilityMode == OEEMode.Automatic || (OEEConfiguration.AvailabilityMode == OEEMode.AutomaticSwitch && Machine.Online)) && triggerId == OEEConfiguration.PerformanceTriggerId && !string.IsNullOrEmpty(triggerId))
// 					{
// 						//Massive Processing OEE
// 						if (batch?.Count > 1)//Valid si esta correcta && batch.Count > 1
// 						{
// 							foreach (object tag in batch.ConvertAll(x => x.Tag))
// 							{
// 								if (tag is not null)
// 								{
// 									if (tag.GetAnonymousValue("OEEConfiguration") is not null)
// 									{
// 										OEEConfiguration = (MachineOEEConfiguration)tag.GetAnonymousValue("OEEConfiguration");
// 									}
// 									if (tag.GetAnonymousValue("OEEConfiguration") is not null)
// 									{
// 										ProcessTypeDetails = (List<ProcessTypeDetail>)tag.GetAnonymousValue("ProcessDetails");
// 									}
// 								}

// 								double produced = Machine.Environment.Received.ToDouble();
// 								double rejected = Machine.Environment.Rejected.ToDouble();
// 								double WorkOrderFactor = Machine.Environment.PerformanceFactor;
// 								double ProcessFactor = Machine.Environment.ProcessPerformanceFactor;
// 								double DeviceFactor = OEEConfiguration.PerformanceDefaultTimeQty * OEEConfiguration.PerformanceTimeFactor;
// 								double performanceDefaultValue = 0;
// 								double CurrentValue = 0;
// 								double CurrentFactor = 0;
// 								if (OEEConfiguration.PerformanceDefaultType == 1)
// 								{
// 									Sensor tempSensor = Machine.Sensors.Find(x => x.Id == OEEConfiguration.PerformanceDefaultValue);
// 									if (tempSensor is not null)
// 									{
// 										performanceDefaultValue = tempSensor.Value.ToDouble();
// 									}
// 								}
// 								else if (OEEConfiguration.PerformanceDefaultType == 2)
// 								{
// 									MachineParam tempParam = Machine.Parameters.Find(x => x.Id == OEEConfiguration.PerformanceDefaultValue);
// 									if (tempParam is not null)
// 									{
// 										performanceDefaultValue = tempParam.Value.ToDouble();
// 									}
// 								}
// 								else if (OEEConfiguration.PerformanceDefaultType == 3)
// 								{
// 									performanceDefaultValue = OEEConfiguration.PerformanceDefaultValue.ToDouble();
// 								}
// 								if (Math.Abs(performanceDefaultValue) < 1e-6)
// 								{
// 									performanceDefaultValue = 1;
// 								}
// 								DeviceFactor /= performanceDefaultValue;

// 								ProcessTypeDetail availabilityProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.PerformanceTimeSourceId);
// 								ProcessTypeDetail performanceProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.PerformanceSourceId);
// 								if (performanceProcessType is not null)
// 								{
// 									CurrentValue = performanceProcessType.Value.ToDouble();
// 								}
// 								if (availabilityProcessType is not null && CurrentValue > 0)
// 								{
// 									CurrentFactor = availabilityProcessType.Value.ToDouble() / CurrentValue;
// 								}

// 								if (BLL.SaveMachinePerformance(Machine.Id, Machine.Environment.ProcessId, Machine.Environment.IsOutput, Machine.Environment.WorkOrderId, CurrentValue, CurrentFactor, DeviceFactor, ProcessFactor, WorkOrderFactor))
// 								{
// 									Machine.Environment.IsCycle = "true";

// 									// detonar evento Machine produced
// 									if (onMachineProduced is not null)
// 									{
// 										CustomActionEventArgs<MachineProducedNotification, bool> MachineProducedEventArgs = new()
// 										{
// 											Request = new MachineProducedNotification
// 											{
// 												MachineId = Machine.Id,
// 												ProcessId = Machine.Environment.ProcessId,
// 												WorkOrderId = Machine.Environment.WorkOrderId,
// 												Quantity = CurrentValue
// 											}
// 										};
// 										onMachineProduced(this, MachineProducedEventArgs);
// 									}

// 									if (Machine.Environment.IsOutput)
// 									{
// 										Machine.Environment.Received = (produced + CurrentValue).ToStr();
// 										int finishMode = Config.Configuration["WorkOrder-FinishMode"].ToInt32();
// 										int progressMode = Config.Configuration["WorkOrder-ProgressMode"].ToInt32();

// 										// Automatic Finish
// 										if (finishMode == 1)
// 										{
// 											ProcessTypeDetail qualityProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.QualitySourceId);
// 											if (qualityProcessType is not null)
// 											{
// 												double currentRejected = qualityProcessType.Value.ToDouble();
// 												rejected += currentRejected;
// 											}
// 											//If Progress Mode is Produced Amount
// 											if (
// 												(progressMode == 1 && produced + CurrentValue >= Machine.Environment.Total.ToDouble() && Machine.Environment.Total.ToDouble() > 0)
// 												||
// 												(progressMode == 2 && (produced - rejected + CurrentValue) >= Machine.Environment.Total.ToDouble() && Machine.Environment.Total.ToDouble() > 0)
// 												)
// 											{
// 												WorkOrder tempOrder = new(Machine.Environment.WorkOrderId)
// 												{
// 													Status = Status.Finished
// 												};
// 												_ = BLL.SystemUpdateWorkOrderStatus(tempOrder, new User(-1));
// 											}
// 										}
// 									}
// 								}

// 								if (OEEConfiguration.QualityMode == OEEMode.Automatic && OEEConfiguration.PerformanceMode == OEEMode.Automatic && OEEConfiguration.AvailabilityMode == OEEMode.Automatic)
// 								{
// 									performanceProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.PerformanceSourceId);
// 									ProcessTypeDetail qualityProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.QualitySourceId);
// 									CurrentValue = performanceProcessType.Value.ToDouble();
// 									double RejectedValue = qualityProcessType.Value.ToDouble();
// 									BLL.SaveMachineQuality(Machine.Id, Machine.Environment.IsOutput, Machine.Environment.WorkOrderId, Machine.Environment.ProcessId, null, QualityType.Automatic, QualityMode.Direct, CurrentValue, RejectedValue, new User(-1));
// 								}
// 							}

// 							if (!string.IsNullOrEmpty(batchHistory))
// 							{
// 								//BLL.SyncAvailabilityBatch(batchHistory);
// 							}

// 							await CalculateDeviceOEEAsync().ConfigureAwait(false);
// 						}
// 						else
// 						{
// 							// Single processing
// 							double produced = Machine.Environment.Received.ToDouble();
// 							double rejected = Machine.Environment.Rejected.ToDouble();
// 							double WorkOrderFactor = Machine.Environment.PerformanceFactor;
// 							double ProcessFactor = Machine.Environment.ProcessPerformanceFactor;
// 							double DeviceFactor = OEEConfiguration.PerformanceDefaultTimeQty * OEEConfiguration.PerformanceTimeFactor;
// 							double performanceDefaultValue = 0;
// 							double CurrentValue = 0;
// 							double CurrentFactor = 0;
// 							if (OEEConfiguration.PerformanceDefaultType == 1)
// 							{
// 								Sensor tempSensor = Machine.Sensors.Find(x => x.Id == OEEConfiguration.PerformanceDefaultValue);
// 								if (tempSensor is not null)
// 								{
// 									performanceDefaultValue = tempSensor.Value.ToDouble();
// 								}
// 							}
// 							else if (OEEConfiguration.PerformanceDefaultType == 2)
// 							{
// 								MachineParam tempParam = Machine.Parameters.Find(x => x.Id == OEEConfiguration.PerformanceDefaultValue);
// 								if (tempParam is not null)
// 								{
// 									performanceDefaultValue = tempParam.Value.ToDouble();
// 								}
// 							}
// 							else if (OEEConfiguration.PerformanceDefaultType == 3)
// 							{
// 								performanceDefaultValue = OEEConfiguration.PerformanceDefaultValue.ToDouble();
// 							}
// 							if (Math.Abs(performanceDefaultValue) < 1e-6)
// 							{
// 								performanceDefaultValue = 1;
// 							}
// 							DeviceFactor /= performanceDefaultValue;

// 							ProcessTypeDetail availabilityProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.PerformanceTimeSourceId);
// 							ProcessTypeDetail performanceProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.PerformanceSourceId);
// 							if (performanceProcessType is not null)
// 							{
// 								CurrentValue = performanceProcessType.Value.ToDouble();
// 							}
// 							if (availabilityProcessType is not null && CurrentValue > 0)
// 							{
// 								CurrentFactor = availabilityProcessType.Value.ToDouble() / CurrentValue;
// 							}
// 							if (BLL.SaveMachinePerformance(Machine.Id, Machine.Environment.ProcessId, Machine.Environment.IsOutput, Machine.Environment.WorkOrderId, CurrentValue, CurrentFactor, DeviceFactor, ProcessFactor, WorkOrderFactor))
// 							{
// 								Machine.Environment.IsCycle = "true";

// 								// detonar evento Machine produced
// 								if (onMachineProduced is not null)
// 								{
// 									CustomActionEventArgs<MachineProducedNotification, bool> MachineProducedEventArgs = new()
// 									{
// 										Request = new MachineProducedNotification
// 										{
// 											MachineId = Machine.Id,
// 											ProcessId = Machine.Environment.ProcessId,
// 											WorkOrderId = Machine.Environment.WorkOrderId,
// 											Quantity = CurrentValue
// 										}
// 									};
// 									onMachineProduced(this, MachineProducedEventArgs);
// 								}

// 								if (Machine.Environment.IsOutput)
// 								{
// 									Machine.Environment.Received = (produced + CurrentValue).ToStr();
// 									int finishMode = Config.Configuration["WorkOrder-FinishMode"].ToInt32();
// 									int progressMode = Config.Configuration["WorkOrder-ProgressMode"].ToInt32();

// 									// Automatic Finish
// 									if (finishMode == 1)
// 									{
// 										ProcessTypeDetail qualityProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.QualitySourceId);
// 										if (qualityProcessType is not null)
// 										{
// 											double currentRejected = qualityProcessType.Value.ToDouble();
// 											rejected += currentRejected;
// 										}
// 										//If Progress Mode is Produced Amount
// 										if (
// 											(progressMode == 1 && produced + CurrentValue >= Machine.Environment.Total.ToDouble() && Machine.Environment.Total.ToDouble() > 0)
// 											||
// 											(progressMode == 2 && (produced - rejected + CurrentValue) >= Machine.Environment.Total.ToDouble() && Machine.Environment.Total.ToDouble() > 0)
// 											)
// 										{
// 											WorkOrder tempOrder = new(Machine.Environment.WorkOrderId)
// 											{
// 												Status = Status.Finished
// 											};
// 											_ = BLL.SystemUpdateWorkOrderStatus(tempOrder, new User(-1));
// 										}
// 									}
// 								}
// 							}

// 							if (OEEConfiguration.QualityMode == OEEMode.Automatic && OEEConfiguration.PerformanceMode == OEEMode.Automatic && OEEConfiguration.AvailabilityMode == OEEMode.Automatic)
// 							{
// 								performanceProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.PerformanceSourceId);
// 								ProcessTypeDetail qualityProcessType = ProcessTypeDetails.Find(x => x.Code == OEEConfiguration.QualitySourceId);
// 								CurrentValue = performanceProcessType.Value.ToDouble();
// 								double RejectedValue = qualityProcessType.Value.ToDouble();
// 								BLL.SaveMachineQuality(Machine.Id, Machine.Environment.IsOutput, Machine.Environment.WorkOrderId, Machine.Environment.ProcessId, null, QualityType.Automatic, QualityMode.Direct, CurrentValue, RejectedValue, new User(-1));
// 							}

// 							await CalculateDeviceOEEAsync().ConfigureAwait(false);
// 						}
// 					}
// 					else if (Machine.OEEConfiguration.AvailabilityMode == OEEMode.AutomaticSwitch && triggerId == ((Sensor)Machine.OEEConfiguration.AvailabilitySensor).Id && !string.IsNullOrEmpty(triggerId) && !string.IsNullOrEmpty(batchHistory))
// 					{
// 						//BLL.SyncAvailabilityBatch(batchHistory);
// 					}
// 				}
// 			}
// 			else
// 			{
// 				await CalculateDeviceOEEAsync().ConfigureAwait(false);
// 			}
// 			OnResolve?.Invoke(this, new CustomActionEventArgs<ResolvedMachine, bool> { Request = new ResolvedMachine(Machine) { IgnoreInsert = ignoreSave } });
// 		}
// 		catch (Exception ex)
// 		{
// 			Machine.IsBusy = false;
// 			if (ex.InnerException is not null)
// 			{
// 				Log(string.Format("Parameter \"{0}\" in device \"{1}\" has the following error: {2}", ex.Message, ex.Source, ex.InnerException.Message));
// 			}
// 			else
// 			{
// 				Log(ex.Message + " StackTrace" + ex.StackTrace);
// 			}
// 		}
// 	}

// 	internal async Task CalculateDeviceOEEAsync()
// 	{
// 		OEEModel returnValue;
// 		DateTime startDate;
// 		if (Machine.OEEHistory.IsEmpty)
// 		{
// 			startDate = DateTime.UtcNow.AddMinutes(-5);
// 		}
// 		else
// 		{
// 			startDate = Machine.OEEHistory.First().Key;
// 		}
// 		returnValue = await BLL.GetLiveOEEAsync(Machine.Id, null).ConfigureAwait(false);
// 		if (returnValue is not null)
// 		{
// 			if (Machine.OEEHistory.IsEmpty)
// 			{
// 				Machine.OEEHistory.AddOrUpdate(startDate, returnValue, (K, O) => returnValue);
// 			}
// 			else
// 			{
// 				Machine.OEEHistory.AddOrUpdate(DateTime.UtcNow, returnValue, (K, O) => returnValue);
// 			}

// 			if (Machine.OEEHistory.Count > 50)
// 			{
// 				_ = Machine.OEEHistory.Remove(Machine.OEEHistory.Keys.First(), out _);
// 			}
// 		}
// 		Machine.Environment.Availability = returnValue.Availability.ToStr();
// 		Machine.Environment.Performance = returnValue.Performance.ToStr();
// 		Machine.Environment.Quality = returnValue.Quality.ToStr();
// 		Machine.Environment.OEE = returnValue.OEE.ToStr();
// 		Machine.Environment.OffTime = returnValue.OffTime;
// 		Machine.Environment.OnTime = returnValue.OnTime;
// 	}

// 	private void EvaluateParameterAlert(MachineParam parameter)
// 	{
// 		if (!ErrorCount.TryGetValue(parameter.Id, out int value))
// 		{
// 			value = 0;
// 			ErrorCount.Add(parameter.Id, value);
// 		}
// 		if (parameter.Value.ToBool())
// 		{
// 			ErrorCount[parameter.Id] = ++value;
// 		}
// 		else
// 		{
// 			ErrorCount[parameter.Id] = 0;
// 		}

// 		if (parameter.OutOfRangeAlert && parameter.Value.ToBool() && parameter.AlertLevels?.Count > 0)
// 		{
// 			bool isLast = false;
// 			List<AlertLevel> currentLevels = [.. parameter.AlertLevels.OrderBy(x => x.Level)];
// 			int total = ErrorCount[parameter.Id];
// 			int last = currentLevels.Sum(x => x.Iterations);
// 			AlertLevel curLvl = currentLevels.Find(x => currentLevels.Where(y => y.Level <= x.Level).Sum(z => z.Iterations) >= total);
// 			int sum = 0;
// 			if (curLvl is not null)
// 			{
// 				sum = currentLevels.Where(x => x.Level <= curLvl.Level).Sum(x => x.Iterations);
// 				if (total < sum)
// 				{
// 					curLvl = currentLevels.Find(x => x.Level == curLvl.Level - 1);
// 					curLvl ??= currentLevels.FirstOrDefault();
// 				}
// 			}
// 			else
// 			{
// 				isLast = true;
// 				curLvl = currentLevels[^1];
// 				sum = last;
// 			}

// 			AlertLevel[] notificationLevels = [.. currentLevels.Where(x => x.Level <= curLvl.Level)];
// 			if ((total == sum && !isLast) || ((isLast || currentLevels.Count == 1) && total % curLvl.Iterations == 0))
// 			{
// 				foreach (AlertLevel level in notificationLevels)
// 				{
// 					CustomBehavior_OnNotify(this, new NotifyParameterRequest
// 					{
// 						Machine = Machine,
// 						Parameter = parameter,
// 						Level = level.Level,
// 						Message = "Alert: {1}",
// 						DetailMessage = "@DetailsParamAlert",
// 						Parameters = string.Join(',', Machine.Description, parameter.Description, parameter.Value.ToStr(), DateTime.UtcNow.ToShortDateString(), DateTime.UtcNow.ToShortTimeString())
// 					});
// 				}
// 			}
// 		}
// 	}

// 	private void CustomBehavior_OnRequestData(object sender, CustomActionEventArgs<SensorDataRequest, List<SensorData>> e)
// 	{
// 		if (!e.Callback.IsNull())
// 		{
// 			try
// 			{
// 				if (!string.IsNullOrEmpty(e.Request.MachineId) || !string.IsNullOrEmpty(e.Request.SensorId))
// 				{
// 					e.Callback(BLL.GetSensorData(e.Request, new User(0)));
// 				}
// 			}
// 			catch (Exception ex)
// 			{
// 				logger.Error(ex);
// 				CustomBehavior behavior = (CustomBehavior)sender;
// 				string request = JsonConvert.SerializeObject(e.Request);
// 				Log(string.Format("Error OnRequestData - Behavior:{2} - SensorId: {0} - Error: {1}", request, ex.Message, behavior.Name));
// 				e.Callback(null);
// 			}
// 		}
// 	}

// 	private static void CustomBehavior_OnNotify(object sender, NotifyParameterRequest e)
// 	{
// 		//AQUÍ SE INSERTA UNA NOTIFICACIÓN
// 	}

// 	private void CustomBehavior_OnRequestSummary(object sender, CustomActionEventArgs<SummarizedSensorDataRequest, SummarizedSensorData> e)
// 	{
// 		if (!e.Callback.IsNull())
// 		{
// 			try
// 			{
// 				e.Callback(BLL.GetSensorSummary(e.Request));
// 			}
// 			catch (Exception ex)
// 			{
// 				logger.Error(ex);
// 				Log(string.Format("Error OnRequestSummary - SensorId: {0} - Error: {1}", e.Request.SensorId, ex.Message));
// 				e.Callback(null);
// 			}
// 		}
// 	}

// 	private void CustomBehavior_OnRequestCustom(object sender, CustomActionEventArgs<CustomDataRequest, List<object>> e)
// 	{
// 		if (!e.Callback.IsNull())
// 		{
// 			try
// 			{
// 				e.Callback(BLL.GetSensorCustom(e.Request));
// 			}
// 			catch (Exception ex)
// 			{
// 				logger.Error(ex);
// 				Log(string.Format("Error OnRequestCustom -Program: {0} , Parameters: {1} - Error: {2}", e.Request.Program, JsonConvert.SerializeObject(e.Request.Parameters), ex.Message));
// 				e.Callback(null);
// 			}
// 		}
// 	}

// 	private void OrderParametersByDependency()
// 	{
// 		List<ParameterDependency> unOrderedList = [];
// 		string currentParam = string.Empty;
// 		try
// 		{
// 			Machine.Parameters?.Where(x => x.Status == Status.Active).ToList()?.ForEach(parameter =>
// 			{
// 				currentParam = parameter.Description;
// 				if (!string.IsNullOrEmpty(parameter.CustomBehaviorId))
// 				{
// 					if (parameter.CustomBehavior.IsNull())
// 					{
// 						/*parameter.CustomBehavior = BehaviorManager.Behaviors.Where(b => b.Id == parameter.CustomBehaviorId).FirstOrDefault();
//                         if (parameter.CustomBehavior.IsNull())
//                         {
//                             throw new Exception(string.Format("The custom behavior with ID {0} was not found", parameter.CustomBehaviorId));
//                         }
//                         if (!parameter.CustomBehavior.isInitialized)
//                         {
//                             parameter.CustomBehavior.OnRequestData += CustomBehavior_OnRequestData;
//                             parameter.CustomBehavior.OnRequestSummary += CustomBehavior_OnRequestSummary;
//                             parameter.CustomBehavior.OnRequestCustom += CustomBehavior_OnRequestCustom;
//                             parameter.CustomBehavior.OnLog += CustomBehavior_OnLog;
//                             parameter.CustomBehavior.OnNotify += CustomBehavior_OnNotify;
//                             parameter.CustomBehavior.isInitialized = true;
//                         }*/
// 						parameter.Formula = string.Empty;
// 					}
// 					MachineParam[] paramList = [.. parameter.CustomBehavior.RequiredCodes.Select(fr => Machine.Parameters.Find(mp => mp.Code == fr))];
// 					paramList = [.. paramList.Where(param => !param.IsNull() && !Machine.Sensors.Any(s => s.Code == param.Code))];
// 					unOrderedList.Add(new ParameterDependency { Parameter = parameter, DependsOn = [.. paramList] });
// 				}
// 				else
// 				{
// 					List<FormulaParam> formulaResult = ParseFormula(parameter.Formula);
// 					if (formulaResult?.Count > 0)
// 					{
// 						MachineParam[] paramList = [.. formulaResult.Select(fr => Machine.Parameters.Find(mp => mp.Code == fr.Name))];
// 						paramList = [.. paramList.Where(param => !param.IsNull() && !Machine.Sensors.Any(s => s.Code == param.Code))];
// 						unOrderedList.Add(new ParameterDependency { Parameter = parameter, DependsOn = [.. paramList] });
// 					}
// 					else
// 					{
// 						unOrderedList.Add(new ParameterDependency { Parameter = parameter, DependsOn = null });
// 					}
// 				}
// 			});
// 		}
// 		catch (Exception ex)
// 		{
// 			throw new Exception(currentParam, ex) { Source = Machine.Description };
// 		}
// 		try
// 		{
// 			int[] result = GetTopologicalSortOrder(unOrderedList);
// 			List<MachineParam> OrderedList = [.. unOrderedList.Where(x => x.DependsOn.IsNull()).Select(x => x.Parameter).ToArray()];
// 			for (int i = result.Length - 1; i >= 0; i--)
// 			{
// 				int value = result[i];
// 				if (!OrderedList.Any(val => val.Id == Machine.Parameters[value].Id))
// 				{
// 					OrderedList.Add(Machine.Parameters[value]);
// 				}
// 			}
// 			unOrderedList = null;
// 			result = null;
// 			Machine.Parameters = OrderedList;
// 		}
// 		catch
// 		{
// 			throw new Exception(string.Format("Machine {0} has cycled parameters", Machine.Description));
// 		}
// 	}
// 	/*
//     private void CustomBehavior_OnLog(object sender, string e)
//     {
//         Log(e);
//     }
//     */
// 	private static int[] GetTopologicalSortOrder(List<ParameterDependency> fields)
// 	{
// 		TopologicalSorter g = new(fields.Count);
// 		Dictionary<string, int> _indexes = [];

// 		//add vertices
// 		for (int i = 0; i < fields.Count; i++)
// 		{
// 			_indexes[fields[i].Parameter.Id.ToLowerInvariant()] = g.AddVertex(i);
// 		}

// 		//add edges
// 		for (int i = 0; i < fields.Count; i++)
// 		{
// 			if (fields[i].DependsOn is not null)
// 			{
// 				for (int j = 0; j < fields[i].DependsOn.Length; j++)
// 				{
// 					if (!fields[i].DependsOn[j].IsNull())
// 					{
// 						g.AddEdge(i,
// 							_indexes[fields[i].DependsOn[j]?.Id?.ToLowerInvariant()]);
// 					}
// 				}
// 			}
// 		}

// 		return g.Sort();
// 	}

// 	private static Regex MyRegex() => MyRegex1();

// 	[GeneratedRegex("{([^}]*)}\\.([^\\s&^)&^:&^(&^\"&^?]*)")]
// 	private static partial Regex MyRegex1();
// }

// /// <summary>
// ///
// /// </summary>
// public class ParameterDependency
// {
// 	/// <summary>
// 	///
// 	/// </summary>
// 	public MachineParam Parameter { get; set; }

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public MachineParam[] DependsOn { get; set; }

// 	/// <summary>Returns a string that represents the current object.</summary>
// 	/// <returns>A string that represents the current object.</returns>
// 	public override string ToString()
// 	{
// 		return !DependsOn.IsNull()
// 			? Parameter.Code + " > " + string.Join(',', DependsOn.Where(static x => x is not null).Select(static x => x.Code))
// 			: Parameter.Code;
// 	}
// }
