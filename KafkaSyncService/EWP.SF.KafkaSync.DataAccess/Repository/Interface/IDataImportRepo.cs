using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;

/// Interface for managing work center data access operations
/// </summary>
public interface IDataImportRepo
{
    Task<List<Entity>> ListEntities();
    ResponseAttachment GetAttachment(string IdAttachment, string AuxId);
}