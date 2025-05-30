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

public interface IEmployeeOperation
{
    Task<List<ResponseData>> ImportEmployeesAsync(List<EmployeeExternal> requestValue, List<EmployeeExternal> originalValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true, bool isDataSync = false);
    Task<List<ResponseData>> MRGEmployee(List<Employee> requestValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true, bool isDataSync = false);

}