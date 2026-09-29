using System;
using UnityEngine;

[Serializable]
public abstract class ScenarioStageData 
{
    public string id;
    public string characterName;
    [TextArea] public string stageText;
    public abstract STAGE_TYPE stageType { get; }

    [Serializable]
    public enum STAGE_TYPE
    {
        DIALOGUE,
        DECISION,
        QUESTION,
        UNLOCK_SKILL,
        MAP_INTERACTION
    }
}
