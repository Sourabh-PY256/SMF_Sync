

namespace EWP.SF.ShopFloor.BusinessEntities;

public class ResponseInstruction
{
	public string InstanceId { get; set; }
	public string ProcedureId { get; set; }
	public int UserId { get; set; }
	public string ResponseObject { get; set; }
	public string ArrayIndexSections { get; set; }
	public int SectionCurrentIndex { get; set; }
	public string EmployeeId { get; set; }
	public string SectionId { get; set; }

	public ProcedureSection SectionCurrent { get; set; }
	public List<InstructionInfo> ArrayIdInstrucctionCurrent { get; set; }
	public bool IsFinish { get; set; }
}

public class InstructionInfo
{
	public string Response { get; set; }
	public string InstructionId { get; set; }
	public int AnswerStep { get; set; }
}
public class RequestSaveInstructionResponse
{
	public string InstanceId { get; set; }
	public string ProcedureId { get; set; }
	public int UserId { get; set; }
	public string ResponseObject { get; set; }
	public string ArrayIndexSections { get; set; }
	public int SectionCurrentIndex { get; set; }
	public string EmployeeId { get; set; }
	public string SectionId { get; set; }

	public string TaskId { get; set; }

	public Section SectionCurrent { get; set; }
	public List<InstructionInfo> ArrayIdInstrucctionCurrent { get; set; }
	public bool IsFinish { get; set; }
}
public class ResponseSaveInstructionResponse
{
	public bool IsSuccess { get; set; }
	public string MessageError { get; set; }
	public TaskResponse Instance { get; set; }
}
