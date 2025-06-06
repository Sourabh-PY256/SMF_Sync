using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.Text;
using Range = EWP.SF.Common.Models.Range;
using SixLabors.ImageSharp;
using EWP.SF.Common.Models.Catalogs;

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
