using System;

[Serializable]
public class ScenarioStageDialogueData : ScenarioStageData
{
    public override STAGE_TYPE stageType => STAGE_TYPE.DIALOGUE;
    public string characterSpriteId;
}
