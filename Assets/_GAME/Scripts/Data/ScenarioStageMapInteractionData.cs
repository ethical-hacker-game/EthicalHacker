using System;

[Serializable]
public class ScenarioStageMapInteractionData : ScenarioStageData
{
    public override STAGE_TYPE stageType => STAGE_TYPE.MAP_INTERACTION;
    public float latitude;
    public float longitude;
    public string locationId;
    public string iconId;
}
