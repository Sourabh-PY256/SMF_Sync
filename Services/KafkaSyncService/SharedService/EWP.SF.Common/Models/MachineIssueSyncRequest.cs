using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace EWP.SF.Common.Models;

/// <summary>
/// Model for Machine Issue synchronization, matching the specific JSON format provided by the user.
/// </summary>
public class MachineIssueSyncRequest
{
    [JsonProperty("Details")]
    public List<MachineIssueDetail> Details { get; set; }

    [JsonProperty("TransactionId")]
    public string TransactionId { get; set; }

    [JsonProperty("DocCode")]
    public string DocCode { get; set; }

    [JsonProperty("Comments")]
    public string Comments { get; set; }

    [JsonProperty("OrderCode")]
    public string OrderCode { get; set; }

    [JsonProperty("OperationNo")]
    public double OperationNo { get; set; }

    [JsonProperty("DocDate")]
    public DateTime DocDate { get; set; }
}

/// <summary>
/// Detail for Machine Issue
/// </summary>
public class MachineIssueDetail
{
    [JsonProperty("MachineCode")]
    public string MachineCode { get; set; }

    [JsonProperty("Time")]
    public double Time { get; set; }
}
