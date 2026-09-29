using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : UIScreen
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform stagesParent;
    [SerializeField] private UIStageElement stagePrefab; 


    public static GameUI Instance { get; private set; }

    public void ShowStage(ScenarioStageData stage)
    {
        UIStageElement stageElement = Instantiate(stagePrefab, stagesParent);
        stageElement.SetupStage(stage);
    }

    public void MainMenu()
    {
        ContentUI.SetupContentUI("Exit to menu", "Do you want to exit to the menu? Your progress will be saved.");

        ButtonImproved yesButton = ContentUI.AddButton("Yes");
        yesButton.OnClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        ButtonImproved noButton = ContentUI.AddButton("No");
        noButton.OnClick.AddListener(() => UIManager.Instance.ShowScreen("GameUI"));
    }

    public void ShowSkillsUI()
    {
        UIManager.Instance.ShowScreen("SkillsUI");
    }

    public void ShowStoryUI()
    {
        UIManager.Instance.ShowScreen("StoryUI");
    }

    private void Awake()
    {
        Instance = this;

        for (int i = stagesParent.childCount - 1; i >= 0; i--)
        {
            Destroy(stagesParent.GetChild(i).gameObject);
        }
    }

    private void LateUpdate()
    {
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}
