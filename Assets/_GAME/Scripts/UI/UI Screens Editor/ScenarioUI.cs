using RuntimeInspectorNamespace;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScenarioUI : UIScreen
{
    [SerializeField] private TextMeshProUGUI infoTMP;
    [SerializeField] private RuntimeInspector runtimeInspector;

    public void Back()
    {
        ContentManager.SaveContent(ContentEditorManager.CurrentContentPack);
        UIManager.Instance.ShowScreen("ContentPackUI");
    }


    protected void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        if(ContentEditorManager.CurrentContentPack == null || ContentEditorManager.ScenarioData == null)
            return;

        infoTMP.text = ContentEditorManager.CurrentContentPack.ContentPackId + " : " + ContentEditorManager.ScenarioData.id;
        runtimeInspector.Inspect(ContentEditorManager.ScenarioData);
    }
}
