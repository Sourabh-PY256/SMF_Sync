using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models;

namespace EWP.SF.Common.DataAccess;

public interface IValidateEntityRepository
{ 
    void ValidateEntityCode(string key, string entity, User systemOperator);
}