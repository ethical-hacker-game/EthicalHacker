using System;
using System.Collections.Generic;

[Serializable]
public class ContentPack
{
    private ContentSourceType sourceType;
    private string baseUrl;
    private ContentPackManifest manifest;
    private List<ScenarioData> scenarioDatas = new List<ScenarioData>();

    public ContentSourceType SourceType { get => sourceType; set => sourceType = value; }
    public string BaseUrl { get => baseUrl; set => baseUrl = value; }
    public ContentPackManifest Manifest { get => manifest; set => manifest = value; }
    public List<ScenarioData> ScenarioDatas { get => scenarioDatas; set => scenarioDatas = value; }

    public string ContentPackId => Manifest.contentId;
    public List<ContentPackManifest.FileData> ScenariosFiles => Manifest.scenarios;

    public bool IsLocal => SourceType == ContentSourceType.LocalContent;

    public ScenarioData GetScenarioDataById(string id)
    {
        for (int i = 0; i < ScenarioDatas.Count; i++)
        {
            if(ScenarioDatas[i].id == id)
                return ScenarioDatas[i];
        }

        return null;
    }

    public enum ContentSourceType
    {
        LocalContent,
        OnlineContent
    }
}