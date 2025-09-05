using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Transactions;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using System.Diagnostics.CodeAnalysis;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.DataAccess;

namespace EWP.SF.Common.BusinessLayer;

public class ValidateEntity : IValidateEntity
{
    private readonly IValidateEntityRepository _validateEntityRepository;
    public ValidateEntity(IValidateEntityRepository validateEntityRepository)
    {
        _validateEntityRepository = validateEntityRepository;
    }
    
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	public void ValidateEntityKeys(ILoggableEntity objEntity, User user)
	{
		ArgumentNullException.ThrowIfNull(objEntity);
		ArgumentNullException.ThrowIfNull(user);

		Type objType = objEntity.GetType();
		string entity = objType.Name;
		Dictionary<string, string> columns = objType.GetProperties()
			.Where(static x => x.GetCustomAttribute<EntityColumnAttribute>() is not null)
			.ToDictionary(
				static x => x.Name,
				static x => x.GetCustomAttribute<EntityColumnAttribute>().ColumnName
			);
		List<string> values = [];
		// if (columns.Count > 0 && !string.IsNullOrEmpty(objEntity.EntityLogConfiguration.LogTable))
		// {
		// 	foreach (string key in columns.Keys)
		// 	{
		// 		object value = objType.GetProperty(key)?.GetValue(objEntity);
		// 		values.Add(value?.ToString() ?? ""); // Handles null values safely
		// 	}
		// }
		string keyToValidate = string.Join('|', values);
		if (!string.IsNullOrEmpty(keyToValidate) && !string.IsNullOrEmpty(entity))
		{
			_validateEntityRepository.ValidateEntityCode(keyToValidate, entity, user);
		}
	}
}
