using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : UIScreen
{
    [SerializeField] private List<GameObject> elementsOnlyPC;


    public void Play()
    {
        UIManager.Instance.ShowScreen("SavesUI");
    }

    public void OpenCredits()
    {
        UIManager.Instance.ShowScreen("CreditsUI");
    }

    public void OpenEditor()
    {
        SceneManager.LoadScene("EditorScene");
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void Awake()
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
    }
}
