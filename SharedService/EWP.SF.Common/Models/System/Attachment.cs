using Newtonsoft.Json;

namespace EWP.SF.Common.Models;

public class Attachment
{
	public string Name { get; set; }
	public string Path { get; set; }

	//public AttachmentType Type { get; set; }

	public string Type { get; set; }

	public User CreatedBy { get; set; }
	public DateTime CreationDate { get; set; }

	public string Id { get; set; }
	public string MimeType { get; set; }
	public string AuxId { get; set; }
	public string Extension { get; set; }

	public int Size { get; set; }

	[JsonIgnore]
	public byte[] Buffer { get; set; }

	public string BufferString { get; set; }
	public bool IsNewCurrent { get; set; }
	public bool MainPicture { get; set; }
}

public enum AttachmentType
{
	License,
	Customer,
	KnowledgeBase,
	Solution,
	Document,
	Other
}
