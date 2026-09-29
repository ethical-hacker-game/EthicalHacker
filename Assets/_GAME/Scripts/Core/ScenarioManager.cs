using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    private int currentStageIndex;
    private readonly Dictionary<MapInteractionMarker, ScenarioPointOfInterestData> pointOfInterestMarkers = new Dictionary<MapInteractionMarker, ScenarioPointOfInterestData>();
    private readonly List<ScenarioPointOfInterestData> pendingPointsOfInterest = new List<ScenarioPointOfInterestData>();
    private MapInteractionMarker activePointOfInterestMarker;

    public static ScenarioManager Instance { get; private set; }
    public static ScenarioData CurrentScenario { get; private set; }
    public static ScenarioStageData CurrentStage { get; private set; }
    public static SaveInfo SaveInfo => SaveLoad.CurrentSaveInfo;

    public static void SetCurrentScenario(ScenarioData scenario)
    {
        CurrentScenario = scenario;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (CurrentScenario != null)
            SetupScenario();
        else
            StartCoroutine(AutoLoadBaseScenario());
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Editor-only shortcut: press Escape to instantly finish the current stage
        // and jump to the next one, for faster scenario testing.
        if (Input.GetKeyDown(KeyCode.Escape))
            SkipCurrentStage();
    }

    private void SkipCurrentStage()
    {
        if (CurrentScenario == null || CurrentScenario.stages.Count == 0)
            return;

        if (currentStageIndex + 1 >= CurrentScenario.stages.Count)
        {
            Debug.Log("Cannot skip: already at the last stage of the scenario.");
            return;
        }

        SetupNextStage();
    }
