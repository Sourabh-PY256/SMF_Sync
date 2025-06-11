using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IComponentOperation
{
    Component GetComponentByCode(string Code);
    Task<ResponseData> MergeProduct(ActionDB mode, Component componentInfo, User systemOperator, bool Validate = false, LevelMessage Level = LevelMessage.Success, bool NotifyOnce = true, bool isNewVersion = false, bool isExternalEndpoint = false, IntegrationSource intSource = IntegrationSource.SF);
    Task<List<ResponseData>> ListUpdateProduct(List<ProductExternal> itemList, List<ProductExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "");
    Task<List<ProcessEntry>> GetProcessEntryById(string processEntryId, User systemOperator);
    Task<List<ProcessEntry>> GetProcessEntry(string productCode, string warehouseId, int? version, int? sequence, User systemOperator);

}