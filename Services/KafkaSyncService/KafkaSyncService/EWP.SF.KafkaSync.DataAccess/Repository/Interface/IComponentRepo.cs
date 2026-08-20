using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IComponentRepo
{
    Task<List<Component>> ListComponents(string componentId, bool ignoreImages = false, string filter = "", DateTime? DeltaDate = null, CancellationToken cancellationToken = default);
    Task<List<Component>> ListProducts(string id, string warehouseCode = "", DateTime? deltaDate = null, CancellationToken cancel = default);

    ResponseData MergeComponent(Component componentInfo, User systemOperator, bool Validation);
    ResponseData MergeProduct(Component componentInfo, User systemOperator, bool Validation, LevelMessage Level);
    Component GetComponentByCode(string Code);
    bool MergeProcessEntryAttributes(string processEntryId, string JSON, User systemOperator);
    Task<List<ProcessEntry>> ListProcessEntry(string code, string warehouse, int? version, int? sequence = 0, string id = "", CancellationToken cancel = default);
    bool MergeProcessEntryTools(string processEntryId, string JSON, User systemOperator);
    bool MergeProcessEntryLabor(string processEntryId, string JSON, User systemOperator);
    bool SaveProductDetails(ProcessEntry entryInfo, string OperationsJSON, string MaterialJSON, string AlternativesJSON, string SubproductsJSON, User systemOperator);
    ProcessEntry CreateProcessEntry(ProcessEntry entryInfo, User systemOperator, IntegrationSource intSrc = IntegrationSource.SF);
    int VerifyProductVersion(ProcessEntry entryInfo);
    bool UpdateProcessEntry(ProcessEntry entryInfo, User systemOperator);
    int GetNextProductVersion(ProcessEntry entryInfo);
}
