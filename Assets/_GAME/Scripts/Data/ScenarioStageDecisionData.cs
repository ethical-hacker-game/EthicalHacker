using System;
using System.Collections.Generic;

[Serializable]
public class ScenarioStageDecisionData : ScenarioStageData
{
    public override STAGE_TYPE stageType => STAGE_TYPE.DECISION;
    public List<DecisionOption> options = new List<DecisionOption>();

    [Serializable]
    public class DecisionOption
    {
        public string optionText;
        public int moneyReward;
        public int honorReward;
    }
}
