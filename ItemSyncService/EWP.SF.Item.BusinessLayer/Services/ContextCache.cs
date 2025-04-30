using System.Collections.Concurrent;

using EWP.SF.Item.BusinessEntities;
using EWP.SF.Helper;
using EWP.SF.Common.Models;
using System.Runtime.Caching;
using EWP.SF.Common.Enumerators;
namespace EWP.SF.Item.BusinessLayer;

public static class ContextCache
{
	public static double? ERPOffset;
	public static List<string> ProcessingDevices = [];
	public static Dictionary<string, string> IncomingContext = [];
	public static List<SyncContext> SyncContext = [];
	private static readonly List<KeyValuePair<int, string>> UserIndex = [];
	private static readonly List<KeyValuePair<int, string>> RoleIndex = [];
	private static readonly List<KeyValuePair<string, string>> HashIndex = [];
	private static readonly ConcurrentDictionary<string, bool> _runningServicesLocal = new();

	public static event EventHandler<CacheItem> OnDelete;

	public static bool IsServiceRunning(string serviceName)
	{
		bool returnValue = false;
		if (_runningServicesLocal.TryGetValue(serviceName, out bool value))
		{
			returnValue = value;
		}
		return returnValue;
	}

	public static void SetRunningService(string serviceName, bool status)
	{
		_ = _runningServicesLocal.AddOrUpdate(serviceName, status, (k, o) => status);
	}

	public static RequestContext GetObject(string key)
	{
		return MemoryCache.Default.GetCacheItem(key, null) is not null ? (RequestContext)((ObjectCache)MemoryCache.Default)[key] : null;
	}

	public static void RemoveContext(string type, int id)
	{
		if (type == "U")
		{
			RemoveByUserId(id);
		}
		if (type == "R")
		{
			RemoveByRole(id);
		}
	}

	public static bool HasKey(string key)
	{
		return MemoryCache.Default.GetCacheItem(key, null) is not null;
	}

	public static bool RemoveObject(string key, bool noReturn = false)
	{
		try
		{
			ObjectCache cache = MemoryCache.Default;

			RequestContext cachedObject = (RequestContext)cache[key];

			if (noReturn)
			{
				cachedObject.User.Status = Status.Deleted;
			}

			if (cachedObject is not null)
			{
				_ = cache.Remove(key);
			}
		}
		catch
		{
		}

		return true;
	}

	public static void RemoveByUserId(int Id)
	{
		foreach (KeyValuePair<int, string> item in new List<KeyValuePair<int, string>>(UserIndex.Where(x => x.Key == Id)))
		{
			_ = RemoveObject(item.Value, true);
		}
	}

	public static void RemoveByRole(int Id)
	{
		foreach (KeyValuePair<int, string> item in RoleIndex.Where(x => x.Key == Id))
		{
			_ = RemoveObject(item.Value, true);
		}
	}

	public static void RemoveByHash(string Hash)
	{
		foreach (KeyValuePair<string, string> item in HashIndex.Where(x => x.Key == Hash))
		{
			_ = RemoveObject(item.Value, true);
		}
	}

	public static void SetObjectToCache(string cacheItemName, RequestContext obj, long expireTime)
	{
		ObjectCache cache = MemoryCache.Default;

		RequestContext cachedObject = (RequestContext)cache[cacheItemName];

		if (cachedObject is not null)
		{
			_ = cache.Remove(cacheItemName);
		}

		CacheItemPolicy policy = new()
		{
			AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(expireTime),
			RemovedCallback = new CacheEntryRemovedCallback(CacheRemovedCallback),
		};

		cachedObject = obj;
		string hash = obj.Token.Md5();
		cache.Set(cacheItemName, cachedObject, policy);
		if (obj.User?.Id is not null && !UserIndex.Any(x => x.Key == obj.User.Id && x.Value == obj.Token))
		{
			UserIndex.Add(new KeyValuePair<int, string>(obj.User.Id, obj.Token));
		}

		if (obj.User?.Role?.Id is not null && !RoleIndex.Any(x => x.Key == obj.User.Role.Id && x.Value == obj.Token))
		{
			RoleIndex.Add(new KeyValuePair<int, string>(obj.User.Role.Id, obj.Token));
		}

		if (!HashIndex.Any(x => x.Key == hash && x.Value == obj.Token))
		{
			HashIndex.Add(new KeyValuePair<string, string>(hash, obj.Token));
		}
	}

	public static void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
	{
		_ = UserIndex.RemoveAll(x => x.Value == arguments.CacheItem.Key);
		_ = RoleIndex.RemoveAll(x => x.Value == arguments.CacheItem.Key);
		_ = HashIndex.RemoveAll(x => x.Value == arguments.CacheItem.Key);

		OnDelete?.Invoke(arguments, arguments.CacheItem);
	}
}
