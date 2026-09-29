using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ContentSelectionUI : UIScreen
{
    [SerializeField] private TMP_InputField newContentInputField;
    [SerializeField] private ButtonImproved buttonPrefab;
    [SerializeField] private Transform contentListContainer;

    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void CreateNewContentPack()
    {
        string newContentId = newContentInputField.text.Trim();
    }

    public void OpenContentFolder()
    {
        System.Diagnostics.Process.Start(Application.streamingAssetsPath, "explorer.exe");
    }

    protected void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        PrepareContentButtons();
    }

    public void PrepareContentButtons()
    {
        foreach (Transform child in contentListContainer)
        {
            Destroy(child.gameObject);
        }

        List<string> contents = ContentManager.GetContentPackNames();

        foreach (string contentId in contents)
        {
            ButtonImproved button = Instantiate(buttonPrefab, contentListContainer);
            button.ContentSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            button.ButtonText = contentId;
            button.OnClick.AddListener(() =>
            {
                UIManager.Instance.ShowScreen("LoadingUI");
                UIManager.Instance.StartCoroutine(
                    ContentManager.LoadContentPack(
                        ContentPack.ContentSourceType.LocalContent,
                        contentId,
                        ContentEditorManager.OpenContentPack));
            });
        }
    }
}
