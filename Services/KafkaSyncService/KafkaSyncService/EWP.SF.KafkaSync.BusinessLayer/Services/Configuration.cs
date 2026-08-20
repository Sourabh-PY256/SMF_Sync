using EWP.SF.KafkaSync.DataAccess;
namespace EWP.SF.KafkaSync.BusinessLayer;

public class Configuration : Dictionary<string, string>
{
	private readonly DataAccess.SecurityManager DAL;

	public Configuration(bool skip = false)
	{
		DAL = new DataAccess.SecurityManager();
		if (!skip)
		{
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
			Refresh().ConfigureAwait(false).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
		}
	}

	public async Task Refresh()
	{
		Clear();
		List<KeyValuePair<string, string>> values = await DAL.GetConfiguration().ConfigureAwait(false);
		values?.ForEach(x =>
		{
			if (!ContainsKey(x.Key))
			{
				Add(x.Key, x.Value);
			}
		});
	}

	public void UpdateConfiguration(string key, string value)
	{
		DAL.UpdateConfiguration(key, value);
		if (ContainsKey(key))
		{
			this[key] = value;
		}
		else
		{
			Add(key, value);
		}
	}

	public async Task UpdateCompleted(string key, bool completedTab, bool completedQuestion, string notes)
	{
		await DAL.UpdateConfigurationTab(key, completedTab, completedQuestion, notes).ConfigureAwait(false);
	}

	public async Task UpdateAnswer(string Id, string questionId, string answer)
	{
		await DAL.UpdateConfigurationAnswer(Id, questionId, answer).ConfigureAwait(false);
	}
}

public static class Config
{
	private static Configuration _configuration;

	public static Configuration Configuration
	{
		get
		{
			_configuration ??= [];
			return _configuration;
		}

		set => _configuration = [];
	}

	public static Configuration SkipConfiguration
	{
		get
		{
			_configuration ??= new Configuration(true);
			return _configuration;
		}

		set => _configuration = new Configuration(true);
	}
}
