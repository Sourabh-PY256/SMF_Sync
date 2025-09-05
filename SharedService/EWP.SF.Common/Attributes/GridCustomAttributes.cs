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
	/// <summary>
	/// Name of the entity in the database.
	/// </summary>
	public string Name { get; internal set; } = name;

	/// <summary>
	/// Name of the component in the front-end.
	/// </summary>
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
	/// <summary>
	/// Name of the property in the database.
	/// </summary>
	public string Name { get; internal set; } = name;
}

/// <summary>
/// Utilizado para definir un "tipo" especifico a una propiedad de una entidad (UserGrid)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridCustomType : Attribute
{
	/// <summary>
	/// Gets or sets the type of the column.
	/// </summary>
	public GridColumnType Type { get; internal set; }

	/// <summary>
	/// Gets or sets the name of the custom type.
	/// </summary>
	public string CustomTypeName { get; internal set; }

	/// <summary>
	/// Gets or sets the custom format of the column.
	/// </summary>
	public GridColumnFormat CustomFormat { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GridCustomType" /> class.
	/// </summary>
	public GridCustomType(GridColumnType type)
	{
		Type = type;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GridCustomType" /> class.
	/// </summary>
	public GridCustomType(GridColumnType type, string customTypeName)
	{
		Type = type;
		CustomTypeName = customTypeName;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GridCustomType" /> class.
	/// </summary>
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
public sealed class GridRequireTranslate : Attribute;

/// <summary>
/// Se utiliza para indicar que requiere una traducción utilizando el nombre de columna real o uno custom
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridRequireDecode : Attribute;

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
	/// <summary>
	/// Gets or sets the property to use.
	/// </summary>
	public string PropertyToUse { get; internal set; } = propertyToUse;

	/// <summary>
	/// Gets or sets the entity reference.
	/// </summary>
	public string EntityReference { get; internal set; } = entityReference;

	/// <summary>
	/// Gets or sets the property to reference.
	/// </summary>
	public string PropertyToReference { get; set; } = propertyReference;

	/// <summary>
	/// Gets or sets the property to show.
	/// </summary>
	public string PropertyToShow { get; internal set; } = propertyToShow;
}

/// <summary>
/// Utilizado para que en el front no pueda utilizarse para ordenar datos
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridDisableSorting : Attribute;

/// <summary>
/// Utilizado para que en el front no pueda ocultarse
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridDisabledHiding : Attribute;

/// <summary>
/// Utilizado para que en el front no pueda ocultarse
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GridConcatProperty(string name) : Attribute
{
	/// <summary>
	/// Gets or sets the name of the property.
	/// </summary>
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
/// <param name="entityName">Se refiere al módulo al que hará referencia en caso de aplicar</param>
/// <param name="propertyToReference">Se refiere al nombre de la propiedad de la entidad referenciada con la que hará la comparación en caso de requerir uno especifico. </param>
/// <param name="propertyToUse">Se refiere a la propiedad que utilizará como párametro de la misma clase. Si se deja vácio significa que utilizará el valor de la propiedad donde se especificó el atributo de Drill Down ([GridDrillDown])</param>
/// <param name="useInSearch">Indica que el valor se utilizará en la busqueda</param>
/// <param name="useAsModalParameter">Indica que dicho valor se utilizará como párametro en las opciones del modal</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class GridDrillDown(string entityName = null, string propertyToReference = null, string propertyToUse = null, bool useInSearch = true, bool useAsModalParameter = false) : Attribute
{
	/// <summary>
	/// Gets or sets the entity name.
	/// </summary>
	public string EntityName { get; internal set; } = entityName;

	/// <summary>
	/// Gets or sets the property to reference.
	/// </summary>
	public string PropertyToReference { get; internal set; } = propertyToReference;

	/// <summary>
	/// Gets or sets the property to use.
	/// </summary>
	public string PropertyToUse { get; internal set; } = propertyToUse;

	/// <summary>
	/// Gets or sets a value indicating whether the property should be used in search.
	/// </summary>
	public bool UseInSearch { get; internal set; } = useInSearch;

	/// <summary>
	/// Gets or sets a value indicating whether the property should be used as a modal parameter.
	/// </summary>
	public bool UseAsModalParameter { get; internal set; } = useAsModalParameter;
}
