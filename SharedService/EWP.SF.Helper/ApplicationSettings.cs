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
	string GetConnectionString(string connectionName = "Main");

	/// <summary>
	/// Gets the connection string for the reports database.
	/// </summary>
	string GetReportsConnectionString();

	/// <summary>
	/// Gets the application setting value for the specified setting name.
	/// </summary>
	string GetAppSetting(string settingName);

	/// <summary>
	/// Checks if the application settings contain a specific key.
	/// </summary>
	bool ContainsKey(string settingName);

	/// <summary>
	/// Gets the database name from the connection string for the specified connection name.
	/// </summary>
	string GetDatabaseFromConnectionString(string connectionName = "Main");
}

/// <summary>
/// ApplicationSettings is a singleton class that provides access to application settings and connection strings.
/// </summary>
public class ApplicationSettings : IApplicationSettings
{
	private static readonly Lazy<ApplicationSettings> _instance = new(static () => new ApplicationSettings());
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
#if DEBUG
		string appDirectory = Directory.GetCurrentDirectory();
#else
		string appDirectory = AppContext.BaseDirectory;
#endif
		string settingsDirectory1 = Path.GetFullPath(Path.Combine(appDirectory, "..", "Settings"));
		string settingsDirectory2 = Path.GetFullPath(Path.Combine(appDirectory, "..", "..", "Settings"));

		string settingsDirectory;
		if (Directory.Exists(settingsDirectory1))
		{
			settingsDirectory = settingsDirectory1;
		}
		else if (Directory.Exists(settingsDirectory2))
		{
			settingsDirectory = settingsDirectory2;
		}
		else
		{
			throw new DirectoryNotFoundException("Settings directory not found in either location.");
		}

		_configuration = new ConfigurationBuilder()
			.SetBasePath(appDirectory)
			.AddJsonFile(Path.Combine(settingsDirectory, "appsettings.json"), optional: false, reloadOnChange: false)
			.AddJsonFile(Path.Combine(appDirectory, "appsettings.json"), optional: true, reloadOnChange: false)
			.AddJsonFile(Path.Combine(settingsDirectory, "appsettings.ConnectionStrings.json"), optional: false, reloadOnChange: false)
			.AddJsonFile(Path.Combine(appDirectory, "appsettings.ConnectionStrings.json"), optional: true, reloadOnChange: false)
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
	/// <exception cref="InvalidOperationException"></exception>
	public string GetConnectionString(string connectionName = "Main")
	{
		return _configuration?.GetConnectionString(connectionName)?
			.Replace("{App}", appName, StringComparison.InvariantCultureIgnoreCase)
			.Replace("{User}", repoUser, StringComparison.InvariantCultureIgnoreCase) ??
			throw new InvalidOperationException($"Connection string '{connectionName}' not found.");
	}

	/// <summary>
	/// Gets the connection string for the reports database.
	/// </summary>
	public string GetReportsConnectionString() => GetConnectionString("Reports");

	/// <summary>
	/// Gets the application setting value for the specified setting name.
	/// </summary>
	/// <exception cref="KeyNotFoundException"></exception>
	public string GetAppSetting(string settingName)
	{
		string? value = _configuration?.GetSection("AppSettings")?[settingName];
		if (value is not null)
		{
			return value;
		}

		// If the setting is nested, handle OS-specific conditions
		IConfigurationSection? section = _configuration?.GetSection(settingName);
		return section?.Exists() == true ?
			section[osKey] ??
				section["Windows"] ??
				throw new KeyNotFoundException($"Setting '{settingName}' not found.") :
			throw new KeyNotFoundException($"Setting '{settingName}' not found.");
	}

	/// <summary>
	/// Gets the application setting value for the specified setting name.
	/// </summary>
	/// <exception cref="KeyNotFoundException"></exception>
	public IConfigurationSection? GetSection(string settingName)
	{
		return _configuration?.GetSection(settingName);
	}

	/// <summary>
	/// Checks if the application settings contain a specific key.
	/// </summary>
	public bool ContainsKey(string settingName) => _configuration?.GetSection("AppSettings")?.GetChildren().Any(x => x.Key == settingName) ?? false;

	/// <summary>
	/// Gets the database name from the connection string for the specified connection name.
	/// </summary>
	/// <exception cref="InvalidOperationException"></exception>
	public string GetDatabaseFromConnectionString(string connectionName = "Main")
	{
		string connectionString = GetConnectionString(connectionName);
		DbConnectionStringBuilder builder = new() { ConnectionString = connectionString };
		return builder["Database"]?.ToString() ?? throw new InvalidOperationException($"Database parameter for '{connectionName}' not found.");
	}
}
