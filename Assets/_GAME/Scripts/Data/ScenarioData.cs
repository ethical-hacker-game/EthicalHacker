using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScenarioData
{
    public string id;
    public string contentPackId;
    public string name;
    [TextArea] public string description;
    public List<ScenarioStageData> stages = new List<ScenarioStageData>();
    public List<SkillData> skills = new List<SkillData>();
    public List<ScenarioPointOfInterestData> pointsOfInterest = new List<ScenarioPointOfInterestData>();
}
