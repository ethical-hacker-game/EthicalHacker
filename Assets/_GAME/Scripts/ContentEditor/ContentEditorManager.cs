using UnityEngine;

public class ContentEditorManager : MonoBehaviour
{
    public static ContentPack CurrentContentPack { get; private set; }
    public static ScenarioData ScenarioData { get; private set; }

    public static void OpenContentPack(ContentPack contentPack)
    {
        CurrentContentPack = contentPack;
        UIManager.Instance.ShowScreen("ContentPackUI");
    }

    public static void OpenScenario(ScenarioData scenarioData)
    {
        ScenarioData = scenarioData;
        UIManager.Instance.ShowScreen("ScenarioUI");
    }
}
