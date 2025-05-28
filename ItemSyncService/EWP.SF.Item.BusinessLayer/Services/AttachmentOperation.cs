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
public class AttachmentOperation : IAttachmentOperation
{
    #if RELEASE
    private string FullAttachmentPath => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Config.Configuration["PathAttachment"]));
#else
    private string FullAttachmentPath => _applicationSettings.GetAppSetting("DebugAttachmentPath").ToStr();
#endif
    private readonly IAttachmentRepo _attachmentRepo;
    private readonly IApplicationSettings _applicationSettings;

    public AttachmentOperation(IattachmentRepo attachmentRepo, IApplicationSettings applicationSettings)
    {
        _attachmentRepo = attachmentRepo;
        _applicationSettings = applicationSettings;
    }


    #region Attachment
    public async Task<List<AttachmentResponse>> SaveAttachment(List<AttachmentLocal> listAttachmentRequest, User systemOperator)
    {
        List<AttachmentResponse> returnValue = [];

        try
        {
            foreach (AttachmentLocal attachmentRequest in listAttachmentRequest)
            {
                // Insert or update the attachment in the database
                AttachmentResponse tempAdd = await _attachmentRepo.AttachmentPut(attachmentRequest, systemOperator).ConfigureAwait(false);

                if ((attachmentRequest.TypeCode == "File" || attachmentRequest.TypeCode == "Image") &&
                    !string.IsNullOrEmpty(attachmentRequest.FileBase64))
                {
                    // Determine the attachment path
                    string relativePath = attachmentRequest.IsTemp
                        ? Path.Combine(FullAttachmentPath, "Temp")
                        : FullAttachmentPath;

                    // Ensure the directory exists
                    if (!Directory.Exists(relativePath))
                    {
                        Directory.CreateDirectory(relativePath);
                    }

                    // Write the file
                    string filePath = Path.Combine(relativePath, tempAdd.Id);
                    byte[] imageBytes = Convert.FromBase64String(attachmentRequest.FileBase64);

                    await File.WriteAllBytesAsync(filePath, imageBytes).ConfigureAwait(false);
                }

                // Update response properties
                tempAdd.Name = attachmentRequest.Name;
                tempAdd.Size = attachmentRequest.Size;
                tempAdd.IsTemp = attachmentRequest.IsTemp;
                tempAdd.Extension = attachmentRequest.Extension;
                tempAdd.CreationDate = tempAdd.CreationDate;

                returnValue.Add(tempAdd);
            }
        }
        catch (Exception ex)
        {
            //logger.Error("An error occurred while saving attachments.", ex);check
            throw;
        }

        return returnValue;
    }

    public async Task<bool> AttachmentSync(string AttachmentId, string AuxId, User systemOperator)
    {
        string ResultAttachmentId = await _attachmentRepo.AttachmentSync(AttachmentId, AuxId, systemOperator).ConfigureAwait(false);

        if (ResultAttachmentId is not null)
        {
            string pathFile = Path.Combine(FullAttachmentPath, AttachmentId);
            string pathTempFolder = Path.Combine(FullAttachmentPath, "Temp");
            string pathTempFile = Path.Combine(FullAttachmentPath, "Temp", AttachmentId);
            if (!Directory.Exists(FullAttachmentPath))
            {
                _ = Directory.CreateDirectory(FullAttachmentPath);
            }
            if (!Directory.Exists(pathTempFolder))
            {
                _ = Directory.CreateDirectory(pathTempFolder);
            }

            if (File.Exists(pathTempFile))
            {
                File.Move(pathTempFile, pathFile);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> SaveImageEntity(string Entity, string FileBase, string AuxId, User systemOperator)
    {
        if (string.IsNullOrEmpty(FileBase) || !FileBase.Contains("data:image"))
        {
            return false;
        }
        List<AttachmentLocal> listAttachmentRequest = [];
        try
        {
            byte[] imageBytes = Convert.FromBase64String(FileBase[(FileBase.LastIndexOf(',') + 1)..]);
            await using MemoryStream ms = new(imageBytes, 0, imageBytes.Length);
            using Image image = await Image.LoadAsync(ms).ConfigureAwait(false);
            AttachmentLocal objAdd = new()
            {
                TypeCode = "Image",
                Name = Entity + " " + AuxId,
                Extension = "jpg",
                FileBase64 = FileBase[(FileBase.LastIndexOf(',') + 1)..],
                AuxId = AuxId,
                Entity = Entity,
                IsTemp = false,
                Size = imageBytes.Length.ToString(),
                Status = 1,
                IsImageEntity = true,
            };

            listAttachmentRequest.Add(objAdd);
            _ = await SaveAttachment(listAttachmentRequest, systemOperator).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here if necessary
            return false;
        }
        return true;
    }
    #endregion Attachment
}
