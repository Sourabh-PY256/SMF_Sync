namespace EWP.SF.KafkaSync.BusinessEntities;

public enum EnableType
{
	Yes = 1,
	No = 0
}

public enum TriggerType
{
	Time = 1,
	Erp = 2,
	SmartFactory = 3,
	OpenScreenSync = 4,
	ManualButtonSync = 5,
	DataSyncSettings = 6
}

public enum ServiceStatus
{
	Active = 1,
	Executing = 2,
	Disabled = 3
}

public enum DateTimeFormatType
{
	UTC = 1,
	TimeZone = 2
}

public enum TokenRenewOrigin
{
	Header = 1,
	Response = 2
}

public enum ServiceExecOrigin
{
	Timer = 1,
	//Webhook = 2,
	KafkaProducer = 2,
	Event = 3,
	SyncButton = 4
}
