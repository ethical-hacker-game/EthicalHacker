using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

public class NewGameUI : UIScreen
{
    [SerializeField] private List<GameObject> elementsOnlyPC;

    [SerializeField] private TMP_InputField githubUrlInputField;
    [SerializeField] private TextMeshProUGUI contentPackHeader;
    [SerializeField] private ButtonImproved scenarioButtonPrefab;
    [SerializeField] private Transform contentListContainer;

    public void OpenContentFolder()
    {
        System.Diagnostics.Process.Start(Application.streamingAssetsPath, "explorer.exe");
    }

    public void ReloadContent()
    {
        PrepareContentButtons();
    }

    public void LoadContentFromGitHub()
    {
        string url = githubUrlInputField.text;
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("GitHub URL is empty");
            return;
        }
        UIManager.Instance.StartCoroutine(
            ContentManager.LoadContentPack(
                ContentPack.ContentSourceType.OnlineContent,
                url,
                AddContentPackButtons));
    }

    private void AddContentPackButtons(ContentPack contentPack)
    {
        TextMeshProUGUI contentPackTMP = Instantiate(contentPackHeader, contentListContainer);
        contentPackTMP.text = contentPack.ContentPackId;

        foreach (ScenarioData scenario in contentPack.ScenarioDatas)
        {
            ButtonImproved button = Instantiate(scenarioButtonPrefab, contentListContainer);
            button.ContentSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            button.ButtonText = scenario.name;
            button.OnClick.AddListener(() =>
            {
                UIManager.Instance.ShowScreen("LoadingUI");
                SaveLoad.CreateNewSave(SaveLoad.CurrentSaveSlotKey, contentPack, scenario);
                ScenarioManager.SetCurrentScenario(scenario);
                SceneManager.LoadScene("GameGlobe");
            });
        }
    }

    public void Back()
    {
        UIManager.Instance.ShowScreen("SavesUI");
    }

    private void Start()
    {
#if UNITY_STANDALONE
        foreach (var element in elementsOnlyPC)
        {
            element.SetActive(true);
        }
#else
        foreach (var element in elementsOnlyPC)
        {
            element.SetActive(false);
        }
#endif

        PrepareContentButtons();
    }

    private void PrepareContentButtons()
    {
        foreach (Transform child in contentListContainer)
        {
            Destroy(child.gameObject);
        }

        if (ContentManager.SupportsLocalFileSystem() == false)
        {
            UIManager.Instance.StartCoroutine(
                    ContentManager.LoadContentPack(
                        ContentPack.ContentSourceType.OnlineContent,
                        Path.Combine(Application.streamingAssetsPath, "BaseContent"),
                        AddContentPackButtons));

            return;
        }

        List<string> contents = ContentManager.GetContentPackNames();

        foreach (string contentId in contents)
        {
            UIManager.Instance.StartCoroutine(
                    ContentManager.LoadContentPack(
                        ContentPack.ContentSourceType.LocalContent,
                        contentId,
                        AddContentPackButtons));

        }
    }
}