#endif

    private void SetupScenario()
    {
        if(CurrentScenario.stages.Count == 0)
        {
            Debug.LogError("Scenario has no stages defined");
            return;
        }

        SetupPointsOfInterest();
        currentStageIndex = SaveInfo.stageIndex;
        SetupNextStage();
    }

    public void SetupNextStage()
    {
        SaveInfo.stageIndex = currentStageIndex;
        SaveInfo.stageId = CurrentStage?.id ?? "";
        SaveLoad.SaveCurrentGame();

        currentStageIndex++;
        if (currentStageIndex >= CurrentScenario.stages.Count)
        {
            Debug.LogError("No more stages available in the scenario");
            return;
        }

        CurrentStage = CurrentScenario.stages[currentStageIndex];
        Debug.Log($"Stage {currentStageIndex + 1}/{CurrentScenario.stages.Count}: {CurrentStage.id}");
        GameUI.Instance.ShowStage(CurrentStage);
        TryShowPointsOfInterest();
    }

    private IEnumerator AutoLoadBaseScenario()
    {
//#if UNITY_EDITOR
//        string contentPackId = "TestContent";
//#else
        string contentPackId = "BaseContent";
//#endif

        yield return ContentManager.LoadContentPack(ContentPack.ContentSourceType.LocalContent, contentPackId, (contentPack) =>
        {
            if (contentPack.ScenarioDatas.Count > 0)
            {
                CurrentScenario = contentPack.ScenarioDatas[0];
                SaveLoad.CreateNewSave("AutoSave", contentPack, CurrentScenario);
                SetupScenario();
            }
            else
            {
                Debug.LogError("No scenarios found in content pack");
            }
        });
    }

    private void SetupPointsOfInterest()
    {
        ClearPointsOfInterest();
        pendingPointsOfInterest.Clear();

        if (CurrentScenario == null || CurrentScenario.pointsOfInterest == null || CurrentScenario.pointsOfInterest.Count == 0)
            return;

        for (int i = 0; i < CurrentScenario.pointsOfInterest.Count; i++)
        {
            ScenarioPointOfInterestData poiData = CurrentScenario.pointsOfInterest[i];
            if (poiData == null)
                continue;

            pendingPointsOfInterest.Add(poiData);
        }

        TryShowPointsOfInterest();
    }

    private void HandlePointOfInterestClicked(MapInteractionMarker marker)
    {
        if (marker == null || activePointOfInterestMarker != null)
            return;

        if (!pointOfInterestMarkers.TryGetValue(marker, out ScenarioPointOfInterestData poiData))
            return;

        activePointOfInterestMarker = marker;
        SaveInfo.PointOfInterestDecisionInfo completedInfo = GetPointOfInterestDecision(poiData.id);
        if (completedInfo != null)
        {
            ShowCompletedPointOfInterest(poiData, completedInfo);
            return;
        }

        ShowPointOfInterest(poiData);
    }

    private void ShowPointOfInterest(ScenarioPointOfInterestData poiData)
    {
        if (poiData == null)
            return;

        ContentUI.SetupContentUI(poiData.title, poiData.description);

        if (poiData.decisions != null && poiData.decisions.Count > 0)
        {
            for (int i = 0; i < poiData.decisions.Count; i++)
            {
                int optionIndex = i;
                ScenarioPointOfInterestData.DecisionOption option = poiData.decisions[i];
                string optionText = option.optionText;
                ButtonImproved optionButton = ContentUI.AddButton(optionText);
                optionButton.OnClick.AddListener(() => HandlePointOfInterestDecision(poiData, optionIndex, optionText));
            }
        }
        else
        {
            ButtonImproved continueButton = ContentUI.AddButton("Continue");
            continueButton.OnClick.AddListener(() => HandlePointOfInterestClosed(poiData));
        }
    }

    private void HandlePointOfInterestDecision(ScenarioPointOfInterestData poiData, int optionIndex, string optionText)
    {
        if (poiData == null || poiData.decisions == null)
            return;

        if (optionIndex < 0 || optionIndex >= poiData.decisions.Count)
            return;

        SavePointOfInterestDecision(poiData, optionIndex, optionText);

        ScenarioPointOfInterestData.DecisionOption option = poiData.decisions[optionIndex];
        AddStats(option.moneyReward, option.honorReward, option.legendReward);

        SaveLoad.SaveCurrentGame();
        ClosePointOfInterest(poiData);
    }

    private void HandlePointOfInterestClosed(ScenarioPointOfInterestData poiData)
    {
        if (poiData == null)
            return;

        SavePointOfInterestCompletion(poiData);
        AddStats(poiData.moneyReward, poiData.honorReward, poiData.legendReward);
        SaveLoad.SaveCurrentGame();
        ClosePointOfInterest(poiData);
    }

    private void ClosePointOfInterest(ScenarioPointOfInterestData poiData)
    {
        if (activePointOfInterestMarker != null)
        {
            activePointOfInterestMarker.SetPulseEnabled(false);
            activePointOfInterestMarker.transform.localScale = Vector3.one * 0.5f;
            activePointOfInterestMarker.SetColor(new Color(0.5f, 0.8f, 1f, 0.5f)); // Light blue, semi-transparent

            activePointOfInterestMarker = null;
        }

        UIManager.Instance.ShowScreen("GameUI");
    }

    private void ClosePointOfInterestInfo()
    {
        activePointOfInterestMarker = null;
        UIManager.Instance.ShowScreen("GameUI");
    }

    private void RemovePointOfInterest(MapInteractionMarker marker)
    {
        if (marker == null)
            return;

        pointOfInterestMarkers.Remove(marker);
        marker.Remove();
    }

    private void ClearPointsOfInterest()
    {
        if (pointOfInterestMarkers.Count == 0)
            return;

        foreach (var marker in pointOfInterestMarkers.Keys)
        {
            if (marker != null)
                Destroy(marker.gameObject);
        }

        pointOfInterestMarkers.Clear();
        activePointOfInterestMarker = null;
    }

    private void TryShowPointsOfInterest()
    {
        if (pendingPointsOfInterest.Count == 0)
            return;

        if (PlaceOnGlobe.Instance == null)
        {
            Debug.LogWarning("Point of interest markers cannot be placed without PlaceOnGlobe.");
            return;
        }

        int progressPercent = GetScenarioProgressPercent();
        for (int i = pendingPointsOfInterest.Count - 1; i >= 0; i--)
        {
            ScenarioPointOfInterestData poiData = pendingPointsOfInterest[i];
            if (poiData == null)
            {
                pendingPointsOfInterest.RemoveAt(i);
                continue;
            }

            if (!CanShowPointOfInterest(poiData, progressPercent))
                continue;

            if (CreatePointOfInterestMarker(poiData))
            {
                NotificationElementUI.Instance.ShowNotification("A new interaction is available on the globe.");
                pendingPointsOfInterest.RemoveAt(i);
            }
        }
    }

    private bool CreatePointOfInterestMarker(ScenarioPointOfInterestData poiData)
    {
        GameObject markerObject = PlaceOnGlobe.Instance.PlaceAndReturn(null, poiData.latitude, poiData.longitude);
        if (markerObject == null)
            return false;

        MapInteractionMarker marker = markerObject.GetComponent<MapInteractionMarker>();
        if (marker == null)
            marker = markerObject.AddComponent<MapInteractionMarker>();

        SaveInfo.PointOfInterestDecisionInfo completedInfo = GetPointOfInterestDecision(poiData.id);

        marker.SetRemoveOnClick(false);
        marker.ApplyMarkerType(false);
        if (completedInfo != null)
        {
            marker.SetPulseEnabled(false);
            marker.transform.localScale = Vector3.one * 0.5f;
            marker.SetColor(new Color(0.5f, 0.8f, 1f, 0.5f)); // Light blue, semi-transparent
        }
        marker.OnClicked.RemoveAllListeners();
        marker.OnClicked.AddListener(() => HandlePointOfInterestClicked(marker));

        pointOfInterestMarkers[marker] = poiData;
        return true;
    }

    private bool CanShowPointOfInterest(ScenarioPointOfInterestData poiData, int progressPercent)
    {
        if (poiData == null)
            return false;

        int requiredPercent = Mathf.Clamp(poiData.showAtProgressPercent, 0, 100);
        return progressPercent >= requiredPercent;
    }

    private int GetScenarioProgressPercent()
    {
        int totalStages = CurrentScenario != null ? CurrentScenario.stages.Count : 0;
        if (totalStages <= 0)
            return 0;

        int currentStage = Mathf.Clamp(currentStageIndex + 1, 0, totalStages);
        return Mathf.FloorToInt((currentStage / (float)totalStages) * 100f);
    }

    private void SavePointOfInterestDecision(ScenarioPointOfInterestData poiData, int optionIndex, string optionText)
    {
        if (poiData == null || SaveLoad.CurrentSaveInfo == null)
            return;

        if (SaveLoad.CurrentSaveInfo.pointOfInterestDecisions == null)
            SaveLoad.CurrentSaveInfo.pointOfInterestDecisions = new List<SaveInfo.PointOfInterestDecisionInfo>();

        if (GetPointOfInterestDecision(poiData.id) != null)
            return;

        SaveInfo.PointOfInterestDecisionInfo decisionInfo = new SaveInfo.PointOfInterestDecisionInfo
        {
            poiId = poiData.id,
            title = poiData.title,
            description = poiData.description,
            optionIndex = optionIndex,
            optionsCount = poiData.decisions != null ? poiData.decisions.Count : 0,
            optionText = optionText
        };

        SaveLoad.CurrentSaveInfo.pointOfInterestDecisions.Add(decisionInfo);
    }

    private void SavePointOfInterestCompletion(ScenarioPointOfInterestData poiData)
    {
        if (poiData == null || SaveLoad.CurrentSaveInfo == null)
            return;

        if (SaveLoad.CurrentSaveInfo.pointOfInterestDecisions == null)
            SaveLoad.CurrentSaveInfo.pointOfInterestDecisions = new List<SaveInfo.PointOfInterestDecisionInfo>();

        if (GetPointOfInterestDecision(poiData.id) != null)
            return;

        SaveInfo.PointOfInterestDecisionInfo decisionInfo = new SaveInfo.PointOfInterestDecisionInfo
        {
            poiId = poiData.id,
            title = poiData.title,
            description = poiData.description,
            optionIndex = -1,
            optionsCount = poiData.decisions != null ? poiData.decisions.Count : 0,
            optionText = string.Empty
        };

        SaveLoad.CurrentSaveInfo.pointOfInterestDecisions.Add(decisionInfo);
    }

    private SaveInfo.PointOfInterestDecisionInfo GetPointOfInterestDecision(string poiId)
    {
        if (string.IsNullOrEmpty(poiId) || SaveLoad.CurrentSaveInfo == null)
            return null;

        List<SaveInfo.PointOfInterestDecisionInfo> decisions = SaveLoad.CurrentSaveInfo.pointOfInterestDecisions;
        if (decisions == null)
            return null;

        for (int i = 0; i < decisions.Count; i++)
        {
            if (decisions[i] != null && decisions[i].poiId == poiId)
                return decisions[i];
        }

        return null;
    }

    private void ShowCompletedPointOfInterest(ScenarioPointOfInterestData poiData, SaveInfo.PointOfInterestDecisionInfo info)
    {
        if (poiData == null)
            return;

        string title = string.IsNullOrWhiteSpace(poiData.title) ? "Point of Interest" : poiData.title;

        string description = string.Empty;
        if (!string.IsNullOrWhiteSpace(poiData.description))
        {
            description += poiData.description + "\n\n";
        }

        if (info != null && !string.IsNullOrWhiteSpace(info.optionText))
        {
            description += $"You have completed this activity and selected: {info.optionText}";
        }
        else
        {
            description += "You have completed this activity.";
        }

        ContentUI.SetupContentUI(title, description);
        ButtonImproved okButton = ContentUI.AddButton("OK");
        okButton.OnClick.AddListener(ClosePointOfInterestInfo);
    }

    private void AddStats(int moneyDelta, int honorDelta, int legendDelta)
    {
        if (SaveInfo == null)
            return;

        SaveInfo.statMoney += moneyDelta;
        SaveInfo.statHonor += honorDelta;
        SaveInfo.statLegend += legendDelta;
        SaveLoad.NotifyStatsChanged();
    }

#if UNITY_EDITOR
    [ContextMenu("Unlock All Points Of Interest")]
    private void ForceShowAllPOIs()
    {
        if (PlaceOnGlobe.Instance == null)
        {
            Debug.LogWarning("Point of interest markers cannot be placed without PlaceOnGlobe.");
            return;
        }

        for (int i = pendingPointsOfInterest.Count - 1; i >= 0; i--)
        {
            ScenarioPointOfInterestData poiData = pendingPointsOfInterest[i];
            if (poiData == null)
            {
                pendingPointsOfInterest.RemoveAt(i);
                continue;
            }

            if (CreatePointOfInterestMarker(poiData))
            {
                NotificationElementUI.Instance.ShowNotification("A new interaction is available on the globe.");
                pendingPointsOfInterest.RemoveAt(i);
            }
        }
    }
#endif
}
