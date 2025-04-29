using System.Data.Common;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Configuration;

namespace EWP.SF.Helper;

/// <summary>
/// ApplicationSettings is a singleton class that provides access to application settings and connection strings.
/// </summary>
public interface IApplicationSettings
{
	/// <summary>
	/// Gets the connection string for the specified connection name.
	/// </summary>
	/// <param name="connectionName"></param>
	/// <returns></returns>
	string GetConnectionString(string connectionName = "Main");

	/// <summary>
	/// Gets the connection string for the reports database.
	/// </summary>
	/// <returns></returns>
	string GetReportsConnectionString();

	/// <summary>
	/// Gets the application setting value for the specified setting name.
	/// </summary>
	/// <param name="settingName"></param>
	/// <returns></returns>
	string GetAppSetting(string settingName);

	/// <summary>
	/// Checks if the application settings contain a specific key.
	/// </summary>
	/// <param name="settingName"></param>
	/// <returns></returns>
	bool ContainsKey(string settingName);

	/// <summary>
	/// Gets the database name from the connection string for the specified connection name.
	/// </summary>
	/// <param name="connectionName"></param>
	/// <returns></returns>
	string GetDatabaseFromConnectionString(string connectionName = "Main");
}

/// <summary>
/// ApplicationSettings is a singleton class that provides access to application settings and connection strings.
/// </summary>
public class ApplicationSettings : IApplicationSettings
{
	private static readonly Lazy<ApplicationSettings> _instance = new(() => new ApplicationSettings());
	private readonly IConfiguration? _configuration;

	// Cached values (called once)
	private readonly string appName;
	private readonly string osKey;
	private readonly string repoUser;

	// Singleton instance for old code
	/// <summary>
	/// Gets the singleton instance of the <see cref="ApplicationSettings"/> class.
	/// </summary>
	public static ApplicationSettings Instance => _instance.Value;

	// Old code support: Direct instantiation without DI
	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
	/// </summary>
	public ApplicationSettings()
	{
		_configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.json")), optional: false, reloadOnChange: false)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
			.AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.ConnectionStrings.json")), optional: false, reloadOnChange: false)
			.AddJsonFile("appsettings.ConnectionStrings.json", optional: true, reloadOnChange: false)
			.Build();

		// Cache values during object construction
		appName = Common.GetProjectName();
		appName = string.IsNullOrEmpty(appName) ? "" : $"[{appName}]";
		repoUser = GitHelper.GetCurrentGitUser().ConfigureAwait(false).GetAwaiter().GetResult();
		repoUser = string.IsNullOrEmpty(repoUser) ? "" : $"[{repoUser}]";
		osKey = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Linux";
	}

	// New code: DI-based constructor
	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationSettings"/> class with the specified configuration.
	/// </summary>
	/// <param name="configuration"></param>
	public ApplicationSettings(IConfiguration configuration)
	{
		_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		// Cache values during object construction
		appName = Common.GetProjectName();
		appName = string.IsNullOrEmpty(appName) ? "" : $"[{appName}]";
		repoUser = GitHelper.GetCurrentGitUser().ConfigureAwait(false).GetAwaiter().GetResult();
		repoUser = string.IsNullOrEmpty(repoUser) ? "" : $"[{repoUser}]";
		osKey = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Linux";
	}

	/// <summary>
	/// Gets the connection string for the specified connection name.
	/// </summary>
	/// <param name="connectionName"></param>
	/// <returns></returns>
	public string GetConnectionString(string connectionName = "Main")
	{
		return _configuration?.GetConnectionString(connectionName)?
			.Replace("{App}", appName, StringComparison.InvariantCultureIgnoreCase)
			.Replace("{User}", repoUser, StringComparison.InvariantCultureIgnoreCase)
			?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");
	}

	/// <summary>
	/// Gets the connection string for the reports database.
	/// </summary>
	/// <returns></returns>
	public string GetReportsConnectionString() => GetConnectionString("Reports");

	/// <summary>
	/// Gets the application setting value for the specified setting name.
	/// </summary>
	/// <param name="settingName"></param>
	/// <returns></returns>
	public string GetAppSetting(string settingName)
	{
		string? value = _configuration?.GetSection("AppSettings")?[settingName];
		if (value is not null)
		{
			return value;
		}

		// If the setting is nested, handle OS-specific conditions
		IConfigurationSection? section = _configuration?.GetSection(settingName);
		if (section?.Exists() == true)
		{
			return section[osKey]
				?? section["Windows"]
				?? throw new KeyNotFoundException($"Setting '{settingName}' not found.");
		}

		throw new KeyNotFoundException($"Setting '{settingName}' not found.");
	}

	/// <summary>
	/// Checks if the application settings contain a specific key.
	/// </summary>
	/// <param name="settingName"></param>
	/// <returns></returns>
	public bool ContainsKey(string settingName) => _configuration?.GetSection("AppSettings")?.GetChildren().Any(x => x.Key == settingName) ?? false;

	/// <summary>
	/// Gets the database name from the connection string for the specified connection name.
	/// </summary>
	/// <param name="connectionName"></param>
	/// <returns></returns>
	public string GetDatabaseFromConnectionString(string connectionName = "Main")
	{
		string connectionString = GetConnectionString(connectionName);
		DbConnectionStringBuilder builder = new() { ConnectionString = connectionString };
		return builder["Database"]?.ToString()
			?? throw new InvalidOperationException($"Database parameter for '{connectionName}' not found.");
	}
}
