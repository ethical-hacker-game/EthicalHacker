using System;
using System.Collections.Generic;
using UnityEngine;

public static class SaveLoad
{
    private const string SaveSlotKeyPrefix = "SaveSlot";

    public static string CurrentSaveSlotKey { get; private set; }
    public static SaveInfo CurrentSaveInfo { get; private set; }

    public static event Action StatsChanged;

    public static List<SaveInfo> SaveInfos
    {
        get
        {
            if (!isInitialized)
                LoadSaveInfos();

            return saveInfos;
        }
    }

    private static List<SaveInfo> saveInfos = new List<SaveInfo>();
    private static bool isInitialized = false;

    public static void SetCurrentSaveSlot(int slotIndex)
    {
        CurrentSaveSlotKey = GetSaveKey(slotIndex);
        CurrentSaveInfo = GetSaveInfo(slotIndex);

        if (CurrentSaveInfo == null)
            return;


    }

    public static void CreateNewSave(string slotKey, ContentPack contentPack, ScenarioData scenarioData)
    {
        CurrentSaveSlotKey = slotKey;
        CurrentSaveInfo = new SaveInfo
        {
            slotId = CurrentSaveSlotKey,
            scenarioName = scenarioData.name,
            saveDateTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),

            sourceTypeName = contentPack.SourceType.ToString(),
            sourceBaseUrl = contentPack.BaseUrl,
            sourceContentPackId = contentPack.ContentPackId,
            sourceScenarioId = scenarioData.id,

            stageIndex = -1
        };
    }

    public static void SaveCurrentGame()
    {
        if(CurrentSaveInfo == null)
        {
            Debug.LogError("No current save info to save");
            return;
        }

        CurrentSaveSlotKey = CurrentSaveInfo.slotId;

        if (string.IsNullOrEmpty(CurrentSaveSlotKey))
            CurrentSaveSlotKey = SaveSlotKeyPrefix + "_Dev";

        CurrentSaveInfo.saveDateTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string saveDataJson = JsonUtility.ToJson(CurrentSaveInfo);
        PlayerPrefs.SetString(CurrentSaveSlotKey, saveDataJson);
        PlayerPrefs.Save();
    }

    public static SaveInfo GetSaveInfo(int slotIndex)
    {
        if (!isInitialized)
            LoadSaveInfos();

        if (saveInfos.Count == 0)
            return null;

        string slotKey = GetSaveKey(slotIndex);

        for (int i = 0; i < saveInfos.Count; i++)
        {
            if (saveInfos[i].slotId == slotKey)
                return saveInfos[i];
        }

        return null;
    }

    public static void DeleteSave(int slotIndex)
    {
        string slotKey = GetSaveKey(slotIndex);
        if (PlayerPrefs.HasKey(slotKey))
        {
            PlayerPrefs.DeleteKey(slotKey);
            LoadSaveInfos();
        }
    }

    public static void LoadSaveInfos()
    {
        saveInfos.Clear();

        for (int i = 0; i < 3; i++)
        {
            string saveKey = GetSaveKey(i);

            if (PlayerPrefs.HasKey(saveKey))
            {
                string saveDataJson = PlayerPrefs.GetString(saveKey);
                SaveInfo saveInfo = JsonUtility.FromJson<SaveInfo>(saveDataJson);
                saveInfos.Add(saveInfo);
            }
        }

        isInitialized = true;
    }

    internal static void NotifyStatsChanged()
    {
        StatsChanged?.Invoke();
    }

    private static string GetSaveKey(int slotIndex)
    {
        if(slotIndex <0)
            return $"{SaveSlotKeyPrefix}_Dev";

        return $"{SaveSlotKeyPrefix}{slotIndex}";
    }
}
