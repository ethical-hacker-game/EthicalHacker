using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MapInteractionMarker : MonoBehaviour
{
    public UnityEvent OnClicked { get; } = new UnityEvent();

    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float shrinkDuration = 0.25f;

    [SerializeField] private MeshRenderer iconRenderer;
    [SerializeField] private Texture2D iconStory;
    [SerializeField] private Texture2D iconOther;

    [SerializeField] private bool removeOnClick = true;
    [SerializeField] private Renderer[] colorTargets;

    private bool isRemoving;
    private bool pulseEnabled = true;
    private float typeScaleMultiplier = 1f;

    private void Update()
    {
        if (isRemoving || !pulseEnabled)
        {
            return;
        }
        var t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        var scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = Vector3.one * (scale * typeScaleMultiplier);
    }

    public void SetRemoveOnClick(bool shouldRemove)
    {
        removeOnClick = shouldRemove;
    }

    public void SetPulseEnabled(bool shouldPulse)
    {
        pulseEnabled = shouldPulse;

        if (!pulseEnabled)
        {
            transform.localScale = Vector3.one * typeScaleMultiplier;
        }
    }

    public void ApplyMarkerType(bool isStory)
    {
        typeScaleMultiplier = isStory ? 1f : 0.7f;

        EnsureColorTargets();

        if (colorTargets != null && colorTargets.Length > 0)
        {
            Color color = GetMarkerColor(isStory);
            SetColor(color);
        }

        if (iconRenderer != null)
        {
            var iconTexture = GetMarkerTexture(isStory);
            if (iconTexture != null)
            {
                iconRenderer.material.mainTexture = iconTexture;
            }
        }
    }

    public void SetColor(Color color)
    {
        EnsureColorTargets();

        if (colorTargets != null && colorTargets.Length > 0)
        {
            for (int i = 0; i < colorTargets.Length; i++)
            {
                if (colorTargets[i] == null)
                    continue;

                colorTargets[i].material.color = color;
            }
        }
    }

    public void Remove()
    {
        if (isRemoving)
            return;

        StartCoroutine(ShrinkAndRemove());
    }

    public void ClickMe()
    {
        if (isRemoving)
        {
            return;
        }

        OnClicked?.Invoke();
        if (removeOnClick)
            StartCoroutine(ShrinkAndRemove());
    }

    private IEnumerator ShrinkAndRemove()
    {
        isRemoving = true;

        var startScale = transform.localScale;
        var duration = Mathf.Max(shrinkDuration, 0.01f);
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (shrinkDuration < 0.01f)
        {
            shrinkDuration = 0.01f;
        }
    }

    private void EnsureColorTargets()
    {
        if (colorTargets != null && colorTargets.Length > 0)
            return;

        colorTargets = GetComponentsInChildren<Renderer>(true);
    }

    private static Color GetMarkerColor(bool isStory)
    {
        return new Color(1f, 1f, 0f);
    }

    private Texture2D GetMarkerTexture(bool isStory)
    {
        return isStory ? iconStory : iconOther;
    }
}
