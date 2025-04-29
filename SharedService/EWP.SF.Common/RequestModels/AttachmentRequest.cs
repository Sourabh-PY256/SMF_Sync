using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EWP.SF.Common.Models;

using Newtonsoft.Json;

namespace EWP.SF.Common.RequestModels.Operations;

public class AttachmentRequest
{
	public string Id { get; set; }
	public string AuxId { get; set; }
	public string TypeCode { get; set; }
	public string Name { get; set; }
	public string Ext { get; set; }
	public string Size { get; set; }
	public string Entity { get; set; }
	public int Status { get; set; }
	public bool IsFileOffice { get; set; }
	public FileAttachmentRequest File { get; set; }
	public List<KeysRequest> KeyColumns { get; set; }
}

public class FileAttachmentRequest
{
	public string name { get; set; }
	public string FileBase64 { get; set; }
	public string lastModified { get; set; }
	public string lastModifiedDate { get; set; }
	public string size { get; set; }
	public string type { get; set; }
}

public class KeysRequest
{
	public string Type { get; set; }
	public string Value { get; set; }
	public int OrderAttach { get; set; }
	public List<Key> Keys { get; set; }
}

public class Key
{
	public string Name { get; set; }
	public string Value { get; set; }
}

public class FileRequest
{
	public string Name { get; set; }
	public string FileBase64 { get; set; }
	public string Ext { get; set; }
	public int Size { get; set; }
	public string Entity { get; set; }
	public string AuxId { get; set; }
	public bool IsNewCurrent { get; set; }
}

public class AttachmentLocal
{
	public string Id { get; set; }
	public string AuxId { get; set; }
	public string TypeCode { get; set; }
	public string Name { get; set; }
	public string Extension { get; set; }
	public string Size { get; set; }
	public string Entity { get; set; }
	public int Status { get; set; }
	public string FileBase64 { get; set; }
	public bool IsTemp { get; set; }
	public bool FoundImage { get; set; }
	public List<KeysRequest> KeyColumns { get; set; }
	public bool IsImageEntity { get; set; }
}

public class AttachmentResponse
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Size { get; set; }
	public string ModifyDate { get; set; }
	public string Actions { get; set; }
	public bool IsTemp { get; set; }
	public string Extension { get; set; }
	public DateTime CreationDate { get; set; }
}

public class AttachmentImageEntity
{
	public string AttachmentId { get; set; }
	public string AttachmentBase64 { get; set; }
}

public class AttachmentExternal
{
	[Required]
	[MaxLength(100)]
	[Description("Entity")]
	[JsonProperty(PropertyName = "Entity")]
	[DefaultMappingEntity("Entity")]
	public string Entity { get; set; }
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Procedure Code format.")]
	public string Code { get; set; }
	[MaxLength(200)]
	public string Aux1 { get; set; }
	[MaxLength(200)]
	public string Aux2 { get; set; }
	[MaxLength(200)]
	public string Aux3 { get; set; }
	[MaxLength(200)]
	public string Aux4 { get; set; }
	[MaxLength(200)]
	public string AttachmentId { get; set; }
	[MaxLength(300)]
	public string AttachmentName { get; set; }
	[MaxLength(200)]
	public string AttachmentExtension { get; set; }
}
public class AttachmentExternalResponse
{
	public string Entity { get; set; }
	public string EntityType { get; set; }
	public string AuxId { get; set; }
	public string Code { get; set; }
	public string Aux1 { get; set; }
	public string Aux2 { get; set; }
	public string Aux3 { get; set; }
	public string Aux4 { get; set; }
	public string AttachmentIdExternal { get; set; }
	public string AttachmentName { get; set; }
	public string AttachmentExtension { get; set; }
}
