using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapInteractionRaycaster : MonoBehaviour, IPointerClickHandler
{
    public static MapInteractionRaycaster Instance { get; private set; }

    [SerializeField] private Camera globeCamera;
    [SerializeField] private LayerMask hitLayers = ~0;

    public RawImage MapImage { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (MapImage == null)
            MapImage = GetComponent<RawImage>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MapImage == null || globeCamera == null)
            return;

        RectTransform rectTransform = MapImage.rectTransform;
        if (rectTransform == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        Rect rect = rectTransform.rect;
        float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
            return;

        Vector2 uv = new Vector2(normalizedX, normalizedY);
        Rect uvRect = MapImage.uvRect;
        uv = new Vector2(Mathf.Lerp(uvRect.xMin, uvRect.xMax, uv.x), Mathf.Lerp(uvRect.yMin, uvRect.yMax, uv.y));

        Ray ray = globeCamera.ViewportPointToRay(new Vector3(uv.x, uv.y, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, globeCamera.farClipPlane, hitLayers))
            return;

        MapInteractionMarker marker = hit.collider.GetComponentInParent<MapInteractionMarker>();
        if (marker != null)
        {
            marker.ClickMe();
            return;
        }

        Debug.LogWarning("MapInteractionRaycaster: Hit object has no MapInteractionMarker.");
    }
}
