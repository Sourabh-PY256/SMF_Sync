using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.Text;
using Range = EWP.SF.Common.Models.Range;
using SixLabors.ImageSharp;
using EWP.SF.Common.Models.Catalogs;

namespace EWP.SF.Item.BusinessLayer;

public interface ISchedulingCalendarShiftsOperation
{
    bool DeleteSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator);
    SchedulingCalendarShifts UpdateSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator);
}
