using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavesUI : UIScreen
{
    [Space]
    [SerializeField] private ButtonImproved saveSlotButton0;
    [SerializeField] private ButtonImproved saveSlotButton1;
    [SerializeField] private ButtonImproved saveSlotButton2;

    [Space]
    [SerializeField] private ButtonImproved DeleteSaveButton0;
    [SerializeField] private ButtonImproved DeleteSaveButton1;
    [SerializeField] private ButtonImproved DeleteSaveButton2;

    private SaveInfo SaveInfo => SaveLoad.CurrentSaveInfo;

    public void LoadSaveSlot(int slotIndex)
    {
        SaveLoad.SetCurrentSaveSlot(slotIndex);

        if (SaveLoad.CurrentSaveInfo != null)
        {
            UIManager.Instance.ShowScreen("LoadingUI");

            ContentPack.ContentSourceType contentSourceType = (ContentPack.ContentSourceType)Enum.Parse(
                typeof(ContentPack.ContentSourceType),
                SaveInfo.sourceTypeName,
                true);


            UIManager.Instance.StartCoroutine(ContentManager.LoadContentPack(contentSourceType, SaveInfo.sourceBaseUrl, contentPack =>
            {
                for (int i = 0; i < contentPack.ScenarioDatas.Count; i++)
                {
                    if (contentPack.ScenarioDatas[i].id == SaveInfo.sourceScenarioId)
                    {
                        ScenarioManager.SetCurrentScenario(contentPack.ScenarioDatas[i]);
                        SceneManager.LoadScene("GameGlobe");
                        return;
                    }
                }
                ContentUI.SetupContentUI("Error", "Could not find the scenario associated with this save.");
                ContentUI.AddButton("Back to Main Menu").OnClick.AddListener(() =>
                {
                    UIManager.Instance.ShowScreen("MainMenuUI");
                });
            }, false));
        }
        else
        {
            UIManager.Instance.ShowScreen("NewGameUI");
        }

    }

    public void DeleteSaveSlot(int slotIndex)
    {
        string slotName = GetSaveSlotButtonText(slotIndex);
        ContentUI.SetupContentUI("Delete Save", $"Are you sure you want to delete this save?\n\n{slotName}");

        ContentUI.AddButton("Yes").OnClick.AddListener(() =>
        {
            SaveLoad.DeleteSave(slotIndex);
            OnEnable();
            UIManager.Instance.ShowScreen("SavesUI");
        });

        ContentUI.AddButton("No").OnClick.AddListener(() =>
        {
            UIManager.Instance.ShowScreen("SavesUI");
        });
    }

    public void Back()
    {
        UIManager.Instance.ShowScreen("MainMenuUI");
    }

    private void Start()
    {
        SaveLoad.LoadSaveInfos();
    }

    private void OnEnable()
    {
        if (Application.isPlaying == false)
            return;

        saveSlotButton0.ButtonText = GetSaveSlotButtonText(0);
        saveSlotButton1.ButtonText = GetSaveSlotButtonText(1);
        saveSlotButton2.ButtonText = GetSaveSlotButtonText(2);

        DeleteSaveButton0.gameObject.SetActive(SaveLoad.GetSaveInfo(0) != null);
        DeleteSaveButton1.gameObject.SetActive(SaveLoad.GetSaveInfo(1) != null);
        DeleteSaveButton2.gameObject.SetActive(SaveLoad.GetSaveInfo(2) != null);
    }

    private string GetSaveSlotButtonText(int saveInfoIndex)
    {
        SaveInfo saveInfo = SaveLoad.GetSaveInfo(saveInfoIndex);

        if (saveInfo == null)
            return $"New Game";

        return $"{saveInfo.scenarioName} <size=80%>{saveInfo.saveDateTime}</size>";
    }
}
