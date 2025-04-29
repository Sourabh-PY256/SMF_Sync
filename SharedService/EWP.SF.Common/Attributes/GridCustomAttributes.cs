using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Attributes;

/// <summary>
/// Utilizado para definir un "nombre" con el que se buscará en la tabla de sf_user_grid
/// </summary>
/// <remarks>
/// Inicializa el nombre que se utilizará en base de datos
/// </remarks>
/// <param name="name">Nombre en referencia de base de datos en tabla de sf_UserGrid. Éste nombre es el mismo como esta regostrado en sf_entities</param>
/// <param name="component">Nombre del componente del front. Éste campo se utiliza para diferenciarlo en la base de datos ya que es uno diferente.
/// esto es util para los módulos de front que utilizan la misma entidad pero difernete en base de datos</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GridBDEntityName(string name, string component = null) : Attribute
{
	public string Name { get; internal set; } = name;
	public string Component { get; internal set; } = component;
}

/// <summary>
/// Para no considerar dicha propiedad como columna
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridIgnoreProperty : Attribute;

/// <summary>
/// Utilizado para definir un titulo personalizado a la propiedad para el grid
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridCustomPropertyName(string name) : Attribute
{
	public string Name { get; internal set; } = name;
}

/// <summary>
/// Utilizado para definir un "tipo" especifico a una propiedad de una entidad (UserGrid)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridCustomType : Attribute
{
	public GridColumnType Type { get; internal set; }
	public string CustomTypeName { get; internal set; }
	public GridColumnFormat CustomFormat { get; internal set; }

	public GridCustomType(GridColumnType type)
	{
		Type = type;
	}

	public GridCustomType(GridColumnType type, string customTypeName)
	{
		Type = type;
		CustomTypeName = customTypeName;
	}

	public GridCustomType(GridColumnType type, string customTypeName, GridColumnFormat customFormat)
	{
		Type = type;
		CustomTypeName = customTypeName;
		CustomFormat = customFormat;
	}
}

/// <summary>
/// Se utiliza para indicar que requiere una traducción utilizando el nombre de columna real o uno custom
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridRequireTranslate : Attribute
{
}

/// <summary>
/// Se utiliza para indicar que requiere una traducción utilizando el nombre de columna real o uno custom
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridRequireDecode : Attribute
{
}

/// <summary>
/// Permitirá relacionar información entre entidades
/// </summary>
/// <remarks>
/// Define las propiedades con las que se relacionará y las que se utilizarán
/// </remarks>
/// <param name="propertyToUse">Si la pripiedad es un objeto complejo, éste párametro se utilizá para hacer referencia a una propiedad de dicho objeto y poder realizar
/// dichas operaciones utilizando ese valor en lugar del objeto original</param>
/// <param name="entityReference">Nombre del módulo (entidad) al que hará referencia en el front</param>
/// <param name="propertyReference">Nombre de la propiedad con la que se relacionará de la entidad</param>
/// <param name="propertyToShow">Nombre de la propiedad de dicha entidad buscada en "EntityReferece" que se mostrará en su lugar del valor original</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridLookUpEntity(string propertyToUse, string entityReference, string propertyReference, string propertyToShow) : Attribute
{
	public string PropertyToUse { get; internal set; } = propertyToUse;
	public string EntityReference { get; internal set; } = entityReference;
	public string PropertyToReference { get; set; } = propertyReference;
	public string PropertyToShow { get; internal set; } = propertyToShow;
}

/// <summary>
/// Utilizado para que en el front no pueda utilizarse para ordenar datos
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridDisableSorting : Attribute
{
}

/// <summary>
/// Utilizado para que en el front no pueda ocultarse
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridDisabledHiding : Attribute
{
}

/// <summary>
/// Utilizado para que en el front no pueda ocultarse
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridConcatProperty(string name) : Attribute
{
	public string Name { get; internal set; } = name;
}

/// <summary>
/// <para>
/// Para que un drill down funcione se requiere enviar el párametro correcto especificado en cada módulo.
/// En algunos caso se tienen drill downs con párametros incorrectos los cuales pertenecen a un módulo existente, es decir,
/// que en lugar de esperar un "code" se envía un "name" por ejemplo.
/// </para>
/// <para>
/// Esté atributo permite relacionar un valor con otro para poder obtener el objeto
/// Utilizado para definir como tal el "DrillDown" lo que permitirá relacionar información entre entidades
/// Si se utiliza más de uno en la misma propiedad, significa que utilizará ambos valores como parametro e iran en el orden
/// en el que se declararon sobre la propiedad
/// </para>
/// </summary>
/// <remarks>
/// </remarks>
/// <param name="entityName">Se refiere al módulo al que hará referencia en caso de aplicar</param>
/// <param name="propertyToReference">Se refiere al nombre de la propiedad de la entidad referenciada con la que hará la comparación en caso de requerir uno especifico. </param>
/// <param name="propertyToUse">Se refiere a la propiedad que utilizará como párametro de la misma clase. Si se deja vácio significa que utilizará el valor de la propiedad donde se especificó el atributo de Drill Down ([GridDrillDown])</param>
/// <param name="useInSearch">Indica que el valor se utilizará en la busqueda</param>
/// <param name="useAsModalParameter">Indica que dicho valor se utilizará como párametro en las opciones del modal</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class GridDrillDown(string entityName = null, string propertyToReference = null, string propertyToUse = null, bool useInSearch = true, bool useAsModalParameter = false) : Attribute
{
	public string EntityName { get; internal set; } = entityName;
	public string PropertyToReference { get; internal set; } = propertyToReference;
	public string PropertyToUse { get; internal set; } = propertyToUse;
	public bool UseInSearch { get; internal set; } = useInSearch;
	public bool UseAsModalParameter { get; internal set; } = useAsModalParameter;
}
