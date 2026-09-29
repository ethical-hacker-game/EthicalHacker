using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LayoutRebuilder : MonoBehaviour
{
    private RectTransform rectTransform;
    private bool forceRebuild = false;
    private int framesToWait = 0;

    public void ForceRebuild()
    {
        if (rectTransform == null)
            return;

        forceRebuild = true;
        framesToWait = 1;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ForceRebuild();
    }

    private void Update()
    {
        if (forceRebuild == false)
            return;

        if (framesToWait > 0)
        {
            framesToWait--;
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        forceRebuild = false;
    }
}