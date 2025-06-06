namespace EWP.SF.Common.Models.IntegrationStaging;

/// <summary>
///
/// </summary>
public class ResponseSchedule
{
	/// <summary>
	///
	/// </summary>
	public string Facility { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationName { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OpNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Resource { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime SetupStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal TotalSetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal TotalProcessTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime OrderStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime OrderEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Origin { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Product { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DescProduct { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Efficiency { get; set; }
}
