using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ContentManager : MonoBehaviour
{
    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter>
        {
            new ScenarioStageDataJsonConverter()
        }
    };

    public static IEnumerator LoadContentPack(ContentPack.ContentSourceType sourceType, string input, UnityAction<ContentPack> AfterLoad, bool resolveUrl = true)
    {
        string baseUrl;;

        if (resolveUrl)
        {
            try
            {
                ResolveContentLocation(sourceType, input, out baseUrl);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                yield break;
            }
        }
        else
        {
            baseUrl = input;
        }

        string manifestUrl = CombineUrl(baseUrl, "manifest.json");

        using UnityWebRequest request = UnityWebRequest.Get(manifestUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load manifest: {request.error}");
            yield break;
        }

        ContentPackManifest manifest;

        try
        {
            string jsonText = CleanJsonString(request.downloadHandler.text);
            manifest = JsonConvert.DeserializeObject<ContentPackManifest>(jsonText, JsonSettings);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Manifest parse error: {ex.Message}");
            yield break;
        }

        if (manifest == null)
        {
            Debug.LogError("Manifest is null");
            yield break;
        }

        if (manifest.scenarios == null)
            manifest.scenarios = new List<ContentPackManifest.FileData>();

        ContentPack newCP = new ContentPack
        {
            SourceType = sourceType,
            BaseUrl = baseUrl,
            Manifest = manifest
        };

        for (int i = 0; i < newCP.ScenariosFiles.Count; i++)
        {
            yield return LoadScenario(newCP, newCP.ScenariosFiles[i].file, scenario =>
            {
                if (scenario != null)
                    newCP.ScenarioDatas.Add(scenario);
            });
        }

        AfterLoad.Invoke(newCP);
    }

    public static IEnumerator LoadScenario(ContentPack contentPack, string scenarioFileName, UnityAction<ScenarioData> AfterLoad)
    {
        string url = CombineUrl(contentPack.BaseUrl, scenarioFileName);

        using UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Scenario load failed: {request.error}");
            yield break;
        }

        Debug.Log(request.downloadHandler.text);

        ScenarioData scenario;

        try
        {
            string jsonText = CleanJsonString(request.downloadHandler.text);
            scenario = JsonConvert.DeserializeObject<ScenarioData>(jsonText, JsonSettings);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Scenario parse error: {ex.Message}");
            yield break;
        }

        if (scenario == null)
        {
            Debug.LogError("Scenario is null");
            yield break;
        }

        AfterLoad.Invoke(scenario);
    }

    private static string CleanJsonString(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        int startIndex = json.IndexOfAny(new char[] { '{', '[' });
        int endIndex = json.LastIndexOfAny(new char[] { '}', ']' });
        if (startIndex >= 0 && endIndex >= startIndex)
        {
            return json.Substring(startIndex, endIndex - startIndex + 1);
        }
        return json;
    }

    public static List<string> GetContentPackNames()
    {
        List<string> result = new List<string>();

        if (!SupportsLocalFileSystem())
        {
            Debug.LogError("Not supported on this platform");
            return result;
        }

        foreach (string dir in Directory.GetDirectories(Application.streamingAssetsPath))
        {
            result.Add(Path.GetFileName(dir));
        }

        return result;
    }

    public static bool SaveContent(ContentPack contentPack)
    {
        if (!SupportsLocalFileSystem())
        {
            Debug.LogError("Saving not supported on this platform");
            return false;
        }

        if (contentPack == null || contentPack.Manifest == null)
        {
            Debug.LogError("Invalid content pack");
            return false;
        }

        if (!contentPack.IsLocal)
        {
            Debug.LogError("Can save only local content");
            return false;
        }

        contentPack.Manifest.scenarios = new List<ContentPackManifest.FileData>();

        for (int i = 0; i < contentPack.ScenarioDatas.Count; i++)
        {
            ScenarioData scenario = contentPack.ScenarioDatas[i];
            try
            {
                string fileName = scenario.id + ".json";
                string pathScenario = Path.Combine(contentPack.BaseUrl, fileName);
                File.WriteAllText(pathScenario, JsonConvert.SerializeObject(scenario, Formatting.Indented, JsonSettings));
                contentPack.Manifest.scenarios.Add(new ContentPackManifest.FileData(fileName, scenario.name));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save scenario {scenario.id}: {ex.Message}");
            }
        }

        string pathManifest = Path.Combine(contentPack.BaseUrl, "manifest.json");
        
        try
        {
            File.WriteAllText(pathManifest, JsonConvert.SerializeObject(contentPack.Manifest, Formatting.Indented, JsonSettings));
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }

    private static void ResolveContentLocation(ContentPack.ContentSourceType sourceType, string input, out string baseUrl)
    {
        baseUrl = null;

        switch (sourceType)
        {
            case ContentPack.ContentSourceType.LocalContent:
                if (string.IsNullOrEmpty(input))
                    throw new Exception("Folder name required");

                baseUrl = CombineUrl(Application.streamingAssetsPath, input);
                break;

            case ContentPack.ContentSourceType.OnlineContent:
                baseUrl = ResolveOnlineContentBaseUrl(input);
                break;
        }
    }

    public static string ResolveOnlineContentBaseUrl(string url)
    {
        url = url.TrimEnd('/');

        if (url.StartsWith("https://raw.githubusercontent.com/"))
            return url;

        if (!url.StartsWith("https://github.com/"))
            return url;

        string path = url.Replace("https://github.com/", "");
        string[] parts = path.Split('/');

        if (parts.Length < 2)
            throw new Exception("Invalid GitHub URL");

        string user = parts[0];
        string repo = parts[1];

        if (parts.Length == 2)
            return $"https://raw.githubusercontent.com/{user}/{repo}/main";

        if (parts[2] == "tree")
        {
            string branch = parts[3];
            string sub = parts.Length > 4 ? string.Join("/", parts, 4, parts.Length - 4) : "";
            return $"https://raw.githubusercontent.com/{user}/{repo}/{branch}/{sub}";
        }

        throw new Exception("Unsupported GitHub format");
    }

    public static bool SupportsLocalFileSystem()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return true;
#else
        return false;
#endif
    }

    public static string CombineUrl(params string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return string.Empty;

        string result = paths[0];
        for (int i = 1; i < paths.Length; i++)
        {
            result = $"{result.TrimEnd('/')}/{paths[i].TrimStart('/')}";
        }
        return result;
    }
}