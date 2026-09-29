using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;

public class SceneMenuGenerator : AssetPostprocessor
{
    private const string ScriptPath = "Assets/Editor/GeneratedSceneMenu.cs";

    // Method called automatically when any project files change (added/removed/renamed)
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool scenesChanged = importedAssets.Any(path => path.EndsWith(".unity")) ||
                             deletedAssets.Any(path => path.EndsWith(".unity")) ||
                             movedAssets.Any(path => path.EndsWith(".unity")) ||
                             movedFromAssetPaths.Any(path => path.EndsWith(".unity"));

        if (scenesChanged)
        {
            GenerateMenu();
        }
    }

    [MenuItem("Scenes/Update Scene Menu", priority = 100)]
    public static void GenerateMenu()
    {
        // Find all scenes in the project
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        var scenePaths = sceneGuids.Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToList();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// This file is auto-generated. Do not edit it manually.");
        sb.AppendLine("using UnityEditor;");
        sb.AppendLine("using UnityEditor.SceneManagement;");
        sb.AppendLine();
        sb.AppendLine("public static class GeneratedSceneMenu");
        sb.AppendLine("{");

        int index = 0;
        foreach (var path in scenePaths)
        {
            // Skip scenes from Packages, focus on Assets
            if (path.StartsWith("Packages/"))
                continue;

            string sceneName = Path.GetFileNameWithoutExtension(path);

            // Avoid issues with C# method names
            string methodName = "Open_" + sceneName.Replace(" ", "_").Replace("-", "_").Replace(".", "_") + "_" + index;

            sb.AppendLine($"    [MenuItem(\"Scenes/{sceneName}\", priority = {index})]");
            sb.AppendLine($"    public static void {methodName}()");
            sb.AppendLine("    {");
            sb.AppendLine("        // Ask user to save current scene if it has modifications");
            sb.AppendLine($"        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())");
            sb.AppendLine("        {");
            sb.AppendLine($"            EditorSceneManager.OpenScene(\"{path}\");");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            index++;
        }

        sb.AppendLine("}");

        // Ensure the Editor folder exists
        string editorFolder = Path.GetDirectoryName(ScriptPath);
        if (!Directory.Exists(editorFolder))
        {
            Directory.CreateDirectory(editorFolder);
        }

        File.WriteAllText(ScriptPath, sb.ToString());
        AssetDatabase.Refresh();
    }
}