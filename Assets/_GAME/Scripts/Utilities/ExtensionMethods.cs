using UnityEngine;

public static class ExtensionMethods
{
    public static void DontDestroyOnLoadSafe(this GameObject obj)
    {
        obj.transform.SetParent(null);
        Object.DontDestroyOnLoad(obj);
    }
}
