using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLayerChange : MonoBehaviour
{
    [SerializeField] private LayerMask selectedLayer;

    IEnumerator Start()
    {
        yield return null; 
        int layer = Mathf.RoundToInt(Mathf.Log(selectedLayer.value, 2));
        SetLayerRecursively(gameObject, layer);
    }

    public static void SetLayerRecursively(GameObject parentObject, int newLayer)
    {
        parentObject.layer = newLayer;

        foreach (Transform child in parentObject.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
