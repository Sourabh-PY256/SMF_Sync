

using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class ResponseInstruction
{
	/// <summary>
	///
	/// </summary>
	public string InstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcedureId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ResponseObject { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ArrayIndexSections { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SectionCurrentIndex { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProcedureSection SectionCurrent { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<InstructionInfo> ArrayIdInstrucctionCurrent { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsFinish { get; set; }
}

/// <summary>
///
/// </summary>
public class InstructionInfo
{
	/// <summary>
	///
	/// </summary>
	public string Response { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int AnswerStep { get; set; }
}
/// <summary>
///
/// </summary>
public class RequestSaveInstructionResponse
{
	/// <summary>
	///
	/// </summary>
	public string InstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcedureId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ResponseObject { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ArrayIndexSections { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SectionCurrentIndex { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TaskId { get; set; }

	/// <summary>
	///
	/// </summary>
	public Section SectionCurrent { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<InstructionInfo> ArrayIdInstrucctionCurrent { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsFinish { get; set; }
}
/// <summary>
///
/// </summary>
public class ResponseSaveInstructionResponse
{
	/// <summary>
	///
	/// </summary>
	public bool IsSuccess { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MessageError { get; set; }

	/// <summary>
	///
	/// </summary>
	public TaskResponse Instance { get; set; }
}
