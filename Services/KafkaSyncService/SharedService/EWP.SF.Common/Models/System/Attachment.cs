using Newtonsoft.Json;

namespace EWP.SF.Common.Models;
/// <summary>
/// Attachment
/// </summary>
public class Attachment
{
	/// <summary>
	/// Gets or sets the name of the attachment.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the content type of the attachment.
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// Gets or sets the type of the attachment.
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	/// Gets or sets the user who created the attachment.
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	/// Gets or sets the date and time when the attachment was created.
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	/// Gets or sets the date and time when the attachment was last modified.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Gets or sets the date and time when the attachment was last modified.
	/// </summary>
	public string MimeType { get; set; }

	/// <summary>
	/// Gets or sets the unique identifier for the attachment.
	/// </summary>
	public string AuxId { get; set; }

	/// <summary>
	/// Gets or sets the file extension for the attachment.
	/// </summary>
	public string Extension { get; set; }

	/// <summary>
	/// Gets or sets the content type of the attachment.
	/// </summary>
	public int Size { get; set; }

	/// <summary>
	/// Gets or sets the type of the attachment.
	/// </summary>
	[JsonIgnore]
	public byte[] Buffer { get; set; }

	/// <summary>
	/// Gets or sets the string representation of the attachment buffer.
	/// </summary>
	public string BufferString { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the attachment is new and current.
	/// </summary>
	public bool IsNewCurrent { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the attachment is a main picture.
	/// </summary>
	public bool MainPicture { get; set; }
}

/// <summary>
/// Represents the type of the attachment.
/// </summary>
public enum AttachmentType
{
	/// <summary>
	/// Represents a license attachment.
	/// </summary>
	License,
	/// <summary>
	/// Represents a customer attachment.
	/// </summary>
	Customer,
	/// <summary>
	/// Represents a knowledge base attachment.
	/// </summary>
	KnowledgeBase,
	/// <summary>
	/// Represents a solution attachment.
	/// </summary>
	Solution,
	/// <summary>
	/// Represents a document attachment.
	/// </summary>
	Document,
	/// <summary>
	/// Represents a file attachment.
	/// </summary>
	Other
}
