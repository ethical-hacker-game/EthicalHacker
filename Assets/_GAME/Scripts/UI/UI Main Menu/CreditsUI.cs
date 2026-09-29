using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreditsUI : UIScreen
{
    [Header("UI")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI creditsTMP;
    [SerializeField] private float scrollTime = 20f;
    [SerializeField] private bool boldAllLines = false;

    [Space]
    [SerializeField] private List<CreditsReplacements> creditsReplacements = new List<CreditsReplacements>();


    public void Back()
    {
        UIManager.Instance.ShowScreen("MainMenuUI");
    }

    private void OnEnable()
    {
        if (Application.isPlaying == false)
            return;

        ScrollToTop();
    }

    private void Awake()
    {
        StartCoroutine(LoadCredits());
    }

    private IEnumerator LoadCredits()
    {
        string localPath = System.IO.Path.Combine(Application.streamingAssetsPath, "credits.txt");

        if (ContentManager.SupportsLocalFileSystem())
        {
            if (System.IO.File.Exists(localPath) == false)
            {
                Debug.LogWarning($"Credits file not found: {localPath}");
                yield break;
            }

            ApplyCreditsText(System.IO.File.ReadAllLines(localPath));
            yield break;
        }

        string url = ContentManager.CombineUrl(Application.streamingAssetsPath, "credits.txt");

        using UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load credits: {request.error}");
            yield break;
        }

        string[] allLines = request.downloadHandler.text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        ApplyCreditsText(allLines);
    }

    private void ApplyCreditsText(string[] allLines)
    {
        if (boldAllLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {
                allLines[i] = $"<b>{allLines[i]}</b>";
            }
        }

        string allText = string.Join("\n", allLines);

        for (int i = 0; i < creditsReplacements.Count; i++)
        {
            CreditsReplacements replacement = creditsReplacements[i];

            if (string.IsNullOrEmpty(replacement.textToReplace))
                continue;

            allText = allText.Replace(replacement.textToReplace, replacement.newText);
        }

        creditsTMP.text = allText;
    }

    private void Update()
    {
        if (scrollRect.verticalNormalizedPosition > 0)
        {
            scrollRect.verticalNormalizedPosition -= 1 / scrollTime * Time.deltaTime;
        }
    }

    private void ScrollToTop()
    {
        scrollRect.verticalNormalizedPosition = 1;
    }

    [Serializable]
    public class CreditsReplacements
    {
        public string textToReplace;
        public string newText;
    }
}