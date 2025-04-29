namespace EWP.SF.Item.BusinessEntities;

public enum MessageBrokerType
{
	TagData = 17,
	Downtime = 0,
	SensorData = 1,
	MachineUpdate = 2,
	Alert = 3,
	Email = 4,
	Permission = 5,
	WorkOrder = 6,
	ProcessStatus = 7,
	ActivitySchedule = 8,
	ManualProductIsssue = 9,
	ManualMaterialIssue = 10,
	ActivityExecuted = 11,
	TriggerPrint = 12,
	ExternalProductReceipt = 13,
	ExternalMaterialIssue = 14,
	CatalogChanged = 15,
	SecondaryContrainstGroup = 16,
	ResourceIssue = 19,
	taskChanged = 18
}
