using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface ISkillRepo

{
    List<Skill> ListAllSkills(DateTime? DeltaDate = null);

    List<Skill> ListMachineSkills(string machineId = "");
    bool SaveMachineSkills(string machineId, List<string> skillIds, User systemOperator);

    
}