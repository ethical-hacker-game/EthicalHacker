using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ContentPackUI : UIScreen
{
    [SerializeField] private TMP_InputField newScenarioInputField;
    [SerializeField] private ButtonImproved buttonPrefab;
    [SerializeField] private Transform scenarioListContainer;

    public void Back()
    {
        UIManager.Instance.ShowScreen("ContentSelectionUI");
    }

    public void CreateNewScenario()
    {
        string newScenarioId = newScenarioInputField.text.Trim();

        if (ContentEditorManager.CurrentContentPack.GetScenarioDataById(newScenarioId) != null)
            return;

        ScenarioData newScenario = new ScenarioData()
        {
            id = newScenarioId,
            contentPackId = ContentEditorManager.CurrentContentPack.ContentPackId
        };

        ContentEditorManager.CurrentContentPack.ScenarioDatas.Add(newScenario);
        ContentEditorManager.OpenScenario(newScenario);
    }

    public void OpenContentFolder()
    {
        string contentPackPath = ContentManager.CombineUrl(Application.streamingAssetsPath, ContentEditorManager.CurrentContentPack.ContentPackId);
        System.Diagnostics.Process.Start(contentPackPath, "explorer.exe");
    }


    protected void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        PrepareScenarioButtons();
    }

    private void PrepareScenarioButtons()
    {
        foreach (Transform child in scenarioListContainer)
        {
            Destroy(child.gameObject);
        }

        if (ContentEditorManager.CurrentContentPack == null)
            return;

        List<ScenarioData> scenarioDatas = ContentEditorManager.CurrentContentPack.ScenarioDatas;

        foreach (ScenarioData scenario in scenarioDatas)
        {
            ButtonImproved button = Instantiate(buttonPrefab, scenarioListContainer);
            button.ContentSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            button.ButtonText = scenario.id + ": " + scenario.name;
            button.OnClick.AddListener(() =>
            {
                UIManager.Instance.ShowScreen("LoadingUI");
                ContentEditorManager.OpenScenario(scenario);
            });
        }
    }
}
