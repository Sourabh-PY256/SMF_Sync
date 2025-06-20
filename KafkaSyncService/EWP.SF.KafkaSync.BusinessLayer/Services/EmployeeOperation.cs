
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using System.Xml.Serialization;
using SixLabors.ImageSharp;
using EWP.SF.Common.Models.Catalogs;
using Newtonsoft.Json;
using EWP.SF.Common.Constants;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class EmployeeOperation : IEmployeeOperation
{
    private readonly IEmployeeRepo _employeeRepo;
    private readonly ICatalogRepo _catalogRepo;
    private readonly ISchedulingRepo _schedulingRepo;
    private readonly IAttachmentOperation _attachmentOperation;
    private readonly IActivityOperation _activityOperation;
    private readonly ISchedulingCalendarShiftsOperation _schedulingCalendarShiftsOperation;

    public EmployeeOperation(IEmployeeRepo employeeRepo, IAttachmentOperation attachmentOperation,
     IActivityOperation activityOperation,
     ICatalogRepo catalogRepo, ISchedulingRepo schedulingRepo,
     ISchedulingCalendarShiftsOperation schedulingCalendarShiftsOperation)
    {
        _employeeRepo = employeeRepo;
        _catalogRepo = catalogRepo;
        _schedulingRepo = schedulingRepo;
        _attachmentOperation = attachmentOperation;
        _activityOperation = activityOperation;
        _schedulingCalendarShiftsOperation = schedulingCalendarShiftsOperation;

    }
    #region Employee
    public async Task<List<ResponseData>> ImportEmployeesAsync(List<EmployeeExternal> requestValue, List<EmployeeExternal> originalValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true, bool isDataSync = false)
    {
        List<ResponseData> returnValue = [];
        Employee employee = null;
        Employee employeeHistory = null;
        EmployeeContractsDetail historyContract = null;
        List<Employee> employeeList = [];
        List<CatProfile> lstProfiles = _catalogRepo.GetCatalogProfile();
        List<CatSkills> lstCatSkills = _catalogRepo.GetCatSkillsList();
        CatProfile catProfile = null;
        CatSkills catSkill = null;
        EmployeeSkills employeeSkill = null;
        EmployeeContractsDetail employeeContract = null;
        foreach (EmployeeExternal cycleItem in requestValue)
        {
            EmployeeExternal item = cycleItem;

            employeeHistory = _employeeRepo.GetEmployee(cycleItem.EmployeeCode);
            bool editMode = employeeHistory is not null;
            if (editMode && originalValue is not null)
            {
                item = originalValue.Find(x => x.EmployeeCode == cycleItem.EmployeeCode);
                item ??= cycleItem;
            }
            if (!editMode)
            {
                employee = new Employee
                {
                    Code = item.EmployeeCode,
                    Name = !string.IsNullOrEmpty(item.EmployeeName) ? item.EmployeeName : item.EmployeeCode,
                    LastName = item.EmployeeLastName,
                    Email = item.Email,
                    ExternalId = item.ExternalId
                };
            }
            else
            {
                employee = employeeHistory;
                if (!string.IsNullOrEmpty(item.EmployeeName))
                {
                    employee.Name = item.EmployeeName;
                }
                if (!string.IsNullOrEmpty(item.EmployeeLastName))
                {
                    employee.LastName = item.EmployeeLastName;
                }
                if (!string.IsNullOrEmpty(item.Email))
                {
                    employee.Email = item.Email;
                }
                if (!string.IsNullOrEmpty(item.ExternalId))
                {
                    employee.ExternalId = item.ExternalId;
                }
            }

            employee.Status = (Status)(string.IsNullOrEmpty(item.Status) ? 1 : item.Status == "Disable" ? 2 : 1);

            if (employee.Status != Status.Active && !editMode)
            {
                throw new Exception("Cannot import a disabled employee record");
            }
            employee.CostPerHour = item.CostPerHour;

            if (employeeHistory is null)
            {
                //Validar de que forma se cargará el pass cuando sea desde import y sea un registro nuevo
                employee.Password = Helper.Security.getHash(employee.Code.ToUpperInvariant() + ":" + Helper.Security.getHash(employee.Name));
                //Front end have this code to create a hash
                //$scope.employees.Current.Username.toUpperCase() + ":" + MD5($scope.employees.Current.Password).toUpperCase()
            }

            if (!editMode || !string.IsNullOrEmpty(item.ProfileCode))
            {
                catProfile = lstProfiles?.Where(q => string.Equals(q.Code, item.ProfileCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(x => x.Status != Status.Failed);
                //Valid value Profile to Catalog. If the value null, set default profile value.
                if (catProfile is not null)
                {
                    employeeContract = new EmployeeContractsDetail
                    {
                        ProfileId = catProfile.ProfileId,
                        DateStart = DateTime.Now.Date
                    };
                    if (employeeHistory is null || (employeeHistory.CurrentPositionId == employee.CurrentPositionId && (employee.EmployeeSkills is null || employee.EmployeeSkills.Count == 0)))
                    {
                        employee.EmployeeSkills = [];
                        foreach (string val in catProfile.Skills.Split(','))
                        {
                            if (val.Length > 0)
                            {
                                catSkill = lstCatSkills?.FirstOrDefault(q => q.SkillId == val);
                                employeeSkill = new EmployeeSkills
                                {
                                    SkillId = catSkill.SkillId
                                };
                                employee.EmployeeSkills.Add(employeeSkill);
                            }
                        }
                        employee.EmployeeContractsDetail = [employeeContract];
                    }
                    else
                    {
                        employeeHistory.EmployeeSkills = _employeeRepo.EmployeeSkillsList(employeeHistory.Id);
                        employeeHistory.EmployeeContractsDetail = _employeeRepo.ContractsDetailList(employeeHistory.Id);
                        historyContract = employeeHistory.EmployeeContractsDetail.OrderByDescending(qq => qq.DateEnd).FirstOrDefault();
                        if (historyContract is not null && historyContract.ProfileId != employeeContract.ProfileId)
                        {
                            employee.EmployeeSkills = [];
                            foreach (string val in catProfile.Skills.Split(','))
                            {
                                if (val.Length > 0)
                                {
                                    catSkill = lstCatSkills?.FirstOrDefault(q => q.SkillId == val);
                                    employeeSkill = new EmployeeSkills
                                    {
                                        SkillId = catSkill.SkillId
                                    };
                                    employee.EmployeeSkills.Add(employeeSkill);
                                }
                            }

                            employeeHistory.EmployeeContractsDetail = [.. employeeHistory.EmployeeContractsDetail.Where(qq => qq.ProfileId != historyContract.ProfileId && qq.DateEnd != historyContract.DateEnd)];
                            historyContract.DateEnd = DateTime.Now.Date.AddDays(-1);
                            employeeHistory.EmployeeContractsDetail.Add(historyContract);
                            employeeHistory.EmployeeContractsDetail.Add(employeeContract);
                            employee.EmployeeContractsDetail = employeeHistory.EmployeeContractsDetail;
                        }
                        else
                        {
                            employee.EmployeeSkills = employeeHistory.EmployeeSkills;
                            employee.EmployeeContractsDetail = employeeHistory.EmployeeContractsDetail;
                        }
                    }
                }
                else
                {
                    throw new Exception("Profile code not found '" + item.ProfileCode + "'");
                }
            }
            employeeList.Add(employee);
        }
        returnValue.AddRange(await MRGEmployee(employeeList, systemOperator, Validate, Level, NotifyOnce, isDataSync).ConfigureAwait(false));
        return returnValue;
    }
    public async Task<List<ResponseData>> MRGEmployee(List<Employee> requestValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true, bool isDataSync = false)
    {
        List<ResponseData> returnValue = [];
        ResponseData responseMessage;
        ResponseData responseError;
        //List<Employee> users = new List<User>();
        ResponseModel result;
        List<EmployeeSkills> responseEmployeeSkills = [];
        List<EmployeeContractsDetail> responseEmployeeContractsDetail = [];
        Employee employeeLog = null;
        const int Line = 0;
        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.HR_EMPLOYEE_MANAGE))
        {
            throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }
        NotifyOnce = requestValue.Count == 1;
        foreach (Employee item in requestValue)
        {
            try
            {
                item.UserId = systemOperator.Id;
                //Coversión de datos para crear las listas
                if (!string.IsNullOrEmpty(item.AssetsId))
                {
                    item.EmployeeAssets =
                    [
                        .. item.AssetsId.Split(',').Select(q => new AssetsTree
                            {
                                AssetCode = q.Substring(q.IndexOf('-') + 1, q.Length - q[..q.IndexOf('-')].Length - 1),
                                AssetTypeCode = q[..q[..q.IndexOf('-')].Length]
                            }),
                        ];
                }

                responseMessage = _employeeRepo.MRGEmployee(item, Validate, systemOperator);
                returnValue.Add(responseMessage);
                if (!responseMessage.IsSuccess)
                {
                    continue;
                }
                else if (responseMessage.Id == "@CEOPositionHeldBy")
                {
                    responseMessage.Entity = new Employee { Id = "@CEOPositionHeldBy" };
                    continue;
                }
                await _attachmentOperation.SaveImageEntity("Employee", item.Image, item.Code, systemOperator).ConfigureAwait(false);
                if (item.AttachmentIds is not null)
                {
                    foreach (string attachment in item.AttachmentIds)
                    {
                        await _attachmentOperation.AttachmentSync(attachment, item.Code, systemOperator).ConfigureAwait(false);
                    }
                }
                if (item.Activities?.Count > 0)
                {
                    foreach (Activity activity in item.Activities)
                    {
                        if (string.IsNullOrEmpty(activity.Id))
                        {
                            Activity newActivity = await _activityOperation.CreateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ManualDelete)
                        {
                            bool tempResult = await _activityOperation.DeleteActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ActivityClassId > 0)
                        {
                            await _activityOperation.UpdateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                    }
                }
                if (item.Shift?.CodeShift is not null
                                  && !string.IsNullOrEmpty(item.Shift.CodeShift))
                {
                    item.Shift.Validation = false;
                    item.Shift.IdAsset = item.Code;
                    _ = _schedulingCalendarShiftsOperation.UpdateSchedulingCalendarShifts(item.Shift, systemOperator);
                }
                if (item.ShiftDelete?.Id is not null
                && !string.IsNullOrEmpty(item.ShiftDelete.Id))
                {
                    item.ShiftDelete.Validation = false;
                    _ = _schedulingCalendarShiftsOperation.DeleteSchedulingCalendarShifts(item.ShiftDelete, systemOperator);
                }
                item.Id = responseMessage.Id;
                if (returnValue is not null && !Validate)
                {
                    //Verificamos si se guardaran skills
                    if (item.EmployeeSkills?.Count > 0)
                    {
                        XmlSerializer employeeSkills = new(typeof(List<EmployeeSkills>));
                        MemoryStream ms = new();

                        employeeSkills.Serialize(ms, item.EmployeeSkills);
                        string xmlDowntimesTypes = System.Text.Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");

                        responseEmployeeSkills = _employeeRepo.CreateEmployeeSkills(item.Id, xmlDowntimesTypes, systemOperator);
                        responseEmployeeSkills.ForEach(x => x.AttachmentId = item.EmployeeSkills.First(x => x.SkillId == x.SkillId).AttachmentId);
                        //Validar el guardado de Attachment

                        foreach (EmployeeSkills itemSkill in responseEmployeeSkills)
                        {
                            if (!string.IsNullOrEmpty(itemSkill.AttachmentId))
                            {
                                await _attachmentOperation.AttachmentSync(itemSkill.AttachmentId, itemSkill.Employee_Skills_Id, systemOperator).ConfigureAwait(false);
                            }
                        }
                    }

                    //Verificamos si se guardaran contratos
                    if (item.EmployeeContractsDetail?.Count > 0)
                    {
                        XmlSerializer employeesContractsDetail = new(typeof(List<EmployeeContractsDetail>));
                        MemoryStream ms = new();

                        employeesContractsDetail.Serialize(ms, item.EmployeeContractsDetail);
                        string xmlDowntimesTypes = System.Text.Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");

                        responseEmployeeContractsDetail = _employeeRepo.CreateEmployeeContractsDetail(item.Id, xmlDowntimesTypes, systemOperator);
                        if (!isDataSync)
                        {
                            GeneratePositionShiftExplosion(item.Code);
                        }
                    }
                }

                //Valid insert detalle tables
                // if ()
                //    continue;
                if (!Validate)
                {
                    employeeLog = _employeeRepo.ListEmployees(null, item.Code).Find(x => x.Status != Status.Failed);
                    await employeeLog.Log(responseMessage.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                }

                if (!Validate)
                {
                    try
                    {
                        // if (responseMessage.Action == ActionDB.Create)
                        // {
                        //     result = await CheckUserTraining(employeeLog).ConfigureAwait(false);
                        //     if (result.IsSuccess && result.Data is null)
                        //     {
                        //         result = await CreateUserTraining(employeeLog).ConfigureAwait(false);
                        //     }
                        // }
                        // else
                        // {
                        //     result = await CheckUserTraining2(employeeLog.Email, employeeLog.Code).ConfigureAwait(false);
                        //     if (result.IsSuccess && result.Data is null)
                        //     {
                        //         result = await CreateUserTraining2(employeeLog.Email, employeeLog.Code, employeeLog.Name, employeeLog.LastName).ConfigureAwait(false);
                        //     }
                        // }Need to discuss
                    }
                    catch (Exception ex)
                    {
                        responseError = new ResponseData
                        {
                            Message = ex.Message,
                            Code = "Warning Line:" + Line.ToStr()
                        };
                        returnValue.Add(responseError);
                    }
                }
                if (NotifyOnce && !Validate)
                {
                    responseMessage.Entity = employeeLog;
                    //Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Employees, Data = employeeLog, responseMessage.Action }, responseMessage.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                }
            }
            catch (Exception ex)
            {
                responseError = new ResponseData
                {
                    Message = ex.Message,
                    Code = "Line:" + Line.ToStr()
                };
                returnValue.Add(responseError);
            }
        }
        // if (!NotifyOnce && !Validate)
        // {
        // 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Employees, Data = new object(), Action = ActionDB.IntegrateAll });
        // }
        return Level switch
        {
            LevelMessage.Warning => [.. returnValue.Where(x => !string.IsNullOrEmpty(x.Message))],
            LevelMessage.Error => [.. returnValue.Where(x => !x.IsSuccess)],
            _ => returnValue
        };
    }
    public void GeneratePositionShiftExplosion(string employeeCode)
    {
        _schedulingRepo.GeneratePositionShiftExplosion(employeeCode);
    }
    /// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public List<Employee> GetEmployees(string id, string code, User systemOperator, DateTime? DeltaDate = null)
    {
        #region Permission validation

        if (!systemOperator.Permissions.Any(static x => x.Code is Permissions.PRD_ORDERPROGRESS_MANAGE or Permissions.HR_EMPLOYEE_MANAGE))
        {
            throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        return _employeeRepo.ListEmployees(id, code, DeltaDate);
    }
    /// <summary>
    ///
    /// </summary>
    // public async Task<ResponseModel> CheckUserTraining(Employee employee)
    // {
    // 	ResponseModel returnValue = new();
    // 	Training credentials = await GetTraining().ConfigureAwait(false);
    // 	string apiBaseUrl = credentials.urltraining + "/webservice/rest/server.php";
    // 	try
    // 	{
    // 		using HttpClient client = new();
    // 		client.DefaultRequestHeaders.Accept.Clear();
    // 		// client.DefaultRequestHeaders.Add("Authorization", token.TokenType + " " + token.AccessToken);
    // 		// StringContent httpContent = new StringContent(JsonConvert.SerializeObject(permissionData), Encoding.UTF8, "application/json");

    // 		if (string.IsNullOrEmpty(employee.Email))
    // 		{
    // 			employee.Email = employee.Name.ToUpperInvariant() + "@sf.com";
    // 		}
    // 		KeyValuePair<string, string>[] data =
    // 		[
    // 			new KeyValuePair<string, string>("wstoken",credentials.token),
    // 				new KeyValuePair<string, string>("wsfunction", "core_user_get_users_by_field"),
    // 				new KeyValuePair<string, string>("moodlewsrestformat", "json"),
    // 				new KeyValuePair<string, string>("field", "username"),
    // 				new KeyValuePair<string, string>("values[0]", employee.Code.ToUpperInvariant())
    // 		];

    // 		using FormUrlEncodedContent content = new(data);
    // 		HttpResponseMessage result = await client.PostAsync(new Uri(apiBaseUrl), content).ConfigureAwait(false);
    // 		string resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
    // 		returnValue.Data = JsonConvert.DeserializeObject<object>(resultContent);
    // 	}
    // 	catch (Exception ex)
    // 	{
    // 		//logger.Error(ex);
    // 		returnValue.IsSuccess = false;
    // 		returnValue.Message = ex.Message;
    // 	}
    // 	return returnValue;
    // }
    #endregion Employee
}