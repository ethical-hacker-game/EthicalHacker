using System;

[Serializable]
public class ScenarioStageUnlockSkillData : ScenarioStageData
{
    public override STAGE_TYPE stageType => STAGE_TYPE.UNLOCK_SKILL;
    public string skillId;
}
