using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveInfo
{
    public string slotId;
    public string scenarioName;
    public string saveDateTime;

    public string sourceTypeName;
    public string sourceBaseUrl;
    public string sourceContentPackId;
    public string sourceScenarioId;

    public int statMoney;
    public int statHonor;
    public int statLegend;

    public int stageIndex;
    public string stageId;

    public List<string> unlockedSkills = new List<string>();
    public List<DecisionInfo> decisions = new List<DecisionInfo>();
    public List<PointOfInterestDecisionInfo> pointOfInterestDecisions = new List<PointOfInterestDecisionInfo>();

    public int ProgressPercent
    {
        get
        {
            int totalStages = ScenarioManager.CurrentScenario != null ? ScenarioManager.CurrentScenario.stages.Count : 0;

            int currentStage = stageIndex + 1;
            currentStage = Mathf.Clamp(currentStage, 0, totalStages);

            int progressPercent = totalStages > 0
                ? Mathf.FloorToInt((currentStage / (float)totalStages) * 100f)
                : 0;

            return progressPercent;
        }
    }


    [Serializable]
    public class DecisionInfo
    {
        public string stageId;
        public string stageText;
        public int optionIndex;
        public int optionsCount;
        public string optionText;

        public string FullText => $"{stageText}\n\n{optionText}";
    }

    [Serializable]
    public class PointOfInterestDecisionInfo
    {
        public string poiId;
        public string title;
        public string description;
        public int optionIndex;
        public int optionsCount;
        public string optionText;
    }
}
