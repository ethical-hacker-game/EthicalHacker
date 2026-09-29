// This file is auto-generated. Do not edit it manually.
using UnityEditor;
using UnityEditor.SceneManagement;

public static class GeneratedSceneMenu
{
    [MenuItem("Scenes/population", priority = 0)]
    public static void Open_population_0()
    {
        // Ask user to save current scene if it has modifications
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_ASSETS/Unity3DGlobe/Scenes/population.unity");
        }
    }

    [MenuItem("Scenes/EditorScene", priority = 1)]
    public static void Open_EditorScene_1()
    {
        // Ask user to save current scene if it has modifications
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_GAME/Scenes/EditorScene.unity");
        }
    }

    [MenuItem("Scenes/GameGlobe", priority = 2)]
    public static void Open_GameGlobe_2()
    {
        // Ask user to save current scene if it has modifications
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_GAME/Scenes/GameGlobe.unity");
        }
    }

    [MenuItem("Scenes/MainMenu", priority = 3)]
    public static void Open_MainMenu_3()
    {
        // Ask user to save current scene if it has modifications
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_GAME/Scenes/MainMenu.unity");
        }
    }

}
