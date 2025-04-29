namespace EWP.SF.Common.Models;

public class ConfigurationLiveDashBoard
{
	public string IdConfigurationLiveDashBoard { get; set; }
	public string DataLayoutId { get; set; }
	public string Machines { get; set; }
	public string ProductionLineId { get; set; }
}

public class ConfigurationLiveDashboardVM
{
	public string DataLayoutId { get; set; }
	public string ProductionLineId { get; set; }
}

public class ConfigurationAssetLiveDashBoard
{
	public string Layout { get; set; }
	public string AssetType { get; set; }
	public string AssetCode { get; set; }
}
