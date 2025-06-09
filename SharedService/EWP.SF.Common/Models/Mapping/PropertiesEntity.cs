using System.Diagnostics.CodeAnalysis;
using System.Reflection;


namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public static class PropertiesEntity
{
	/// <summary>
	///
	/// </summary>
	public static Type GetEntityType(string entity)
	{
		// NOTA: Para encontrarlo debe existir en algun namespace de los siguientes
		string[] pathList =
		[
			"EWP.SF.API.BusinessEntities",
				"EWP.SF.API.BusinessEntities.Attributes",
				"EWP.SF.API.BusinessEntities.Common_Models",
				"EWP.SF.API.BusinessEntities.Common_Models.Generic",
				"EWP.SF.API.BusinessEntities.Common_Models.System",
				"EWP.SF.API.BusinessEntities.Constants",
				"EWP.SF.API.BusinessEntities.Enumerators",
				"EWP.SF.API.BusinessEntities.EventHandlers",
				"EWP.SF.API.BusinessEntities.Mapping_Models",
				"EWP.SF.API.BusinessEntities.MesModels",
				"EWP.SF.API.BusinessEntities.MesModels.DataSync",
				"EWP.SF.API.BusinessEntities.MesModels.NotificationSettings",
				"EWP.SF.API.BusinessEntities.MesModels.Sensors",
				"EWP.SF.API.BusinessEntities.MesModels.Tag",
				"EWP.SF.API.BusinessEntities.MigrationModels",
				"EWP.SF.API.BusinessEntities.RequestModels",
				"EWP.SF.API.BusinessEntities.RequestModels.IntegrationStaging",
				"EWP.SF.API.BusinessEntities.RequestModels.MES",
				"EWP.SF.API.BusinessEntities.RequestModels.Operations",
				"EWP.SF.API.BusinessEntities.RequestModels.Settings",
				"EWP.SF.API.BusinessEntities.RequestModels.System",
				"EWP.SF.API.BusinessEntities.ResponseModels",
				"EWP.SF.API.BusinessEntities.ResponseModels.Dashboards",
				"EWP.SF.API.BusinessEntities.ResponseModels.IntegrationStaging",
				"EWP.SF.API.BusinessEntities.ResponseModels.WorkOrder",
				"EWP.SF.API.BusinessEntities.RequestModels.Operations.AttachmentImageEntity.AttachmentExternal"
		];

		// busca uno por uno hasta encontrarlo
		foreach (string path in pathList)
		{
			Type type = GetByPath(path, entity);
			if (type is not null)
			{
				return type;
			}
		}

		// si no lo encontró, lanza excepción
		throw new Exception($"No se encontró entidad {entity} solicitada");
	}

	private static Type GetByPath(string path, string name)
	{
		try
		{
			return Type.GetType($"{path}.{name}");
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	///
	/// </summary>
	public static object GetProperties(string entity, string[] parent = null)
	{
		Type objType = GetEntityType(entity);
		return GetListProperty(objType);
	}

	/// <summary>
	///
	/// </summary>
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	public static List<PropertiesEntityModel> GetListProperty(Type objType)
	{
		// get all public static properties of MyClass type
		PropertyInfo[] propertyInfos;
		PropertiesEntityModel property;
		List<PropertiesEntityModel> propiertieslst = [];
		propertyInfos = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance |
		BindingFlags.Static);
		// sort properties by name
		Array.Sort(propertyInfos,
				static (PropertyInfo propertyInfo1, PropertyInfo propertyInfo2) => propertyInfo1.Name.CompareTo(propertyInfo2.Name));

		// write property names
		foreach (PropertyInfo propertyInfo in propertyInfos)
		{
			property = new PropertiesEntityModel
			{
				Name = propertyInfo.Name
			};
			foreach (CustomAttributeData attributeCustom in propertyInfo.CustomAttributes)
			{
				switch (attributeCustom.AttributeType.Name)
				{
					case "KeyAttribute":
						property.IsKey = true;
						break;

					case "RequiredAttribute":
						property.IsRequired = true;
						break;

					case "MaxLengthAttribute":
						try
						{
							property.LongField = Convert.ToInt32(attributeCustom.ConstructorArguments[0].Value.ToString());
						}
						catch { }
						break;

					case "RegularExpressionAttribute":
						try
						{
							property.Expression = attributeCustom.ConstructorArguments[0].Value.ToString();
						}
						catch { }
						break;

					case "DefaultMappingEntity":
						try
						{
							property.DefaultMappingEntity = attributeCustom.ConstructorArguments[0].Value.ToString();
						}
						catch { }
						break;
				}
			}
			property.DataType = propertyInfo.PropertyType.Name;
			if (property.DataType.Contains("List"))
			{
				property.IsList = true;
				Type objTypeChild = Type.GetType(propertyInfo.PropertyType.GenericTypeArguments[0].ToString(), true);
				property.Child = GetListProperty(objTypeChild);
			}
			propiertieslst.Add(property);
		}

		return propiertieslst;
	}
}

/// <summary>
///
/// </summary>
public class PropertiesEntityModel
{
	/// <summary>
	///
	/// </summary>
	public bool IsKey { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DataType { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsRequired { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsList { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Expression { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? LongField { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DefaultMappingEntity { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<DefaultMappingEntityObject> DefaultMappingEntityList { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<PropertiesEntityModel> Child { get; set; }
}
