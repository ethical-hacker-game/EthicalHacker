using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIStageElement : MonoBehaviour
{
    [SerializeField] private TextPrinter textPrinter;
    [SerializeField] private GameObject npcImageParent;
    [SerializeField] private Image npcImage;
    [SerializeField] private ButtonImproved buttonContinue;

    [Space]
    [SerializeField] private Transform elementsParent;
    [SerializeField] private ButtonImproved buttonPrefab;

    private ScenarioStageData currentStageData;
    private ScenarioStageData.STAGE_TYPE stageType;

    private UnityAction onStageFinished;
    private readonly List<ButtonImproved> stageButtons = new List<ButtonImproved>();
    private string baseStageText;
    private bool waitingForSkillUnlock;
    private string unlockSkillId;
    private GameObject mapInteractionInstance;

    private SaveInfo CurrentSaveInfo => SaveLoad.CurrentSaveInfo;

    public void SetupStage(ScenarioStageData scenarioStageData)
    {
        buttonContinue.gameObject.SetActive(false);
        ClearStageButtons();
        ClearMapInteraction();
        waitingForSkillUnlock = false;
        unlockSkillId = null;

        currentStageData = scenarioStageData;
        stageType = currentStageData.stageType;

        baseStageText = $"<b>{scenarioStageData.characterName}</b>:\n{scenarioStageData.stageText}";

        switch (scenarioStageData.stageType)
        {
            case ScenarioStageData.STAGE_TYPE.DECISION:
                baseStageText += "\n\nMake a key choice";
                break;
            case ScenarioStageData.STAGE_TYPE.QUESTION:
                baseStageText += "\n\nKnowledge check";
                break;
        }

        textPrinter.PrintText(baseStageText, TextPrintFinished);

        if(scenarioStageData.stageType == ScenarioStageData.STAGE_TYPE.DIALOGUE)
        {
            npcImageParent.SetActive(true);
            npcImage.sprite = ImageManager.GetNpcImage(((ScenarioStageDialogueData)scenarioStageData).characterSpriteId);
        }
        else
        {
            npcImageParent.SetActive(false);
        }
    }

    public void ContinueClick()
    {
        ScenarioManager.Instance.SetupNextStage();
        buttonContinue.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (waitingForSkillUnlock == false || string.IsNullOrEmpty(unlockSkillId))
            return;

        if (SkillManager.GetSkillStatus(unlockSkillId) == SkillManager.SkillStatus.Active)
        {
            waitingForSkillUnlock = false;
            ButtonImproved unlockButton = stageButtons.Count > 0 ? stageButtons[0] : null;
            if (unlockButton != null)
            {
                string unlockedSkillName = GetSkillName(unlockSkillId);
                if (string.IsNullOrEmpty(unlockedSkillName))
                    unlockedSkillName = unlockSkillId;

                unlockButton.ButtonText = $"Unlocked skill: {unlockedSkillName}";
                SetButtonDisabledColor(unlockButton, new Color(0f, 0f, 0f, 0.5f));
                unlockButton.ButtonComponent.interactable = false;
            }

            buttonContinue.gameObject.SetActive(true);
        }
    }

    private void TextPrintFinished()
    {
        switch (stageType)
        {
            case ScenarioStageData.STAGE_TYPE.DIALOGUE:
                buttonContinue.gameObject.SetActive(true);
                break;
            case ScenarioStageData.STAGE_TYPE.DECISION:
                SpawnDecisionButtons();
                break;
            case ScenarioStageData.STAGE_TYPE.QUESTION:
                SpawnAnswerButtons();
                break;
            case ScenarioStageData.STAGE_TYPE.UNLOCK_SKILL:
                SetupUnlockSkillStage();
                break;
            case ScenarioStageData.STAGE_TYPE.MAP_INTERACTION:
                SetupMapInteractionStage();
                break;
            default:
                buttonContinue.gameObject.SetActive(true);
                break;
        }
    }

    private void SpawnDecisionButtons()
    {
        ClearStageButtons();

        ScenarioStageDecisionData decisionData = currentStageData as ScenarioStageDecisionData;
        if (decisionData == null || decisionData.options.Count == 0)
        {
            buttonContinue.gameObject.SetActive(true);
            return;
        }

        for (int i = 0; i < decisionData.options.Count; i++)
        {
            ScenarioStageDecisionData.DecisionOption option = decisionData.options[i];
            int optionIndex = i;
            string optionText = option.optionText;
            ButtonImproved optionButton = Instantiate(buttonPrefab, elementsParent);
            optionButton.ButtonText = optionText;
            if (optionButton.ContentSizeFitter != null)
                optionButton.ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            stageButtons.Add(optionButton);
            optionButton.OnClick.AddListener(() => HandleDecisionSelected(optionButton, decisionData, optionIndex, optionText));
        }
    }

    private void SpawnAnswerButtons()
    {
        ClearStageButtons();

        ScenarioStageQuestionData questionData = currentStageData as ScenarioStageQuestionData;
        if (questionData == null || questionData.answers.Count == 0)
        {
            buttonContinue.gameObject.SetActive(true);
            return;
        }

        for (int i = 0; i < questionData.answers.Count; i++)
        {
            int answerIndex = i;
            string answerText = questionData.answers[i];
            ButtonImproved answerButton = Instantiate(buttonPrefab, elementsParent);
            answerButton.ButtonText = answerText;
            if (answerButton.ContentSizeFitter != null)
                answerButton.ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            stageButtons.Add(answerButton);
            answerButton.OnClick.AddListener(() => HandleAnswerSelected(questionData, answerIndex, answerText, answerButton));
        }
    }

    private void HandleDecisionSelected(ButtonImproved selectedButton, ScenarioStageDecisionData decisionData, int optionIndex, string optionText)
    {
        SaveDecision(decisionData, optionIndex, optionText);
        ApplyDecisionRewards(decisionData, optionIndex);
        for (int i = stageButtons.Count - 1; i >= 0; i--)
        {
            ButtonImproved button = stageButtons[i];
            if (button != selectedButton)
                Destroy(button.gameObject);
        }

        selectedButton.ButtonComponent.interactable = false;
        SetButtonDisabledColor(selectedButton, new Color(0f, 0f, 0f, 0.5f));
        ContinueClick();
    }

    private void SaveDecision(ScenarioStageDecisionData decisionData, int optionIndex, string optionText)
    {
        if (decisionData == null || SaveLoad.CurrentSaveInfo == null)
            return;

        if (SaveLoad.CurrentSaveInfo.decisions == null)
            SaveLoad.CurrentSaveInfo.decisions = new List<SaveInfo.DecisionInfo>();

        SaveInfo.DecisionInfo decisionInfo = new SaveInfo.DecisionInfo
        {
            stageId = decisionData.id,
            stageText = decisionData.stageText,
            optionIndex = optionIndex,
            optionsCount = decisionData.options != null ? decisionData.options.Count : 0,
            optionText = optionText
        };

        SaveLoad.CurrentSaveInfo.decisions.Add(decisionInfo);
    }

    private void HandleAnswerSelected(ScenarioStageQuestionData questionData, int answerIndex, string answerText, ButtonImproved answerButton)
    {
        if (answerIndex == questionData.correctAnswerIndex)
        {
            ApplyQuestionRewards(questionData);
            SetButtonDisabledColor(answerButton, Color.green);
            DisableStageButtons();
            buttonContinue.gameObject.SetActive(true);
            return;
        }

        answerButton.ButtonComponent.interactable = false;
        ContentUI.SetupContentUI(null, questionData.incorrectAnswerText);
        ButtonImproved continueButton = ContentUI.AddButton("Continue");
        continueButton.OnClick.AddListener(() => UIManager.Instance.ShowScreen("GameUI"));
    }

    private void SetupUnlockSkillStage()
    {
        ClearStageButtons();

        ScenarioStageUnlockSkillData unlockSkillData = currentStageData as ScenarioStageUnlockSkillData;
        if (unlockSkillData == null)
        {
            buttonContinue.gameObject.SetActive(true);
            return;
        }

        unlockSkillId = unlockSkillData.skillId;
        if (SkillManager.GetSkillStatus(unlockSkillData.skillId) == SkillManager.SkillStatus.Active)
        {
            buttonContinue.gameObject.SetActive(true);
            return;
        }

        waitingForSkillUnlock = true;
        ButtonImproved unlockButton = Instantiate(buttonPrefab, elementsParent);
        unlockButton.ButtonText = GetUnlockButtonText(unlockSkillData.skillId);
        unlockButton.OnClick.AddListener(() => UIManager.Instance.ShowScreen("SkillsUI"));
        stageButtons.Add(unlockButton);
    }

    private void SetupMapInteractionStage()
    {
        ClearMapInteraction();

        ScenarioStageMapInteractionData mapData = currentStageData as ScenarioStageMapInteractionData;
        if (mapData == null)
        {
            buttonContinue.gameObject.SetActive(true);
            return;
        }

        FlyToLocation.Instance.FlyTo(mapData.latitude, mapData.longitude);

        mapInteractionInstance = PlaceOnGlobe.Instance.PlaceAndReturn(null, mapData.latitude, mapData.longitude);

        if (mapInteractionInstance == null)
        {
            buttonContinue.gameObject.SetActive(true);
            return;
        }

        MapInteractionMarker marker = mapInteractionInstance.GetComponent<MapInteractionMarker>();
        if (marker == null)
            marker = mapInteractionInstance.AddComponent<MapInteractionMarker>();

        marker.ApplyMarkerType(true);
        marker.OnClicked.RemoveAllListeners();
        marker.OnClicked.AddListener(HandleMapInteractionClicked);
    }

    private void HandleMapInteractionClicked()
    {
        ClearMapInteraction();
        ScenarioManager.Instance.SetupNextStage();
    }

    private string GetUnlockButtonText(string skillId)
    {
        if (string.IsNullOrEmpty(skillId))
        {
            Debug.LogError("Invalid skill ID for unlock stage.");
            return "Continue";
        }

        if (SkillManager.Instance == null || SkillManager.Instance.Skills == null)
        {
            Debug.LogError("Skill manager is not available for unlock stage.");
            return "Continue";
        }

        for (int i = 0; i < SkillManager.Instance.Skills.Count; i++)
        {
            SkillData skill = SkillManager.Instance.Skills[i];
            if (skill.id == skillId)
                return $"Unlock {skill.name}";
        }

        Debug.LogError($"Skill '{skillId}' not found for unlock stage.");
        return "Continue";
    }

    private string GetSkillName(string skillId)
    {
        if (string.IsNullOrEmpty(skillId) || SkillManager.Instance == null || SkillManager.Instance.Skills == null)
            return null;

        for (int i = 0; i < SkillManager.Instance.Skills.Count; i++)
        {
            SkillData skill = SkillManager.Instance.Skills[i];
            if (skill.id == skillId)
                return skill.name;
        }

        return null;
    }

    private void ClearStageButtons()
    {
        for (int i = elementsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(elementsParent.GetChild(i).gameObject);
        }

        stageButtons.Clear();
    }

    private void ClearMapInteraction()
    {
        if (mapInteractionInstance == null)
            return;

        Destroy(mapInteractionInstance);
        mapInteractionInstance = null;
    }

    private void DisableStageButtons()
    {
        for (int i = 0; i < stageButtons.Count; i++)
        {
            stageButtons[i].ButtonComponent.interactable = false;
        }
    }

    private void SetButtonDisabledColor(ButtonImproved button, Color color)
    {
        if (button == null)
            return;

        ColorBlock colors = button.ButtonComponent.colors;
        colors.disabledColor = color;
        button.ButtonComponent.colors = colors;
    }

    private void ApplyDecisionRewards(ScenarioStageDecisionData decisionData, int optionIndex)
    {
        if (decisionData == null || CurrentSaveInfo == null || decisionData.options == null)
            return;

        if (optionIndex < 0 || optionIndex >= decisionData.options.Count)
            return;

        ScenarioStageDecisionData.DecisionOption option = decisionData.options[optionIndex];
        AddStats(option.moneyReward, option.honorReward, 0);
    }

    private void ApplyQuestionRewards(ScenarioStageQuestionData questionData)
    {
        if (questionData == null || CurrentSaveInfo == null)
            return;

        AddStats(questionData.moneyReward, 0, questionData.legendReward);
    }

    private void AddStats(int moneyDelta, int honorDelta, int legendDelta)
    {
        if (CurrentSaveInfo == null)
            return;

        CurrentSaveInfo.statMoney += moneyDelta;
        CurrentSaveInfo.statHonor += honorDelta;
        CurrentSaveInfo.statLegend += legendDelta;

        SaveLoad.NotifyStatsChanged();
    }
}
