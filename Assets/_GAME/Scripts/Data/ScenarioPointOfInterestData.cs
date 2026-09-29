using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScenarioPointOfInterestData
{
    public string id;
    public string title;
    [TextArea] public string description;
    public float latitude;
    public float longitude;
    public int showAtProgressPercent;
    public int moneyReward;
    public int honorReward;
    public int legendReward;
    public List<DecisionOption> decisions = new List<DecisionOption>();

    [Serializable]
    public class DecisionOption
    {
        public string optionText;
        public int moneyReward;
        public int honorReward;
        public int legendReward;
    }
}
