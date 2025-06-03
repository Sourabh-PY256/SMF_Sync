using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;

namespace EWP.SF.Item.BusinessLayer;
public class ProfileOperation : IProfileOperation
{
    private readonly ICatalogRepo _profileRepo;
    private readonly IApplicationSettings _applicationSettings;
    private readonly IAttachmentOperation _attachmentOperation;
    private readonly IActivityOperation _activityOperation;
    private readonly ISchedulingCalendarShiftsOperation _schedulingCalendarShiftsOperation;

    public ProfileOperation(ICatalogRepo profileRepo, IApplicationSettings applicationSettings, IAttachmentOperation attachmentOperation,
     IActivityOperation activityOperation, ISchedulingCalendarShiftsOperation schedulingCalendarShiftsOperation)
    {
        _profileRepo = profileRepo;
        _applicationSettings = applicationSettings;
        _attachmentOperation = attachmentOperation;
        _activityOperation = activityOperation;
        _schedulingCalendarShiftsOperation = schedulingCalendarShiftsOperation;
        ;
    }
	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<ResponseData> MergeProfile(CatProfile ProfileInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		ResponseData returnValue = null;

		#region Permission validation

		// if (!systemOperator.Permissions.Any(x => x.Code.Equals(Permissions.CAT_CATALOGS_MANAGE, StringComparison.Ordinal)))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		//Coversión de datos para crear las listas
		if (!string.IsNullOrEmpty(ProfileInfo.AssignedAsset))
		{
			ProfileInfo.PositionAssets =
			[
				.. ProfileInfo.AssignedAsset.Split(',').Select(q => new PositionAssets
					{
						AssetCode = q.Substring(q.IndexOf('-') + 1, q.Length - q[..q.IndexOf('-')].Length - 1),
						AssetTypeCode = q[..q[..q.IndexOf('-')].Length]
					}),
				];
		}
		if (!string.IsNullOrEmpty(ProfileInfo.Skills))
		{
			ProfileInfo.PositionSkills =
			[
				.. ProfileInfo.Skills.Split(',').Select(q => new PositionSkills
					{
						SkillCode = q
					}),
				];
		}
		returnValue = _profileRepo.MergeProfile(ProfileInfo, systemOperator, Validate);
		if (!Validate && returnValue is not null)
		{
			CatProfile ObjProfile = _profileRepo.GetCatalogProfile().Where(x => x.Code == returnValue.Code).FirstOrDefault(x => x.Status != Status.Failed);
			//await ObjProfile.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
			if (NotifyOnce)
			{
				await _attachmentOperation.SaveImageEntity("Position", ProfileInfo.Image, returnValue.Code, systemOperator).ConfigureAwait(false);
				//Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Profiles, returnValue.Action, Data = ObjProfile }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
				//returnValue.Entity = ObjProfile;
			}
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateProfile(List<PositionExternal> profileInfoList, List<PositionExternal> profileInfoListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (profileInfoList?.Count > 0)
		{
			NotifyOnce = profileInfoList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (PositionExternal cycleProfile in profileInfoList)
			{
				PositionExternal profile = cycleProfile;
				Line++;
				try
				{
					CatProfile originalPosition = _profileRepo.GetCatalogProfile().Where(x => x.Code == profile.PositionCode)?.FirstOrDefault(x => x.Status != Status.Failed);
					bool editMode = originalPosition is not null;
					if (editMode && profileInfoListOriginal is not null)
					{
						profile = profileInfoListOriginal.Find(x => x.PositionCode == cycleProfile.PositionCode);
						profile ??= cycleProfile;
					}
					BaseId = profile.PositionCode;
					List<ValidationResult> results = [];
					ValidationContext context = new(profile, null, null);
					if (!Validator.TryValidateObject(profile, context, results))
					{
						if (editMode)
						{
							_ = results.RemoveAll(result => result.ErrorMessage.Contains("is required", StringComparison.OrdinalIgnoreCase));
						}
						if (results.Count > 0)
						{
							throw new Exception($"{results[0]}");
						}
					}
					// Status
					int status = 1;
					if (!editMode || !string.IsNullOrEmpty(profile.Status))
					{
						if (string.Equals(profile.Status.Trim(), "DISABLE", StringComparison.OrdinalIgnoreCase))
						{
							status = 2;
						}
					}

					if (!editMode && status == 2)
					{
						throw new Exception("Cannot import a disabled profile record");
					}
					// AuthorizationRequired
					bool authorizationRequired = true;
					if (!editMode || !string.IsNullOrEmpty(profile.ReqAuthorization))
					{
						if (string.Equals(profile.Status.Trim(), "NO", StringComparison.OrdinalIgnoreCase))
						{
							authorizationRequired = false;
						}
					}

					string level = "";
					if (!string.IsNullOrEmpty(profile.ScheduleLevel))
					{
						switch (profile?.ScheduleLevel.ToStr().ToUpperInvariant())
						{
							case "PRIMARY":

							case "SECONDARY":
								level = profile?.ScheduleLevel;
								break;
						}
					}
					// Skills
					string skills = string.Empty;
					if (profile.Skills is not null && !editMode)
					{
						List<string> arrSkills = null;
						List<CatSkills> skillList = _profileRepo.GetCatSkillsList();
						skillList.ForEach(skill =>
						{
							profile.Skills.ForEach(skillProfile =>
							{
								if (string.Equals(skillProfile.SkillCode.Trim(), skill.Code.Trim(), StringComparison.OrdinalIgnoreCase))
								{
									arrSkills.Add(skill.SkillId);
								}
							});
						});
						skills = string.Join(',', [.. arrSkills]);
					}
					// Data Import
					CatProfile profileInfo = new()
					{
						Code = profile.PositionCode,
						NameProfile = !string.IsNullOrEmpty(profile.PositionName) ? profile.PositionName : profile.PositionCode,
						AuthorizationRequired = authorizationRequired,
						CostPerHour = Convert.ToDecimal(profile.CostPerHour),
						Skills = skills,
						Status = (Status)status,
						Schedule = profile.Schedule.ToStr().Equals("yes", StringComparison.OrdinalIgnoreCase),
						ScheduleLevel = level
					};

					if (editMode)
					{
						profileInfo = originalPosition;
						profileInfo.AuthorizationRequired = authorizationRequired;
						if (!string.IsNullOrEmpty(skills))
						{
							profileInfo.Skills = skills;
						}
						if (!string.IsNullOrEmpty(profile.PositionName))
						{
							profileInfo.NameProfile = profile.PositionName;
						}
						if (profile.CostPerHour > 0)
						{
							profileInfo.CostPerHour = Convert.ToDecimal(profile.CostPerHour);
						}
						if (!string.IsNullOrEmpty(profile.PositionName))
						{
							profileInfo.NameProfile = profile.PositionName;
						}
						if (status > 0)
						{
							profileInfo.Status = (Status)status;
						}
						if (!string.IsNullOrEmpty(profile.Schedule))
						{
							profileInfo.Schedule = profile.Schedule.Equals("yes", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(level))
						{
							profileInfo.ScheduleLevel = level;
						}
					}
					ResponseData response = await MergeProfile(profileInfo, systemOperator, Validate).ConfigureAwait(false);
					returnValue.Add(response);
				}
				catch (Exception ex)
				{
					MessageError = new ResponseData
					{
						Id = BaseId,
						Message = ex.Message,
						Code = "Line:" + Line.ToStr()
					};
					returnValue.Add(MessageError);
				}
			}
		}
		if (!Validate)
		{
			// if (!NotifyOnce)
			// {
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Profiles, Action = ActionDB.IntegrateAll.ToStr() });
			// }
			returnValue = Level switch
			{
				LevelMessage.Warning => [.. returnValue.Where(x => !string.IsNullOrEmpty(x.Message))],
				LevelMessage.Error => [.. returnValue.Where(x => !x.IsSuccess)],
				_ => returnValue
			};
		}
		return returnValue;
	}
}
