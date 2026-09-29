using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContentUI : UIScreen
{
    public static ContentUI Instance => UIManager.Instance.GetScreen("ContentUI") as ContentUI;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI headerTMP;
    [SerializeField] private TextMeshProUGUI contentTMP;

    [Header("Buttons")]
    [SerializeField] private UI_LayoutRebuilder buttonsParent;
    [SerializeField] private ButtonImproved buttonPrefab;

    private List<ButtonImproved> buttons = new List<ButtonImproved>();

    public static void SetupContentUI(string header, string content)
    {
        Instance.ClearContentUI();
        UIManager.Instance.ShowScreen("ContentUI");

        if (string.IsNullOrEmpty(header) == false)
        {
            Instance.headerTMP.gameObject.SetActive(true);
            Instance.headerTMP.text = header;
        }

        if (string.IsNullOrEmpty(content) == false)
        {
            Instance.contentTMP.gameObject.SetActive(true);
            Instance.contentTMP.text = content;
        }
    }

    public static ButtonImproved AddButton(string buttonText)
    {
        ButtonImproved newButton = Instantiate(Instance.buttonPrefab, Instance.buttonsParent.transform);
        newButton.ButtonText = buttonText;
        Instance.buttons.Add(newButton);
        Instance.buttonsParent.ForceRebuild();
        return newButton;
    }

    private void ClearContentUI()
    {
        headerTMP.gameObject.SetActive(false);
        contentTMP.gameObject.SetActive(false);

        for (int i = buttonsParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonsParent.transform.GetChild(i).gameObject);
        }

        buttons.Clear();
    }
}