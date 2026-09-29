using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public List<SkillData> Skills => ScenarioManager.CurrentScenario.skills;
    public List<string> ActivatedSkills => SaveLoad.CurrentSaveInfo.unlockedSkills;

    public static SkillStatus GetSkillStatus(string skillId)
    {
        if (string.IsNullOrEmpty(skillId))
        {
            Debug.LogError("Invalid skill ID");
            return SkillStatus.Locked;
        }

        if (Instance.ActivatedSkills.Contains(skillId))
            return SkillStatus.Active;

        if(ScenarioManager.CurrentStage.stageType == ScenarioStageData.STAGE_TYPE.UNLOCK_SKILL)
        {
            var unlockStage = ScenarioManager.CurrentStage as ScenarioStageUnlockSkillData;

            if (unlockStage != null && unlockStage.skillId == skillId)
                return SkillStatus.Available;
        }

        return SkillStatus.Locked;
    }

    private void Awake()
    {
        Instance = this;
    }

    public enum SkillStatus
    {
        Locked,
        Available,
        Active
    }
}
