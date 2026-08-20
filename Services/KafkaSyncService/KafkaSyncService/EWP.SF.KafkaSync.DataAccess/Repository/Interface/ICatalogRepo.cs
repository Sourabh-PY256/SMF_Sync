using EWP.SF.Common.Models;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface ICatalogRepo
{
    List<CatSkills> GetCatSkillsList(string skill = "", DateTime? DeltaDate = null);
    List<CatProfile> GetCatalogProfile(string profile = "", DateTime? DeltaDate = null);
    ResponseData MergeProfile(CatProfile catProfile, User systemOperator, bool Validation);
    List<Country> GetCountryList(string Code);
    

}