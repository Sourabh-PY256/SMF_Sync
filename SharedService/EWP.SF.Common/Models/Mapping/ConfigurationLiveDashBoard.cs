namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class ConfigurationLiveDashBoard
{
	/// <summary>
	///
	/// </summary>
	public string IdConfigurationLiveDashBoard { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DataLayoutId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Machines { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLineId { get; set; }
}

/// <summary>
///
/// </summary>
public class ConfigurationLiveDashboardVM
{
	/// <summary>
	///
	/// </summary>
	public string DataLayoutId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLineId { get; set; }
}

/// <summary>
///
/// </summary>
public class ConfigurationAssetLiveDashBoard
{
	/// <summary>
	///
	/// </summary>
	public string Layout { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetCode { get; set; }
}
