using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models;

public class MoveEntryExternal
{
    [Key]
    [Required]
    [MaxLength(100)]
    [Description("Doc Code")]
    [JsonProperty(PropertyName = "DocCode")]
    public string DocCode { get; set; }

    [MaxLength(500)]
    [Description("Comments")]
    [JsonProperty(PropertyName = "Comments")]
    public string Comments { get; set; }

    [MaxLength(100)]
    [Description("Employee ID")]
    [JsonProperty(PropertyName = "EmployeeID")]
    public string EmployeeID { get; set; }

    [Required]
    [MaxLength(100)]
    [Description("OrderCode")]
    [JsonProperty(PropertyName = "OrderCode")]
    public string OrderCode { get; set; }

    [Required]
    [Description("OperationNo")]
    [JsonProperty(PropertyName = "OperationNo")]
    public double OperationNo { get; set; }

    [Description("Doc Date")]
    [JsonProperty(PropertyName = "DocDate")]
    public DateTime DocDate { get; set; }

    [Required]
    [Description("Items")]
    [JsonProperty(PropertyName = "Items")]
    public List<MoveEntryExternalItem> Items { get; set; }
}

public class MoveEntryExternalItem
{
    [Required]
    [MaxLength(100)]
    [Description("Material")]
    [JsonProperty(PropertyName = "Material")]
    public string Material { get; set; }

    [Required]
    [Description("Quantity")]
    [JsonProperty(PropertyName = "Quantity")]
    public double Quantity { get; set; }
    
    [Description("Lots")]
    [JsonProperty(PropertyName = "Lots")]
    public List<MoveEntryExternalItemLot> Lots { get; set; }
}

public class MoveEntryExternalItemLot
{
    [Required]
    [MaxLength(100)]
    [Description("Lot No")]
    [JsonProperty(PropertyName = "LotNo")]
    public string LotNo { get; set; }

    [Required]
    [Description("Quantity")]
    [JsonProperty(PropertyName = "Quantity")]
    public double Quantity { get; set; }
}
