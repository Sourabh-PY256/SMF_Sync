using EWP.SF.ShopFloor.DataAccess;

namespace EWP.SF.ShopFloor.BusinessLayer;

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
