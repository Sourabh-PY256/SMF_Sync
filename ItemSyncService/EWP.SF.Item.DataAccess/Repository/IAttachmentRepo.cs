using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IAttachmentRepo {
    Task<AttachmentResponse> AttachmentPut(AttachmentLocal attachment, User systemOperator);
    Task<string> AttachmentSync(string AttachmentId, string AuxId, User systemOperator);
    
}
