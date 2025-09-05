using System.Collections.Generic;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Common.BusinessLayer;

public interface IValidateEntity
{
    void ValidateEntityKeys(ILoggableEntity objEntity, User user);
}