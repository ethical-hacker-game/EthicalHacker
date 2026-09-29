using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public static Dictionary<string, UIScreen> Screens;

    public void ShowScreen(string screenName)
    {
        UIScreen screen;
        Screens.TryGetValue(screenName, out screen);
        
        if(screen == null)
        {
            Debug.LogError($"No screen with name {screenName} found.");
            return;
        }

        screen.gameObject.SetActive(true);

        if(screen.DontHideOtherUIs)
            return;

        for (int i = 0; i < Screens.Count; i++)
        {
            if(Screens.ElementAt(i).Value != screen)
                Screens.ElementAt(i).Value.gameObject.SetActive(false);
        }
    }

    public UIScreen GetScreen(string screenName)
    {
        if (Screens == null)
            return null;

        Screens.TryGetValue(screenName, out UIScreen screen);
        return screen;
    }

    private void Awake()
    {
        Instance = this;
        Screens = new Dictionary<string, UIScreen>();

        UIScreen[] allScreens = GetComponentsInChildren<UIScreen>(true);

        foreach (UIScreen screen in allScreens)
        {
            Screens.Add(screen.gameObject.name, screen);
            screen.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        foreach (KeyValuePair<string, UIScreen> uIScreen in Screens)
        {
            if(uIScreen.Value.IsDefaultScreen)
                uIScreen.Value.gameObject.SetActive(true);
        }
    }
}
