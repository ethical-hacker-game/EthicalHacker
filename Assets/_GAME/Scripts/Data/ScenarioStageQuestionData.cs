using System;
using System.Collections.Generic;

[Serializable]
public class ScenarioStageQuestionData : ScenarioStageData
{
    public override STAGE_TYPE stageType => STAGE_TYPE.QUESTION;
    public int moneyReward;
    public int legendReward;
    public int correctAnswerIndex;
    public List<string> answers = new List<string>();
    public string incorrectAnswerText;
}
