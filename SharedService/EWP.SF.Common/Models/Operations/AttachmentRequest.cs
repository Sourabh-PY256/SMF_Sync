using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Operations;

/// <summary>
///
/// </summary>
public class AttachmentRequest
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AuxId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Ext { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Entity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsFileOffice { get; set; }

	/// <summary>
	///
	/// </summary>
	public FileAttachmentRequest File { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<KeysRequest> KeyColumns { get; set; }
}

/// <summary>
///
/// </summary>
public class FileAttachmentRequest
{
	/// <summary>
	///
	/// </summary>
	public string name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FileBase64 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string lastModified { get; set; }

	/// <summary>
	///
	/// </summary>
	public string lastModifiedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string type { get; set; }
}

/// <summary>
///
/// </summary>
public class KeysRequest
{
	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OrderAttach { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Key> Keys { get; set; }
}

/// <summary>
///
/// </summary>
public class Key
{
	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }
}

/// <summary>
///
/// </summary>
public class FileRequest
{
	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FileBase64 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Ext { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Entity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AuxId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsNewCurrent { get; set; }
}

/// <summary>
///
/// </summary>
public class AttachmentLocal
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AuxId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Extension { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Entity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FileBase64 { get; set; }
	/// <summary>
	///
	/// </summary>
	public byte[] FileBaseByte { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsTemp { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool FoundImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<KeysRequest> KeyColumns { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsImageEntity { get; set; }
}

/// <summary>
///
/// </summary>
public class AttachmentResponse
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Actions { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsTemp { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Extension { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }
}

/// <summary>
///
/// </summary>
public class AttachmentImageEntity
{
	/// <summary>
	///
	/// </summary>
	public string AttachmentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentBase64 { get; set; }
}

/// <summary>
///
/// </summary>
public class AttachmentExternal
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Entity")]
	[JsonProperty(PropertyName = "Entity")]
	[DefaultMappingEntity("Entity")]
	public string Entity { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Procedure Code format.")]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string Aux1 { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string Aux2 { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string Aux3 { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string Aux4 { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string AttachmentId { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(300)]
	public string AttachmentName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string AttachmentExtension { get; set; }
}
/// <summary>
///
/// </summary>
public class AttachmentExternalResponse
{
	/// <summary>
	///
	/// </summary>
	public string Entity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EntityType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AuxId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Aux1 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Aux2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Aux3 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Aux4 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentIdExternal { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentExtension { get; set; }
}
public class AttachmentDownloadResponse
{
	/// <summary>
	///
	/// </summary>
	public string FileName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FilePath { get; set; }

	/// <summary>
	///
	/// </summary>
	public FileStream FileStream { get; set; }
}
