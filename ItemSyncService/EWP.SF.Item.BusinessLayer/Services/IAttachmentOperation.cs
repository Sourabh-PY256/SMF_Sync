
using EWP.SF.Common.Models;

namespace EWP.SF.Item.BusinessLayer;

public interface IAttachmentOperation
{
    Task<List<AttachmentResponse>> SaveAttachment(List<AttachmentLocal> listAttachmentRequest, User systemOperator);
    Task<bool> SaveImageEntity(string Entity, string FileBase, string AuxId, User systemOperator);
    Task<bool> AttachmentSync(string AttachmentId, string AuxId, User systemOperator);
    Task<List<AttachmentExternalResponse>> AttachmentSyncSel(List<AttachmentExternal> listAttachments,
     User systemOperator);
     Task<AttachmentResponse> SaveAttachmentExternal(AttachmentExternalResponse request, string FileBase64, User systemOperator);
    

}
