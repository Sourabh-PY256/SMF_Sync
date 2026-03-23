using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IComponentOperation
{
    Component GetComponentByCode(string Code);

    /// <summary>Unified entry point for UI — normalizes then merges.</summary>
    Task<ResponseData> ProcessProduct(ActionDB mode, Component component, User systemOperator);

    /// <summary>Unified entry point for ERP/DataSync — converts, normalizes, then merges.</summary>
    Task<ResponseData> ProcessProduct(ActionDB mode, ProductExternal externalProduct, User systemOperator);

    /// <summary>Core merge — call ProcessProduct instead for new work.</summary>
    Task<ResponseData> MergeProduct(ActionDB mode, Component componentInfo, User systemOperator, bool Validate = false, LevelMessage Level = LevelMessage.Success, bool NotifyOnce = true, bool isNewVersion = false, bool isExternalEndpoint = false, IntegrationSource intSource = IntegrationSource.SF);

    /// <summary>
    /// Bulk sync entry point. Accepts pre-converted <see cref="Component"/> list
    /// (caller converts ProductExternal → Component before calling this).
    /// Applies the same NormalizeComponent rules as the UI path for each item.
    /// </summary>
    Task<List<ResponseData>> ListUpdateProduct(List<Component> itemList, List<Component> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level);

    Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "");
    Task<List<ProcessEntry>> GetProcessEntryById(string processEntryId, User systemOperator);
    Task<List<ProcessEntry>> GetProcessEntry(string productCode, string warehouseId, int? version, int? sequence, User systemOperator);

}