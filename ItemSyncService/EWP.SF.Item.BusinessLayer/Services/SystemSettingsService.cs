using EWP.SF.Item.DataAccess;

namespace EWP.SF.Item.BusinessLayer;

public class SystemSettingsService(IUtilitiesRepository utilitiesRepository) : ISystemSettingsService
{

  public Task<string> GetTableChecksum(string tableName)
  {
    return utilitiesRepository.GetChecksumTable(tableName);
  }

  public Task<DateTime> GetServerTimeUTC()
  {
    return utilitiesRepository.GetServerUTCTime();
  }
}
